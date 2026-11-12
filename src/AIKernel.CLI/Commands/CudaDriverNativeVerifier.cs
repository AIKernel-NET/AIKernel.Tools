namespace AIKernel.CLI.Commands;

using System.Buffers.Binary;
using System.IO.Compression;
using System.Runtime.InteropServices;

// This verifier only calls ABI introspection and explicit host operations (null context).
// It never discovers a device, creates a CUDA context, or reads LibTorch configuration.
internal static class CudaDriverNativeVerifier
{
    private const string WindowsAsset = "runtimes/win-x64/native/aikernel_cuda_driver.dll";
    private const string LinuxAsset = "runtimes/linux-x64/native/libaikernel_cuda_driver.so";
    private static readonly string[] Exports =
    [
        "ak_abi_version", "ak_get_device_count", "ak_create", "ak_destroy", "ak_get_stats",
        "ak_device_alloc", "ak_device_free", "ak_pinned_alloc", "ak_pinned_free",
        "ak_upload", "ak_upload_shared", "ak_wipe_device", "ak_wipe_scratch", "ak_get_pci_bus_id",
        "ak_download", "ak_gemv", "ak_normalize", "ak_rope", "ak_swiglu", "ak_softmax", "ak_attention_tile"
    ];

    internal static bool ValidatePackage(string path)
    {
        try
        {
            using var package = ZipFile.OpenRead(path);
            if (package.Entries.GroupBy(entry => entry.FullName, StringComparer.Ordinal).Any(group => group.Count() != 1))
                return Failure("package: duplicate-entry");
            foreach (var name in new[] { WindowsAsset, LinuxAsset, "lib/net10.0/AIKernel.Kernel.dll", "licenses/cuda-driver/LICENSE" })
            {
                var entry = package.GetEntry(name);
                if (entry is null || entry.Length == 0) return Failure($"package-entry: {name} missing-or-empty");
                if (name == WindowsAsset && !IsX64Image(entry, windows: true)) return Failure($"package-entry: {name} invalid-image");
                if (name == LinuxAsset && !IsX64Image(entry, windows: false)) return Failure($"package-entry: {name} invalid-image");
                Console.WriteLine($"package-entry: {name} ok");
            }
            if (package.Entries.Any(entry => entry.FullName.Contains("torch", StringComparison.OrdinalIgnoreCase)))
                return Failure("package: unexpected-torch-artifact");
            Console.WriteLine("package-provider: cuda-driver; abi-execution-check: requires --library");
            return true;
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException)
        {
            return Failure($"package: invalid; error={ex.GetType().Name}");
        }
    }

    private static bool IsX64Image(ZipArchiveEntry entry, bool windows)
    {
        using var stream = entry.Open();
        Span<byte> header = stackalloc byte[64];
        stream.ReadExactly(header);
        if (!windows)
            return header[..6].SequenceEqual(new byte[] { 0x7f, (byte)'E', (byte)'L', (byte)'F', 2, 1 })
                && BinaryPrimitives.ReadUInt16LittleEndian(header[18..]) == 62;
        if (header[0] != 'M' || header[1] != 'Z') return false;
        var offset = BinaryPrimitives.ReadInt32LittleEndian(header[60..]);
        if (offset < 64 || offset > 1024 * 1024 || offset + 6 > entry.Length) return false;
        Span<byte> discard = stackalloc byte[256];
        for (var remaining = offset - 64; remaining > 0;)
        {
            var length = Math.Min(remaining, discard.Length);
            stream.ReadExactly(discard[..length]);
            remaining -= length;
        }
        stream.ReadExactly(header[..6]);
        return header[..4].SequenceEqual(new byte[] { (byte)'P', (byte)'E', 0, 0 })
            && BinaryPrimitives.ReadUInt16LittleEndian(header[4..]) == 0x8664;
    }

    internal static bool ValidateLibrary(string path)
    {
        nint library = 0;
        try
        {
            if (!File.Exists(path)) return Failure("library: missing; provider=cuda-driver");
            library = NativeLibrary.Load(Path.GetFullPath(path));
            var exports = new Dictionary<string, nint>(StringComparer.Ordinal);
            foreach (var name in Exports)
            {
                if (!NativeLibrary.TryGetExport(library, name, out var address)) return Failure($"export: {name} missing");
                exports.Add(name, address);
                Console.WriteLine($"export: {name} ok");
            }
            var version = Marshal.GetDelegateForFunctionPointer<AbiVersion>(exports["ak_abi_version"])();
            if (version != 1) return Failure($"cuda-driver-abi: unsupported; actual={version}; expected=1");
            Console.WriteLine("cuda-driver-abi: 1");
            var softmax = Marshal.GetDelegateForFunctionPointer<Softmax>(exports["ak_softmax"]);
            var values = Marshal.AllocHGlobal(2 * sizeof(float));
            try
            {
                Marshal.Copy(new float[] { 3, 1 }, 0, values, 2);
                if (softmax(0, values, 2) != 0) return Failure("cuda-driver-host-softmax: failed");
                var actual = new float[2];
                Marshal.Copy(values, actual, 0, 2);
                var expected = new[] { 1f / (1f + MathF.Exp(-2)), 1f / (1f + MathF.Exp(2)) };
                if (actual.Where((value, index) => !float.IsFinite(value) || MathF.Abs(value - expected[index]) > 1e-5f).Any())
                    return Failure("cuda-driver-host-softmax: invalid-result");
                if (softmax(0, 0, 0) != 1) return Failure("cuda-driver-invalid-arguments: not-rejected");
            }
            finally { Marshal.FreeHGlobal(values); }
            Console.WriteLine("cuda-driver-host-softmax: ok; invalid-arguments: rejected; gpu-test: not-run");
            Console.WriteLine("library-provider: cuda-driver");
            return true;
        }
        catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException or EntryPointNotFoundException
            or MarshalDirectiveException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            return Failure($"library: invalid; provider=cuda-driver; error={ex.GetType().Name}");
        }
        finally { if (library != 0) NativeLibrary.Free(library); }
    }

    private static bool Failure(string message) { Console.WriteLine(message); return false; }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate uint AbiVersion();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Softmax(nint context, nint values, uint count);
}
