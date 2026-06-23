using AIKernel.CLI;
using System.IO.Compression;

namespace AIKernel.Tools.Tests;

public sealed class CliCommandSmokeTests
{
    [Fact]
    public void RuntimePingInvokesStandardMinimalRuntimeProvider()
    {
        var (exitCode, output) = Capture(() => Program.Main(["runtime", "ping"]));

        Assert.Equal(0, exitCode);
        Assert.Contains("capability: aikernel.runtime.ping", output);
        Assert.Contains("status: ok", output);
    }

    [Fact]
    public void SystemInfoPrintsStandardProviderSnapshot()
    {
        var (exitCode, output) = Capture(() => Program.Main(["system", "info"]));

        Assert.Equal(0, exitCode);
        Assert.Contains("\"providerCount\":", output);
        Assert.Contains("\"capabilityCount\":", output);
    }

    [Fact]
    public void CapabilitiesListShowsStandardCapabilityModules()
    {
        var (exitCode, output) = Capture(() => Program.Main(["capabilities", "list"]));

        Assert.Equal(0, exitCode);
        Assert.Contains("aikernel.runtime.ping", output);
        Assert.Contains("aikernel.system.info", output);
        Assert.Contains("aikernel.local.execute", output);
    }

    [Fact]
    public void ExecRunInvokesLocalExecutionProvider()
    {
        using var workspace = TemporaryWorkspace.Create();
        var pipelinePath = Path.Combine(workspace.Path, "pipeline.json");
        File.WriteAllText(
            pipelinePath,
            """
            {
              "type": "Pipeline",
              "steps": [
                { "type": "Step", "name": "start" }
              ]
            }
            """);

        var (exitCode, output) = Capture(() => Program.Main(["exec", "run", pipelinePath]));

        Assert.Equal(0, exitCode);
        Assert.Contains("capability: aikernel.local.execute", output);
        Assert.Contains("dsl.status: Succeeded", output);
    }

    [Fact]
    public void ProcessAndLogsCommandsExposeOsSurface()
    {
        var (runExit, runOutput) = Capture(() => Program.Main(["run", "sample"]));
        var (psExit, psOutput) = Capture(() => Program.Main(["ps"]));
        var (logsExit, logsOutput) = Capture(() => Program.Main(["logs", "sample"]));

        Assert.Equal(0, runExit);
        Assert.Equal(0, psExit);
        Assert.Equal(0, logsExit);
        Assert.Contains("process: sample", runOutput);
        Assert.Contains("Running sample", psOutput);
        Assert.Contains("sample: ProcessStarted", logsOutput);
    }

    [Fact]
    public void RunHaloWorldWasmCompletesAndWritesLogs()
    {
        using var workspace = TemporaryWorkspace.Create();
        var wasmPath = Path.Combine(workspace.Path, "HaloWorld.wasm");
        File.WriteAllBytes(wasmPath, Convert.FromBase64String("AGFzbQEAAAA="));

        var (runExit, runOutput) = Capture(() => Program.Main(["run", "haloworld", "--wasm", wasmPath]));
        var (psExit, psOutput) = Capture(() => Program.Main(["ps"]));
        var (logsExit, logsOutput) = Capture(() => Program.Main(["logs", "haloworld"]));

        Assert.Equal(0, runExit);
        Assert.Equal(0, psExit);
        Assert.Equal(0, logsExit);
        Assert.Contains("process: haloworld", runOutput);
        Assert.Contains("state: Stopped", runOutput);
        Assert.Contains("stdout: HaloWorld", runOutput);
        Assert.Contains("Stopped haloworld", psOutput);
        Assert.Contains("haloworld: Stdout HaloWorld", logsOutput);
        Assert.Contains("haloworld: ProcessStopped", logsOutput);
    }

    [Fact]
    public void GpuRunVectorAddWritesOutput()
    {
        using var workspace = TemporaryWorkspace.Create();
        var leftPath = Path.Combine(workspace.Path, "a.bin");
        var rightPath = Path.Combine(workspace.Path, "b.bin");
        var outputPath = Path.Combine(workspace.Path, "out.bin");
        File.WriteAllBytes(leftPath, FloatsToBytes([1.0f, 2.0f]));
        File.WriteAllBytes(rightPath, FloatsToBytes([3.0f, 4.0f]));

        var (exitCode, output) = Capture(() => Program.Main([
            "gpu", "run", "vector-add",
            "--a", leftPath,
            "--b", rightPath,
            "--out", outputPath
        ]));

        Assert.Equal(0, exitCode);
        Assert.Contains("operation: vector-add", output);
        Assert.Equal([4.0f, 6.0f], BytesToFloats(File.ReadAllBytes(outputPath)));
    }

    [Fact]
    public void GpuVerifyRev3PrintsCanonicalGpuBoundaryRules()
    {
        var (exitCode, output) = Capture(() => Program.Main(["gpu", "verify", "rev3"]));

        Assert.Equal(0, exitCode);
        Assert.Contains("AIKernel GPU canonical contract: v0.1.3 rev3", output);
        Assert.Contains("validation: ok", output);
        Assert.Contains("operations-rule: canonical WebGPU operations are compute.dispatch, compute.vector_add, gpu.hud.composite, gpu.aisthesis.raw-frame, gpu.spatial-reasoning, gpu.zero-copy.raw-texture.", output);
        Assert.Contains("permissions-rule: canonical WebGPU permissions are compute.execute, buffer.read, buffer.write, texture.bind, texture.write.", output);
        Assert.Contains("layout: HudPanelRect; stride=12", output);
        Assert.Contains("layout: StateVector; stride=16", output);
        Assert.Contains("native-abi-probe-validation: dawn=ok; cuda=ok", output);
        Assert.Contains("control-arbitration-metadata-validation: ok", output);
        Assert.Contains("browser-bridge-metadata-validation: ok", output);
        Assert.Contains("execution-layer-metadata-validation: ok", output);
        Assert.Contains("frame-diagnostics-validation: ok", output);
        Assert.Contains("frame-diagnostics-sibling-validation: webgpu=ok; dawn=ok; cuda=ok", output);
        Assert.Contains("dynamic-pipeline-dsl-rule: DynamicPipelineCompilerProvider supports boolean-v1 parenthesized AND/OR/NOT guards", output);
        Assert.Contains("provider-sibling-rule: WebGPUProvider, DawnProvider, and Cuda13Provider are canonical rev3 sibling providers", output);
        Assert.Contains("provider-sibling-metadata-rule: sibling providers should expose provider_family, provider_role, gpu_backend, and gpu_capabilities metadata", output);
        Assert.Contains("provider-boundary-rule: backend-specific adapter, device, driver, and native ABI connection logic stays in provider packages, not Core", output);
        Assert.Contains("pass-bridge-rule: browser WebGPU and Dawn providers should consume WebGpuRev3DispatchEnvelope", output);
        Assert.Contains("dispatch-envelope-validation-rule: browser WebGPU and Dawn bridge handoff should call WebGpuRev3InteropEnvelope.ValidateDispatchEnvelope", output);
        Assert.Contains("browser-bridge-rule: AIKernel.Wasm packages runtime/browser/webgpu-rev3-envelope-bridge.js", output);
        Assert.Contains("browser-bridge-value-catalog-rule: packaged browser bridges should mirror canonical rev3 diagnostics values", output);
        Assert.Contains("browser-executor-rule: createWebGpuRev3BrowserExecutor owns browser WebGPU device lifecycle", output);
        Assert.Contains("execution-layer-metadata-rule: WebGPUProvider, DawnProvider, and Cuda13Provider should expose gpu_bypass, native_js_bridge, aot_compiler_hooks", output);
        Assert.Contains("execution-layer-spoofing-rule: providers should preserve non-canonical caller metadata but overwrite backend, gpu_backend, gpu_bypass", output);
        Assert.Contains("promotion-readiness-evaluator: gate=trace-candidate; authoritative=false; candidate=false; diagnostic=true; blocked=true; reason=storage-texture-not-ready", output);
        Assert.Contains("frame-token-rule: every rev3 GPU dispatch envelope should preserve GpuFrameToken.FrameIndex and GpuFrameToken.SampleTicks", output);
        Assert.Contains("pass-executor-rule: browser and Doom hosts attach concrete Aisthesis, Spatial, and HUD command encoders through passExecutors", output);
        Assert.Contains("builtin-pass-rule: the browser executor can dispatch built-in Aisthesis, Spatial, and HUD compute passes", output);
        Assert.Contains("hud-rule: GPU HUD uses HudPanelRect stride=12 and HudPanelStateVector stride=16", output);
        Assert.Contains("aisthesis-rule: GPU Aisthesis consumes a zero-copy raw target", output);
        Assert.Contains("feature-mask-storage-rule: browser Aisthesis built-ins should emit FeatureMask as a GPU storage texture", output);
        Assert.Contains("diagnostics-rule: providers should expose Game, Bonsai, HUD, and Sensor paths through IGpuDiagnostics", output);
        Assert.Contains("path-role-resolver-rule: providers should classify pass ids through GpuRev3PathRoles.TryResolveFromPassId", output);
        Assert.Contains("diagnostics-metadata-rule: pilot paths should expose rev3_pilot_state, rev3_promotion_gate", output);
        Assert.Contains("diagnostics-value-catalog-rule: rev3 diagnostics metadata values are canonicalized for rev3_execution_mode, rev3_path_role, rev3_pilot_state, and rev3_promotion_gate", output);
        Assert.Contains("frame-diagnostics-metadata-rule: Game, Bonsai, HUD, and Sensor diagnostics should carry rev3_pass_readiness", output);
        Assert.Contains("rev3_frame_index, rev3_sample_ticks", output);
        Assert.Contains("browser-diagnostics-rule: Doom/browser HUD status rows should preserve rev3_pass_id, rev3_path_role, rev3_pilot_state", output);
        Assert.Contains("browser-execution-metadata-rule: browser executors should expose rev3_execution_mode", output);
        Assert.Contains("browser-pass-readiness-rule: browser executors should expose Passes.Aisthesis, Passes.SpatialReasoning, and Passes.HudComposite", output);
        Assert.Contains("browser-pass-readiness-metadata-rule: Doom/browser status rows should project pass readiness into rev3_pass_readiness metadata", output);
        Assert.Contains("control-arbitration-rule: Control GPU arbitration should expose rev3_pass_id, rev3_execution_mode, backend, and fallback metadata", output);
        Assert.Contains("control-arbitration-metadata-rule: Control GPU diagnostics should expose control.intent_id, control.objective_id, control.action_id", output);
        Assert.Contains("control.gpu.missing_capabilities", output);
        Assert.Contains("approved mode is control-gpu-approved; fallback mode is control-cpu-fallback", output);
        Assert.Contains("control-arbitration-fallback-rule: stable Control GPU fallback reasons include cpu-forced, gpu-lost-one-way-fallback", output);
        Assert.Contains("native-validation-rule: Dawn and OSNative providers should honor GpuProviderOptions.EnableNativeValidation", output);
        Assert.Contains("native-abi-probe-rule: DawnProvider and Cuda13Provider should expose deterministic native ABI probe metadata", output);
        Assert.Contains("dawn-native-probe-rule: DawnProvider probe metadata should include dawn.native.abi.available", output);
        Assert.Contains("cuda-native-probe-rule: Cuda13Provider probe metadata should include cuda.native.abi.available", output);
        Assert.Contains("native-dispatch-metadata-rule: DawnProvider and Cuda13Provider invocation/diagnostics metadata should expose native dispatch ABI version", output);
        Assert.Contains("DeviceUnavailable from CommandSubmissionDisabled", output);
        Assert.Contains("native-artifact-integrity-rule: aik gpu verify-native validates DawnCore exports", output);
        Assert.Contains("Cuda13 package runtime assets, and the Cuda13 staged dispatch probe", output);
        Assert.Contains("staged bridges must distinguish DeviceUnavailable from CommandSubmissionDisabled", output);
        Assert.Contains("remaining-work-priority: P0=browser/Doom live GPU promotion gates and Cuda13 dependency-staged direct ABI smoke; P1=Dawn/Cuda13 native command encoders; P2=none after rev3 local NuGet config propagation", output);
        Assert.Contains("promotion-gate-policy: browser WebGPU built-in passes stay diagnostic until live trace parity has stable ready streaks", output);
        Assert.Contains("native-provider-policy: DawnProvider and Cuda13Provider stay fail-closed descriptor/probe surfaces", output);
    }

    [Fact]
    public void HelpCommandListsRev3GpuNativeVerificationExamples()
    {
        var (exitCode, output) = Capture(() => Program.Main(["help"]));

        Assert.Equal(0, exitCode);
        Assert.Contains("aik gpu pending rev3", output);
        Assert.Contains("aik gpu verify-native --provider dawn --package provider.nupkg", output);
        Assert.Contains("aik gpu verify-native --provider cuda13 --package provider.nupkg", output);
        Assert.Contains("aik gpu verify-native --provider cuda13 --library native-library", output);
    }

    [Fact]
    public void GpuPendingRev3PrintsPrioritizedRemainingWork()
    {
        var (exitCode, output) = Capture(() => Program.Main(["gpu", "pending", "rev3"]));

        Assert.Equal(0, exitCode);
        Assert.Contains("AIKernel GPU canonical pending work: v0.1.3 rev3", output);
        Assert.Contains("pending: P0; browser-doom-live-gpu-promotion; gate=live-trace-parity", output);
        Assert.Contains("pending: P0; cuda13-direct-abi-smoke; gate=native-library-load", output);
        Assert.Contains("pending: P1; dawn-native-command-encoders; gate=native-resource-mapping", output);
        Assert.Contains("pending: P1; cuda13-native-command-encoders; gate=native-cuda-abi", output);
        Assert.Contains("closed: P2; release-lane-local-feed-bootstrap; gate=local-nuget-config-propagated", output);
        Assert.Contains("policy: diagnostic-only pilots must stay fail-closed", output);
    }

    [Fact]
    public void GpuVerifyNativeChecksPackageRuntimeEntries()
    {
        using var workspace = TemporaryWorkspace.Create();
        var packagePath = Path.Combine(workspace.Path, "AIKernel.Dawn.Provider.0.1.3-dev1.nupkg");
        using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create))
        {
            archive.CreateEntry("runtimes/win-x64/native/custom_bridge.dll");
            archive.CreateEntry("runtimes/linux-x64/native/libcustom_bridge.so");
        }

        var (exitCode, output) = Capture(() => Program.Main([
            "gpu",
            "verify-native",
            "--package",
            packagePath
        ]));

        Assert.Equal(0, exitCode);
        Assert.Contains("package-entry: runtimes/win-x64/native/custom_bridge.dll ok", output);
        Assert.Contains("package-entry: runtimes/linux-x64/native/libcustom_bridge.so ok", output);
        Assert.Contains("library-check: skipped", output);
        Assert.Contains("native-artifact-integrity: ok", output);
    }

    [Fact]
    public void GpuVerifyNativeChecksCuda13PackageRuntimeEntries()
    {
        using var workspace = TemporaryWorkspace.Create();
        var packagePath = Path.Combine(workspace.Path, "AIKernel.Cuda13.0.Libtorch2.12.win-x64.0.1.3-dev1.nupkg");
        using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create))
        {
            archive.CreateEntry("loader.json");
            archive.CreateEntry("contentFiles/any/any/loader.json");
            archive.CreateEntry("runtimes/win-x64/native/libtorch_bridge.dll");
        }

        var (exitCode, output) = Capture(() => Program.Main([
            "gpu",
            "verify-native",
            "--provider",
            "cuda13",
            "--package",
            packagePath
        ]));

        Assert.Equal(0, exitCode);
        Assert.Contains("native-artifact-integrity: start; provider=cuda13", output);
        Assert.Contains("package-entry: loader.json ok", output);
        Assert.Contains("package-entry: contentFiles/any/any/loader.json ok", output);
        Assert.Contains("package-entry: runtimes/win-x64/native/libtorch_bridge.dll ok", output);
        Assert.Contains("package-provider: cuda13", output);
        Assert.Contains("library-check: skipped", output);
        Assert.Contains("native-artifact-integrity: ok", output);
    }

    [Fact]
    public void GpuVerifyNativeChecksBuiltDawnFixtureWhenAvailable()
    {
        var libraryPath = TryResolveBuiltDawnBridgeLibraryPath();
        if (libraryPath is null)
        {
            return;
        }

        var (exitCode, output) = Capture(() => Program.Main([
            "gpu",
            "verify-native",
            "--library",
            libraryPath
        ]));

        Assert.Equal(0, exitCode);
        Assert.Contains("export: InitializeDawnNative ok", output);
        Assert.Contains("export: aikernel_dawn_dispatch ok", output);
        Assert.Contains("state: abi=1 status=CpuFallback cpu_fallback=true adapter=false device=false", output);
        Assert.Contains("dispatch-state: abi=1 status=CpuFallback cpu_fallback=true adapter=false device=false", output);
        Assert.Contains("dawn-dispatch-response: abi=1 status=CpuFallback failure=CpuFallback frame=42 ticks=1001 diagnostics=0", output);
        Assert.Contains("dawn-dispatch-unknown-pass: failure=UnknownPass", output);
        Assert.Contains("dawn-dispatch-invalid-length: failure=InvalidRequestLength", output);
        Assert.Contains("package-check: skipped", output);
        Assert.Contains("native-artifact-integrity: ok", output);
    }

    [Fact]
    public void GpuVerifyNativeChecksBuiltCuda13FixtureWhenAvailable()
    {
        var libraryPath = TryResolveBuiltCuda13BridgeLibraryPath();
        if (libraryPath is null)
        {
            return;
        }

        var (exitCode, output) = Capture(() => Program.Main([
            "gpu",
            "verify-native",
            "--provider",
            "cuda13",
            "--library",
            libraryPath
        ]));

        if (exitCode != 0 && output.Contains("library: load-failed", StringComparison.Ordinal))
        {
            return;
        }

        Assert.Equal(0, exitCode);
        Assert.Contains("export: load_model ok", output);
        Assert.Contains("export: unload_model ok", output);
        Assert.Contains("export: forward ok", output);
        Assert.Contains("export: aikernel_cuda13_dispatch ok", output);
        Assert.Contains("cuda-dispatch-response: abi=1 status=NotInitialized failure=CommandSubmissionDisabled frame=1 ticks=100 diagnostics=0", output);
        Assert.Contains("cuda-dispatch-invalid-length: failure=InvalidRequestLength", output);
        Assert.Contains("package-check: skipped", output);
        Assert.Contains("native-artifact-integrity: ok", output);
    }

    [Fact]
    public void ScheduleCommandAddsAndListsCommand()
    {
        var (addExit, addOutput) = Capture(() => Program.Main(["schedule", "add", "--every", "1s", "aik gpu run vector-add"]));
        var (listExit, listOutput) = Capture(() => Program.Main(["schedule", "list"]));

        Assert.Equal(0, addExit);
        Assert.Equal(0, listExit);
        Assert.Contains("every: 1s", addOutput);
        Assert.Contains("aik gpu run vector-add", listOutput);
    }

    [Fact]
    public void ConceptAliasesExposeRomAndTimelineInspectors()
    {
        var (romExit, romOutput) = Capture(() => Program.Main(["rom", "view"]));
        var (nomosExit, nomosOutput) = Capture(() => Program.Main(["nomos", "view"]));
        var (chronosExit, chronosOutput) = Capture(() => Program.Main(["chronos", "timeline"]));
        var (replayExit, replayOutput) = Capture(() => Program.Main(["replay", "timeline"]));

        Assert.Equal(0, romExit);
        Assert.Equal(0, nomosExit);
        Assert.Equal(0, chronosExit);
        Assert.Equal(0, replayExit);
        Assert.Contains("rom.viewer: nomos", romOutput);
        Assert.Contains("rom.alias: aik nomos view", nomosOutput);
        Assert.Contains("timeline[0].event: kernel.clock.inspect", chronosOutput);
        Assert.Contains("timeline[0].event: kernel.clock.inspect", replayOutput);
    }

    private static (int ExitCode, string Output) Capture(Func<int> action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        try
        {
            Console.SetOut(writer);
            var exitCode = action();
            return (exitCode, writer.ToString());
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    private static byte[] FloatsToBytes(float[] values)
        => System.Runtime.InteropServices.MemoryMarshal.AsBytes<float>(values.AsSpan()).ToArray();

    private static float[] BytesToFloats(byte[] values)
        => System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(values).ToArray();

    private static string? TryResolveBuiltDawnBridgeLibraryPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var windowsCandidate = Path.Combine(
                current.FullName,
                "AIKernel.Dawn",
                "native",
                "DawnCore",
                "build_windows",
                "custom_bridge.dll");
            var linuxCandidate = Path.Combine(
                current.FullName,
                "AIKernel.Dawn",
                "native",
                "DawnCore",
                "build_linux",
                "libcustom_bridge.so");

            if (OperatingSystem.IsWindows() && File.Exists(windowsCandidate))
            {
                return windowsCandidate;
            }

            if (OperatingSystem.IsLinux() && File.Exists(linuxCandidate))
            {
                return linuxCandidate;
            }

            current = current.Parent;
        }

        return null;
    }

    private static string? TryResolveBuiltCuda13BridgeLibraryPath()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(
                current.FullName,
                "AIKernel.Cuda13.0",
                "native",
                "build",
                "win-x64",
                "Release",
                "libtorch_bridge.dll");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return null;
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private TemporaryWorkspace(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryWorkspace Create()
        {
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "aikernel-tools-cli-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TemporaryWorkspace(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
