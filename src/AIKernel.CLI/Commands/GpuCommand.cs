namespace AIKernel.CLI.Commands;

using AIKernel.Control.GPU;
using AIKernel.Common.Results;
using AIKernel.Dtos.Gpu;
using AIKernel.Enums;
using System.IO.Compression;
using System.Runtime.InteropServices;

/// <summary>
/// [EN] Handles OS GPU commands.
/// [JA] OS GPU command を処理します。
/// </summary>
public static class GpuCommand
{
    /// <summary>[EN] Runs a GPU command. [JA] GPU command を実行します。</summary>
    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            ShowUsage();
            return 0;
        }

        var result = args[0].ToLowerInvariant() switch
        {
            "list" => Result<int>.Success(List()),
            "verify" => Result<int>.Success(Verify(args.Skip(1).ToArray())),
            "verify-native" => Result<int>.Success(VerifyNative(args.Skip(1).ToArray())),
            "pending" => Result<int>.Success(Pending(args.Skip(1).ToArray())),
            "run" => RunKernel(args.Skip(1).ToArray()),
            _ => Result<int>.Success(HelpCommand.Unknown("gpu " + args[0]))
        };
        return ExitCode(result, "GPU command failed");
    }

    private static int List()
    {
        Console.WriteLine("provider: cpu");
        Console.WriteLine("provider: webgpu");
        Console.WriteLine("fallback: cpu");
        return 0;
    }

    private static int Pending(string[] args)
    {
        if (args.Length > 0 &&
            !string.Equals(args[0], "rev3", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(args[0], "0.1.3", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: aik gpu pending rev3");
            return 1;
        }

        Console.WriteLine("AIKernel GPU canonical pending work: v0.1.3 rev3");
        Console.WriteLine("pending: P0; browser-doom-live-gpu-promotion; gate=live-trace-parity; unblock=stable ready streaks for Aisthesis plus HUD presented-path parity");
        Console.WriteLine("pending: P0; cuda13-direct-abi-smoke; gate=native-library-load; unblock=stage LibTorch/CUDA dependencies and rebuild fresh libtorch_bridge.dll");
        Console.WriteLine("pending: P1; dawn-native-command-encoders; gate=native-resource-mapping; unblock=map frame targets and buffers to Dawn command encoders after staged ABI response bridge");
        Console.WriteLine("pending: P1; cuda13-native-command-encoders; gate=native-cuda-abi; unblock=device-buffer dispatch and command submission beyond probe mode");
        Console.WriteLine("closed: P2; release-lane-local-feed-bootstrap; gate=local-nuget-config-propagated; evidence=verify-rev3-gpu-local-lane uses generated rev3 NuGet config");
        Console.WriteLine("policy: diagnostic-only pilots must stay fail-closed until authoritative_ready=true and canonical diagnostics prove zero-copy/raw capture.");
        return 0;
    }

    private static int Verify(string[] args)
    {
        if (args.Length > 0 &&
            !string.Equals(args[0], "rev3", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(args[0], "0.1.3", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: aik gpu verify rev3");
            return 1;
        }

        var required = GpuProviderCapabilities.SupportsCompute |
            GpuProviderCapabilities.SupportsHudComposite |
            GpuProviderCapabilities.SupportsAisthesis |
            GpuProviderCapabilities.SupportsSpatialReasoning |
            GpuProviderCapabilities.SupportsZeroCopyRawTexture |
            GpuProviderCapabilities.SupportsFrameDiagnostics;
        var nativeProbeValidation = ValidateCanonicalNativeProbeMetadataSamples(
            out var dawnNativeProbeValid,
            out var cudaNativeProbeValid);
        var controlArbitrationValidation = ValidateCanonicalControlArbitrationMetadataSample(
            out var controlArbitrationMetadataValid);
        var browserBridgeValidation = ValidateCanonicalBrowserBridgeMetadataSample(
            out var browserBridgeMetadataValid);
        var executionLayerValidation = ValidateCanonicalExecutionLayerMetadataSamples(
            out var executionLayerMetadataValid);
        var frameDiagnosticsValidation = ValidateCanonicalFrameDiagnosticsSample(
            out var frameDiagnosticsValid,
            out var webGpuFrameDiagnosticsValid,
            out var dawnFrameDiagnosticsValid,
            out var cudaFrameDiagnosticsValid,
            out var rev3PromotionReadiness);
        var validation = GpuCanonicalValidation
            .ValidateLayouts(GpuCanonicalLayouts.All)
            .Merge(GpuCanonicalValidation.ValidateCapabilities(required, required))
            .Merge(nativeProbeValidation)
            .Merge(controlArbitrationValidation)
            .Merge(browserBridgeValidation)
            .Merge(executionLayerValidation)
            .Merge(frameDiagnosticsValidation);

        Console.WriteLine("AIKernel GPU canonical contract: v0.1.3 rev3");
        Console.WriteLine($"validation: {(validation.IsValid ? "ok" : "failed")}");
        foreach (var issue in validation.Issues)
        {
            Console.WriteLine($"issue: {issue.Severity}; {issue.Code}; {issue.Path}; {issue.Message}");
        }

        if (!validation.IsValid)
        {
            return 1;
        }

        Console.WriteLine($"required-capabilities: {required}");
        Console.WriteLine($"native-abi-probe-validation: dawn={(dawnNativeProbeValid ? "ok" : "failed")}; cuda={(cudaNativeProbeValid ? "ok" : "failed")}");
        Console.WriteLine($"control-arbitration-metadata-validation: {(controlArbitrationMetadataValid ? "ok" : "failed")}");
        Console.WriteLine($"browser-bridge-metadata-validation: {(browserBridgeMetadataValid ? "ok" : "failed")}");
        Console.WriteLine($"execution-layer-metadata-validation: {(executionLayerMetadataValid ? "ok" : "failed")}");
        Console.WriteLine($"frame-diagnostics-validation: {(frameDiagnosticsValid ? "ok" : "failed")}");
        Console.WriteLine($"frame-diagnostics-sibling-validation: webgpu={(webGpuFrameDiagnosticsValid ? "ok" : "failed")}; dawn={(dawnFrameDiagnosticsValid ? "ok" : "failed")}; cuda={(cudaFrameDiagnosticsValid ? "ok" : "failed")}");
        Console.WriteLine($"promotion-readiness-evaluator: gate={rev3PromotionReadiness.Gate}; authoritative={Flag(rev3PromotionReadiness.IsAuthoritativeReady)}; candidate={Flag(rev3PromotionReadiness.IsPromotionCandidate)}; diagnostic={Flag(rev3PromotionReadiness.IsDiagnosticReady)}; blocked={Flag(rev3PromotionReadiness.IsBlocked)}; reason={rev3PromotionReadiness.Reason}; streak={rev3PromotionReadiness.CandidateStreak}/{rev3PromotionReadiness.RequiredStreak}");
        Console.WriteLine($"operations-rule: canonical WebGPU operations are {string.Join(", ", GpuOperationNames.WebGpuComputeProviderOperations)}.");
        Console.WriteLine($"permissions-rule: canonical WebGPU permissions are {string.Join(", ", GpuPermissionNames.WebGpuComputeRequiredPermissions)}.");
        foreach (var layout in GpuCanonicalLayouts.All)
        {
            Console.WriteLine($"layout: {layout.Name}; stride={layout.Stride}; max={layout.MaxItems}");
        }

        Console.WriteLine("raw-capture-rule: sensor analysis must bind RawFramebuffer, not HUD composite or display fallback.");
        Console.WriteLine("spatial-matrix-rule: AisMatrix order is Topos, Route, Threat, Zoe; CTG belongs to StateVector.");
        Console.WriteLine("dynamic-pipeline-dsl-rule: DynamicPipelineCompilerProvider supports boolean-v1 parenthesized AND/OR/NOT guards for route, profile, and GPU arbitration pipelines.");
        Console.WriteLine("provider-sibling-rule: WebGPUProvider, DawnProvider, and Cuda13Provider are canonical rev3 sibling providers that share DTOs, pass ids, metadata keys, and diagnostics vocabulary.");
        Console.WriteLine($"provider-sibling-metadata-rule: sibling providers should expose {GpuProviderMetadataKeys.ProviderFamily}, {GpuProviderMetadataKeys.ProviderRole}, {GpuProviderMetadataKeys.GpuBackend}, and {GpuProviderMetadataKeys.GpuCapabilities} metadata.");
        Console.WriteLine("provider-boundary-rule: backend-specific adapter, device, driver, and native ABI connection logic stays in provider packages, not Core.");
        Console.WriteLine("pass-bridge-rule: browser WebGPU and Dawn providers should consume WebGpuRev3DispatchEnvelope before DTO direct-dispatch.");
        Console.WriteLine("dispatch-envelope-validation-rule: browser WebGPU and Dawn bridge handoff should call WebGpuRev3InteropEnvelope.ValidateDispatchEnvelope before command encoder execution.");
        Console.WriteLine("browser-bridge-rule: AIKernel.Wasm packages runtime/browser/webgpu-rev3-envelope-bridge.js as the thin JS envelope adapter.");
        Console.WriteLine("browser-bridge-value-catalog-rule: packaged browser bridges should mirror canonical rev3 diagnostics values through a local JS value table and keep it covered by package tests.");
        Console.WriteLine("browser-executor-rule: createWebGpuRev3BrowserExecutor owns browser WebGPU device lifecycle, device-lost fallback, shader cache, and raw texture validation.");
        Console.WriteLine($"execution-layer-metadata-rule: WebGPUProvider, DawnProvider, and Cuda13Provider should expose {GpuProviderMetadataKeys.GpuBypass}, {GpuProviderMetadataKeys.NativeJsBridge}, {GpuProviderMetadataKeys.AotCompilerHooks}, {GpuProviderMetadataKeys.DeterministicFrameSampling}, and {GpuProviderMetadataKeys.ZeroCopyBufferHandling} metadata.");
        Console.WriteLine($"execution-layer-spoofing-rule: providers should preserve non-canonical caller metadata but overwrite {GpuProviderMetadataKeys.Backend}, {GpuProviderMetadataKeys.GpuBackend}, {GpuProviderMetadataKeys.GpuBypass}, {GpuProviderMetadataKeys.NativeJsBridge}, {GpuProviderMetadataKeys.PassBridge}, and {GpuProviderMetadataKeys.ZeroCopyBufferHandling} with canonical provider values.");
        Console.WriteLine("frame-token-rule: every rev3 GPU dispatch envelope should preserve GpuFrameToken.FrameIndex and GpuFrameToken.SampleTicks for deterministic sampling diagnostics.");
        Console.WriteLine("pass-executor-rule: browser and Doom hosts attach concrete Aisthesis, Spatial, and HUD command encoders through passExecutors.");
        Console.WriteLine("builtin-pass-rule: the browser executor can dispatch built-in Aisthesis, Spatial, and HUD compute passes when shaders and GPU resources are bound; HUD may still use host-composed fallback.");
        Console.WriteLine("hud-rule: GPU HUD uses HudPanelRect stride=12 and HudPanelStateVector stride=16; text labels stay in the lightweight overlay.");
        Console.WriteLine("aisthesis-rule: GPU Aisthesis consumes a zero-copy raw target and emits FeatureVector plus optional FeatureMask.");
        Console.WriteLine($"feature-mask-storage-rule: browser Aisthesis built-ins should emit FeatureMask as a GPU storage texture and surface {GpuDiagnosticsMetadataKeys.Rev3FeatureMaskStorageTexture} metadata when the compute path owns it.");
        Console.WriteLine("diagnostics-rule: providers should expose Game, Bonsai, HUD, and Sensor paths through IGpuDiagnostics.");
        Console.WriteLine("path-role-resolver-rule: providers should classify pass ids through GpuRev3PathRoles.TryResolveFromPassId and preserve unknown for unrecognized pass ids.");
        Console.WriteLine($"diagnostics-metadata-rule: pilot paths should expose {GpuDiagnosticsMetadataKeys.Rev3PilotState}, {GpuDiagnosticsMetadataKeys.Rev3PromotionGate}, {GpuDiagnosticsMetadataKeys.Rev3CandidateStreak}, {GpuDiagnosticsMetadataKeys.Rev3RequiredStreak}, {GpuDiagnosticsMetadataKeys.Rev3AuthoritativeReady}, and {GpuDiagnosticsMetadataKeys.Rev3DiagnosticReady}.");
        Console.WriteLine($"diagnostics-value-catalog-rule: rev3 diagnostics metadata values are canonicalized for {GpuDiagnosticsMetadataKeys.Rev3ExecutionMode}, {GpuDiagnosticsMetadataKeys.Rev3PathRole}, {GpuDiagnosticsMetadataKeys.Rev3PilotState}, and {GpuDiagnosticsMetadataKeys.Rev3PromotionGate}.");
        Console.WriteLine($"frame-diagnostics-metadata-rule: Game, Bonsai, HUD, and Sensor diagnostics should carry {GpuDiagnosticsMetadataKeys.Rev3PassReadiness}, {GpuDiagnosticsMetadataKeys.Rev3PathRole}, {GpuDiagnosticsMetadataKeys.Rev3FrameIndex}, {GpuDiagnosticsMetadataKeys.Rev3SampleTicks}, and execution-layer metadata.");
        Console.WriteLine($"browser-diagnostics-rule: Doom/browser HUD status rows should preserve {GpuDiagnosticsMetadataKeys.Rev3PassId}, {GpuDiagnosticsMetadataKeys.Rev3PathRole}, {GpuDiagnosticsMetadataKeys.Rev3PilotState}, and zero-copy metadata from canonical DTOs through live runtime DOM projection.");
        Console.WriteLine($"browser-execution-metadata-rule: browser executors should expose {GpuDiagnosticsMetadataKeys.Rev3ExecutionMode} to distinguish browser-webgpu-compute from deterministic-fallback paths.");
        Console.WriteLine("browser-pass-readiness-rule: browser executors should expose Passes.Aisthesis, Passes.SpatialReasoning, and Passes.HudComposite with ShaderBound, PipelineCached, BuiltInExecutor, InjectedExecutor, and ReadyForBuiltIn.");
        Console.WriteLine($"browser-pass-readiness-metadata-rule: Doom/browser status rows should project pass readiness into {GpuDiagnosticsMetadataKeys.Rev3PassReadiness} metadata and data-rev3-pass-readiness DOM attributes without parsing display text.");
        Console.WriteLine($"control-arbitration-rule: Control GPU arbitration should expose {GpuDiagnosticsMetadataKeys.Rev3PassId}, {GpuDiagnosticsMetadataKeys.Rev3ExecutionMode}, {GpuProviderMetadataKeys.Backend}, and {GpuProviderMetadataKeys.Fallback} metadata; approved mode is {GpuExecutionArbitrationExecutionModes.ControlGpuApproved}; fallback mode is {GpuExecutionArbitrationExecutionModes.ControlCpuFallback}.");
        Console.WriteLine($"control-arbitration-metadata-rule: Control GPU diagnostics should expose {GpuControlMetadataKeys.IntentId}, {GpuControlMetadataKeys.ObjectiveId}, {GpuControlMetadataKeys.ActionId}, {GpuControlMetadataKeys.RequiredCapabilities}, {GpuControlMetadataKeys.AvailableCapabilities}, {GpuControlMetadataKeys.MissingCapabilities}, and {GpuControlMetadataKeys.Tags} metadata.");
        Console.WriteLine($"control-arbitration-fallback-rule: stable Control GPU fallback reasons include {GpuExecutionArbitrationReasons.CpuForced}, {GpuExecutionArbitrationReasons.GpuLostOneWayFallback}, {GpuExecutionArbitrationReasons.ZeroCopyRequiredButUnavailable}, {GpuExecutionArbitrationReasons.RawFramebufferRequiredForZeroCopyAnalysis}, {GpuExecutionArbitrationReasons.NativePassBridgeUnavailable}, and {GpuExecutionArbitrationReasons.ReadbackRequiresFallback}.");
        Console.WriteLine("native-validation-rule: Dawn and OSNative providers should honor GpuProviderOptions.EnableNativeValidation when supported.");
        Console.WriteLine("native-abi-probe-rule: DawnProvider and Cuda13Provider should expose deterministic native ABI probe metadata before loading native libraries.");
        Console.WriteLine($"dawn-native-probe-rule: DawnProvider probe metadata should include {GpuNativeAbiMetadataKeys.DawnNativeAbiAvailable}, {GpuNativeAbiMetadataKeys.DawnNativeAbiReason}, {GpuNativeAbiMetadataKeys.DawnNativeEnvironmentVariable}, {GpuNativeAbiMetadataKeys.DawnNativeInitializationEntryPoint}, {GpuNativeAbiMetadataKeys.DawnNativeDispatchEntryPoint}, and {GpuNativeAbiMetadataKeys.DawnNativeValidation}.");
        Console.WriteLine($"cuda-native-probe-rule: Cuda13Provider probe metadata should include {GpuNativeAbiMetadataKeys.CudaNativeAbiAvailable}, {GpuNativeAbiMetadataKeys.CudaNativeAbiReason}, {GpuNativeAbiMetadataKeys.CudaNativeBridgeAvailable}, {GpuNativeAbiMetadataKeys.CudaLibTorchPathAvailable}, and {GpuNativeAbiMetadataKeys.CudaNativeValidation}.");
        Console.WriteLine("native-dispatch-metadata-rule: DawnProvider and Cuda13Provider invocation/diagnostics metadata should expose native dispatch ABI version, status, failure reason, frame index, sample ticks, and diagnostics byte count; staged bridges must distinguish DeviceUnavailable from CommandSubmissionDisabled.");
        Console.WriteLine("native-artifact-integrity-rule: aik gpu verify-native validates DawnCore exports, CPU fallback ABI state, Dawn package runtime assets, Cuda13 package runtime assets, and the Cuda13 staged dispatch probe when a native library is supplied.");
        Console.WriteLine("remaining-work-priority: P0=browser/Doom live GPU promotion gates and Cuda13 dependency-staged direct ABI smoke; P1=Dawn/Cuda13 native command encoders; P2=none after rev3 local NuGet config propagation.");
        Console.WriteLine("promotion-gate-policy: browser WebGPU built-in passes stay diagnostic until live trace parity has stable ready streaks and authoritative_ready=true.");
        Console.WriteLine("native-provider-policy: DawnProvider and Cuda13Provider stay fail-closed descriptor/probe surfaces until ABI handshake and buffer/texture mapping are verified.");
        return 0;
    }

    private static int VerifyNative(string[] args)
    {
        var options = ParseOptions(args);
        var provider = ResolveNativeArtifactProvider(options);
        if (provider is null)
        {
            Console.WriteLine("Usage: aik gpu verify-native [--provider dawn|cuda13] [--library native-library] [--package provider.nupkg]");
            return 1;
        }

        var hasLibrary = options.TryGetValue("--library", out var requestedLibraryPath) &&
            !string.IsNullOrWhiteSpace(requestedLibraryPath);
        var hasPackage = options.TryGetValue("--package", out var requestedPackagePath) &&
            !string.IsNullOrWhiteSpace(requestedPackagePath);
        var libraryPath = hasLibrary ? requestedLibraryPath! : string.Empty;
        var packagePath = hasPackage ? requestedPackagePath! : string.Empty;

        if (!hasLibrary && !hasPackage)
        {
            Console.WriteLine("Usage: aik gpu verify-native [--provider dawn|cuda13] [--library native-library] [--package provider.nupkg]");
            return 1;
        }

        Console.WriteLine($"native-artifact-integrity: start; provider={provider}");

        if (hasPackage && !ValidateNativePackage(packagePath, provider))
        {
            return 1;
        }

        if (!hasPackage)
        {
            Console.WriteLine("package-check: skipped");
        }

        if (hasLibrary && !ValidateNativeLibrary(libraryPath, provider))
        {
            return 1;
        }

        if (!hasLibrary)
        {
            Console.WriteLine("library-check: skipped");
        }

        Console.WriteLine("native-artifact-integrity: ok");
        return 0;
    }

    private static string? ResolveNativeArtifactProvider(IReadOnlyDictionary<string, string> options)
    {
        if (!options.TryGetValue("--provider", out var provider) ||
            string.IsNullOrWhiteSpace(provider))
        {
            return "dawn";
        }

        provider = provider.Trim().ToLowerInvariant();
        return provider is "dawn" or "cuda13" ? provider : null;
    }

    private static bool ValidateNativePackage(string packagePath, string provider)
    {
        if (string.IsNullOrWhiteSpace(packagePath) || !File.Exists(packagePath))
        {
            Console.WriteLine($"package: missing; path={packagePath}");
            return false;
        }

        try
        {
            using var package = ZipFile.OpenRead(packagePath);
            string[] requiredEntries = provider switch
            {
                "cuda13" =>
                [
                    "loader.json",
                    "contentFiles/any/any/loader.json",
                    "runtimes/win-x64/native/libtorch_bridge.dll"
                ],
                _ =>
                [
                    "runtimes/win-x64/native/custom_bridge.dll",
                    "runtimes/linux-x64/native/libcustom_bridge.so"
                ]
            };

            foreach (var entryName in requiredEntries)
            {
                if (package.GetEntry(entryName) is null)
                {
                    Console.WriteLine($"package-entry: {entryName} missing");
                    return false;
                }

                Console.WriteLine($"package-entry: {entryName} ok");
            }

            Console.WriteLine($"package: {packagePath}");
            Console.WriteLine($"package-provider: {provider}");
            return true;
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"package: invalid; path={packagePath}; error={ex.Message}");
            return false;
        }
    }

    private static bool ValidateNativeLibrary(string libraryPath, string provider)
        => provider == "cuda13"
            ? ValidateCuda13NativeLibrary(libraryPath)
            : ValidateDawnNativeLibrary(libraryPath);

    private static bool ValidateDawnNativeLibrary(string libraryPath)
    {
        if (string.IsNullOrWhiteSpace(libraryPath) || !File.Exists(libraryPath))
        {
            Console.WriteLine($"library: missing; path={libraryPath}");
            return false;
        }

        IntPtr library = IntPtr.Zero;
        try
        {
            if (!TryLoadNativeLibrary(libraryPath, "dawn", out library))
            {
                return false;
            }

            if (!TryGetExport(library, "InitializeDawnNative", out var initializeExport))
            {
                return false;
            }

            if (!TryGetExport(library, "aikernel_dawn_dispatch", out var dispatchExport))
            {
                return false;
            }

            TryGetExport(library, "aikernel_dawn_shutdown", out var shutdownExport, required: false);

            var initialize = Marshal.GetDelegateForFunctionPointer<InitializeDawnNativeDelegate>(initializeExport);
            var dispatch = Marshal.GetDelegateForFunctionPointer<DawnDispatchDelegate>(dispatchExport);
            var shutdown = shutdownExport == IntPtr.Zero
                ? null
                : Marshal.GetDelegateForFunctionPointer<DawnShutdownDelegate>(shutdownExport);

            var statePointer = initialize();
            if (statePointer == IntPtr.Zero)
            {
                Console.WriteLine("state: missing");
                return false;
            }

            var state = ReadNativeState(statePointer);
            if (!ValidateCpuFallbackState(state, "state"))
            {
                return false;
            }

            const int nativeStateSize = 40;
            var response = Marshal.AllocHGlobal(nativeStateSize);
            try
            {
                var status = dispatch(IntPtr.Zero, 0, response, nativeStateSize);
                var dispatchState = ReadNativeState(response);
                if (status != (uint)DawnNativeStatusCode.CpuFallback)
                {
                    Console.WriteLine($"dispatch: unexpected-status; status={StatusName(status)}");
                    return false;
                }

                if (!ValidateCpuFallbackState(dispatchState, "dispatch-state"))
                {
                    return false;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(response);
            }

            if (!ValidateDawnDispatch(dispatch))
            {
                return false;
            }

            shutdown?.Invoke();
            Console.WriteLine($"library: {libraryPath}");
            return true;
        }
        catch (Exception ex) when (ex is BadImageFormatException or EntryPointNotFoundException or DllNotFoundException or MarshalDirectiveException or InvalidOperationException)
        {
            Console.WriteLine($"library: invalid; path={libraryPath}; error={ex.Message}");
            return false;
        }
        finally
        {
            if (library != IntPtr.Zero)
            {
                NativeLibrary.Free(library);
            }
        }
    }

    private static bool ValidateCuda13NativeLibrary(string libraryPath)
    {
        if (string.IsNullOrWhiteSpace(libraryPath) || !File.Exists(libraryPath))
        {
            Console.WriteLine($"library: missing; path={libraryPath}");
            return false;
        }

        IntPtr library = IntPtr.Zero;
        try
        {
            if (!TryLoadNativeLibrary(libraryPath, "cuda13", out library))
            {
                return false;
            }

            if (!TryGetExport(library, "load_model", out _))
            {
                return false;
            }

            if (!TryGetExport(library, "unload_model", out _))
            {
                return false;
            }

            if (!TryGetExport(library, "forward", out _))
            {
                return false;
            }

            if (!TryGetExport(library, "aikernel_cuda13_dispatch", out var dispatchExport))
            {
                return false;
            }

            var dispatch = Marshal.GetDelegateForFunctionPointer<Cuda13DispatchDelegate>(dispatchExport);
            if (!ValidateCuda13Dispatch(dispatch))
            {
                return false;
            }

            Console.WriteLine("library-provider: cuda13");
            Console.WriteLine($"library: {libraryPath}");
            return true;
        }
        catch (Exception ex) when (ex is BadImageFormatException or EntryPointNotFoundException or DllNotFoundException or MarshalDirectiveException or InvalidOperationException)
        {
            Console.WriteLine($"library: invalid; path={libraryPath}; error={ex.Message}");
            return false;
        }
        finally
        {
            if (library != IntPtr.Zero)
            {
                NativeLibrary.Free(library);
            }
        }
    }

    private static bool TryGetExport(IntPtr library, string exportName, out IntPtr export, bool required = true)
    {
        if (NativeLibrary.TryGetExport(library, exportName, out export))
        {
            Console.WriteLine($"export: {exportName} ok");
            return true;
        }

        if (required)
        {
            Console.WriteLine($"export: {exportName} missing");
            return false;
        }

        export = IntPtr.Zero;
        Console.WriteLine($"export: {exportName} skipped");
        return true;
    }

    private static bool TryLoadNativeLibrary(string libraryPath, string provider, out IntPtr library)
    {
        try
        {
            library = NativeLibrary.Load(libraryPath);
            return library != IntPtr.Zero;
        }
        catch (Exception ex) when (ex is BadImageFormatException or DllNotFoundException or FileLoadException or IOException or UnauthorizedAccessException)
        {
            library = IntPtr.Zero;
            Console.WriteLine($"library: load-failed; path={libraryPath}; error={ex.Message}");
            if (ex is DllNotFoundException && string.Equals(provider, "cuda13", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("library-hint: missing-dependent-module; provider=cuda13; ensure LibTorch and CUDA runtime DLLs are on PATH or beside libtorch_bridge.dll.");
            }

            return false;
        }
    }

    private static bool ValidateCpuFallbackState(DawnNativeState state, string label)
    {
        Console.WriteLine(
            $"{label}: abi={state.AbiVersion} status={StatusName(state.Status)} cpu_fallback={Flag(state.CpuFallback)} adapter={Flag(state.AdapterReady)} device={Flag(state.DeviceReady)} vram={state.EstimatedVramBytes} max_storage={state.MaxStorageBufferBindingSize}");

        if (state.AbiVersion != 1)
        {
            Console.WriteLine($"{label}: invalid-abi; expected=1; actual={state.AbiVersion}");
            return false;
        }

        if (state.Status != (uint)DawnNativeStatusCode.CpuFallback)
        {
            Console.WriteLine($"{label}: invalid-status; expected=CpuFallback; actual={StatusName(state.Status)}");
            return false;
        }

        if (!state.CpuFallback || state.AdapterReady || state.DeviceReady)
        {
            Console.WriteLine($"{label}: invalid-readiness; expected cpu_fallback=true adapter=false device=false");
            return false;
        }

        return true;
    }

    private static bool ValidateDawnDispatch(DawnDispatchDelegate dispatch)
    {
        const int headerSize = 40;
        var request = AllocateDawnDispatchRequest(passId: 1, frameIndex: 42, sampleTicks: 1001);
        var unknownPassRequest = AllocateDawnDispatchRequest(passId: 999, frameIndex: 43, sampleTicks: 1002);
        var response = Marshal.AllocHGlobal(headerSize);
        try
        {
            ClearNativeBuffer(response, headerSize);
            var status = dispatch(request, headerSize, response, headerSize);
            var dispatchResponse = ReadDawnDispatchResponse(response);
            Console.WriteLine(
                $"dawn-dispatch-response: abi={dispatchResponse.AbiVersion} status={StatusName(dispatchResponse.Status)} failure={DawnFailureName(dispatchResponse.FailureReason)} frame={dispatchResponse.FrameIndex} ticks={dispatchResponse.SampleTicks} diagnostics={dispatchResponse.DiagnosticsBytes}");

            if (status != (uint)DawnNativeStatusCode.CpuFallback ||
                dispatchResponse.AbiVersion != 1 ||
                dispatchResponse.HeaderSize != headerSize ||
                dispatchResponse.Status != (uint)DawnNativeStatusCode.CpuFallback ||
                dispatchResponse.FailureReason != (uint)DawnNativeDispatchFailureReason.CpuFallback ||
                dispatchResponse.FrameIndex != 42 ||
                dispatchResponse.SampleTicks != 1001 ||
                dispatchResponse.DiagnosticsBytes != 0)
            {
                Console.WriteLine("dawn-dispatch-response: invalid");
                return false;
            }

            ClearNativeBuffer(response, headerSize);
            _ = dispatch(unknownPassRequest, headerSize, response, headerSize);
            var unknownPassResponse = ReadDawnDispatchResponse(response);
            Console.WriteLine(
                $"dawn-dispatch-unknown-pass: failure={DawnFailureName(unknownPassResponse.FailureReason)}");

            if (unknownPassResponse.AbiVersion != 1 ||
                unknownPassResponse.HeaderSize != headerSize ||
                unknownPassResponse.FailureReason != (uint)DawnNativeDispatchFailureReason.UnknownPass)
            {
                Console.WriteLine("dawn-dispatch-unknown-pass: invalid");
                return false;
            }

            ClearNativeBuffer(response, headerSize);
            _ = dispatch(request, 8, response, headerSize);
            var invalidLengthResponse = ReadDawnDispatchResponse(response);
            Console.WriteLine(
                $"dawn-dispatch-invalid-length: failure={DawnFailureName(invalidLengthResponse.FailureReason)}");

            if (invalidLengthResponse.AbiVersion != 1 ||
                invalidLengthResponse.HeaderSize != headerSize ||
                invalidLengthResponse.FailureReason != (uint)DawnNativeDispatchFailureReason.InvalidRequestLength)
            {
                Console.WriteLine("dawn-dispatch-invalid-length: invalid");
                return false;
            }

            return true;
        }
        finally
        {
            Marshal.FreeHGlobal(request);
            Marshal.FreeHGlobal(unknownPassRequest);
            Marshal.FreeHGlobal(response);
        }
    }

    private static bool ValidateCuda13Dispatch(Cuda13DispatchDelegate dispatch)
    {
        const int headerSize = 40;
        var request = AllocateCuda13DispatchRequest(frameIndex: 1, sampleTicks: 100);
        var response = Marshal.AllocHGlobal(headerSize);
        try
        {
            ClearNativeBuffer(response, headerSize);
            var status = dispatch(request, headerSize, response, headerSize);
            var dispatchResponse = ReadCuda13DispatchResponse(response);
            Console.WriteLine(
                $"cuda-dispatch-response: abi={dispatchResponse.AbiVersion} status={Cuda13StatusName(dispatchResponse.Status)} failure={Cuda13FailureName(dispatchResponse.FailureReason)} frame={dispatchResponse.FrameIndex} ticks={dispatchResponse.SampleTicks} diagnostics={dispatchResponse.DiagnosticsBytes}");

            if (status != (uint)Cuda13NativeDispatchStatusCode.NotInitialized ||
                dispatchResponse.AbiVersion != 1 ||
                dispatchResponse.HeaderSize != headerSize ||
                dispatchResponse.Status != (uint)Cuda13NativeDispatchStatusCode.NotInitialized ||
                dispatchResponse.FailureReason != (uint)Cuda13NativeDispatchFailureReason.CommandSubmissionDisabled ||
                dispatchResponse.FrameIndex != 1 ||
                dispatchResponse.SampleTicks != 100 ||
                dispatchResponse.DiagnosticsBytes != 0)
            {
                Console.WriteLine("cuda-dispatch-response: invalid");
                return false;
            }

            ClearNativeBuffer(response, headerSize);
            _ = dispatch(request, 8, response, headerSize);
            var invalidLengthResponse = ReadCuda13DispatchResponse(response);
            Console.WriteLine(
                $"cuda-dispatch-invalid-length: failure={Cuda13FailureName(invalidLengthResponse.FailureReason)}");

            if (invalidLengthResponse.AbiVersion != 1 ||
                invalidLengthResponse.HeaderSize != headerSize ||
                invalidLengthResponse.FailureReason != (uint)Cuda13NativeDispatchFailureReason.InvalidRequestLength)
            {
                Console.WriteLine("cuda-dispatch-invalid-length: invalid");
                return false;
            }

            return true;
        }
        finally
        {
            Marshal.FreeHGlobal(request);
            Marshal.FreeHGlobal(response);
        }
    }

    private static DawnNativeState ReadNativeState(IntPtr statePointer)
        => new()
        {
            AbiVersion = unchecked((uint)Marshal.ReadInt32(statePointer, 0)),
            Status = unchecked((uint)Marshal.ReadInt32(statePointer, 4)),
            EstimatedVramBytes = unchecked((ulong)Marshal.ReadInt64(statePointer, 8)),
            MaxStorageBufferBindingSize = unchecked((ulong)Marshal.ReadInt64(statePointer, 16)),
            AdapterReady = Marshal.ReadInt32(statePointer, 24) != 0,
            DeviceReady = Marshal.ReadInt32(statePointer, 28) != 0,
            CpuFallback = Marshal.ReadInt32(statePointer, 32) != 0
        };

    private static string StatusName(uint status)
        => Enum.IsDefined(typeof(DawnNativeStatusCode), status)
            ? ((DawnNativeStatusCode)status).ToString()
            : $"Unknown({status})";

    private static IntPtr AllocateDawnDispatchRequest(uint passId, ulong frameIndex, ulong sampleTicks)
    {
        const int headerSize = 40;
        var request = Marshal.AllocHGlobal(headerSize);
        ClearNativeBuffer(request, headerSize);
        Marshal.WriteInt32(request, 0, 1);
        Marshal.WriteInt32(request, 4, headerSize);
        Marshal.WriteInt32(request, 8, unchecked((int)passId));
        Marshal.WriteInt32(request, 12, 0);
        Marshal.WriteInt64(request, 16, unchecked((long)frameIndex));
        Marshal.WriteInt64(request, 24, unchecked((long)sampleTicks));
        Marshal.WriteInt32(request, 32, 0);
        Marshal.WriteInt32(request, 36, 0);
        return request;
    }

    private static IntPtr AllocateCuda13DispatchRequest(ulong frameIndex, ulong sampleTicks)
    {
        const int headerSize = 40;
        var request = Marshal.AllocHGlobal(headerSize);
        ClearNativeBuffer(request, headerSize);
        Marshal.WriteInt32(request, 0, 1);
        Marshal.WriteInt32(request, 4, headerSize);
        Marshal.WriteInt32(request, 8, 1);
        Marshal.WriteInt32(request, 12, 0);
        Marshal.WriteInt64(request, 16, unchecked((long)frameIndex));
        Marshal.WriteInt64(request, 24, unchecked((long)sampleTicks));
        Marshal.WriteInt32(request, 32, 0);
        Marshal.WriteInt32(request, 36, 0);
        return request;
    }

    private static void ClearNativeBuffer(IntPtr buffer, int length)
    {
        for (var index = 0; index < length; index++)
        {
            Marshal.WriteByte(buffer, index, 0);
        }
    }

    private static DawnDispatchResponse ReadDawnDispatchResponse(IntPtr responsePointer)
        => new()
        {
            AbiVersion = unchecked((uint)Marshal.ReadInt32(responsePointer, 0)),
            HeaderSize = unchecked((uint)Marshal.ReadInt32(responsePointer, 4)),
            Status = unchecked((uint)Marshal.ReadInt32(responsePointer, 8)),
            FailureReason = unchecked((uint)Marshal.ReadInt32(responsePointer, 12)),
            FrameIndex = unchecked((ulong)Marshal.ReadInt64(responsePointer, 16)),
            SampleTicks = unchecked((ulong)Marshal.ReadInt64(responsePointer, 24)),
            DiagnosticsBytes = unchecked((uint)Marshal.ReadInt32(responsePointer, 32))
        };

    private static Cuda13DispatchResponse ReadCuda13DispatchResponse(IntPtr responsePointer)
        => new()
        {
            AbiVersion = unchecked((uint)Marshal.ReadInt32(responsePointer, 0)),
            HeaderSize = unchecked((uint)Marshal.ReadInt32(responsePointer, 4)),
            Status = unchecked((uint)Marshal.ReadInt32(responsePointer, 8)),
            FailureReason = unchecked((uint)Marshal.ReadInt32(responsePointer, 12)),
            FrameIndex = unchecked((ulong)Marshal.ReadInt64(responsePointer, 16)),
            SampleTicks = unchecked((ulong)Marshal.ReadInt64(responsePointer, 24)),
            DiagnosticsBytes = unchecked((uint)Marshal.ReadInt32(responsePointer, 32))
        };

    private static string Cuda13StatusName(uint status)
        => Enum.IsDefined(typeof(Cuda13NativeDispatchStatusCode), status)
            ? ((Cuda13NativeDispatchStatusCode)status).ToString()
            : $"Unknown({status})";

    private static string DawnFailureName(uint failureReason)
        => Enum.IsDefined(typeof(DawnNativeDispatchFailureReason), failureReason)
            ? ((DawnNativeDispatchFailureReason)failureReason).ToString()
            : $"Unknown({failureReason})";

    private static string Cuda13FailureName(uint failureReason)
        => Enum.IsDefined(typeof(Cuda13NativeDispatchFailureReason), failureReason)
            ? ((Cuda13NativeDispatchFailureReason)failureReason).ToString()
            : $"Unknown({failureReason})";

    private static GpuValidationResult ValidateCanonicalNativeProbeMetadataSamples(
        out bool dawnValid,
        out bool cudaValid)
    {
        var dawnMetadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuNativeAbiMetadataKeys.DawnNativeAbiAvailable] = "false",
            [GpuNativeAbiMetadataKeys.DawnNativeAbiReason] = "path-not-configured",
            [GpuNativeAbiMetadataKeys.DawnNativeEnvironmentVariable] = "AIKERNEL_DAWNCORE_PATH",
            [GpuNativeAbiMetadataKeys.DawnNativeEntryPoint] = "aikernel_dawn_dispatch",
            [GpuNativeAbiMetadataKeys.DawnNativeInitializationEntryPoint] = "InitializeDawnNative",
            [GpuNativeAbiMetadataKeys.DawnNativeDispatchEntryPoint] = "aikernel_dawn_dispatch",
            [GpuNativeAbiMetadataKeys.DawnNativeValidation] = "default",
            [GpuNativeAbiMetadataKeys.DawnNativePathConfigured] = "false"
        };
        var cudaMetadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuNativeAbiMetadataKeys.CudaNativeAbiAvailable] = "false",
            [GpuNativeAbiMetadataKeys.CudaNativeAbiReason] = "native-bridge-not-configured",
            [GpuNativeAbiMetadataKeys.CudaNativeBridgeAvailable] = "false",
            [GpuNativeAbiMetadataKeys.CudaNativeBridgeLibrary] = "libtorch_bridge.dll",
            [GpuNativeAbiMetadataKeys.CudaNativeLoaderConfigured] = "false",
            [GpuNativeAbiMetadataKeys.CudaNativeLoaderEnvironmentVariable] = "AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER",
            [GpuNativeAbiMetadataKeys.CudaNativeValidation] = "default",
            [GpuNativeAbiMetadataKeys.CudaLibTorchPathConfigured] = "false",
            [GpuNativeAbiMetadataKeys.CudaLibTorchPathAvailable] = "false",
            [GpuNativeAbiMetadataKeys.CudaLibTorchPathEnvironmentVariable] = "AIKERNEL_LIBTORCH_PATH"
        };

        var dawnValidation = GpuCanonicalValidation.ValidateNativeAbiProbeMetadata(
            GpuBackend.Dawn,
            dawnMetadata);
        var cudaValidation = GpuCanonicalValidation.ValidateNativeAbiProbeMetadata(
            GpuBackend.Cuda,
            cudaMetadata);

        dawnValid = dawnValidation.IsValid;
        cudaValid = cudaValidation.IsValid;
        return dawnValidation.Merge(cudaValidation);
    }

    private static GpuValidationResult ValidateCanonicalControlArbitrationMetadataSample(
        out bool valid)
    {
        var metadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuDiagnosticsMetadataKeys.Rev3PassId] = GpuOperationNames.GpuHudComposite,
            [GpuDiagnosticsMetadataKeys.Rev3ExecutionMode] = GpuExecutionArbitrationExecutionModes.ControlGpuApproved,
            [GpuProviderMetadataKeys.Backend] = GpuBackend.WebGpu.ToString(),
            [GpuProviderMetadataKeys.Fallback] = GpuExecutionArbitrationReasons.None,
            [GpuControlMetadataKeys.IntentId] = "render",
            [GpuControlMetadataKeys.ObjectiveId] = "doom-hud",
            [GpuControlMetadataKeys.ActionId] = "compose",
            [GpuControlMetadataKeys.Reason] = GpuExecutionArbitrationReasons.GpuExecutionApproved,
            [GpuControlMetadataKeys.RequiredCapabilities] = (GpuProviderCapabilities.SupportsCompute |
                GpuProviderCapabilities.SupportsHudComposite).ToString(),
            [GpuControlMetadataKeys.AvailableCapabilities] = (GpuProviderCapabilities.SupportsCompute |
                GpuProviderCapabilities.SupportsHudComposite).ToString(),
            [GpuControlMetadataKeys.MissingCapabilities] = GpuProviderCapabilities.None.ToString(),
            [GpuControlMetadataKeys.InputTargetKind] = GpuFrameTargetKind.RawFramebuffer.ToString(),
            [GpuControlMetadataKeys.ReadbackPolicy] = GpuReadbackPolicy.None.ToString(),
            [GpuControlMetadataKeys.ZeroCopyRawTextureRequired] = "true",
            [GpuControlMetadataKeys.NativePassBridgeRequired] = "false",
            [GpuControlMetadataKeys.NativePassBridgeAvailable] = "true",
            [GpuControlMetadataKeys.Tags] = "gpu,approved,render,doom-hud,compose"
        };

        var validation = GpuCanonicalValidation.ValidateControlArbitrationMetadata(metadata);
        valid = validation.IsValid;
        return validation;
    }

    private static GpuValidationResult ValidateCanonicalBrowserBridgeMetadataSample(
        out bool valid)
    {
        var metadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuProviderMetadataKeys.Rev3BrowserPassReadiness] = "Passes.{Aisthesis,SpatialReasoning,HudComposite}:ShaderBound,PipelineCached,BuiltInExecutor,InjectedExecutor,ReadyForBuiltIn",
            [GpuProviderMetadataKeys.Rev3EnvelopeBridge] = "runtime/browser/webgpu-rev3-envelope-bridge.js",
            [GpuProviderMetadataKeys.Rev3EnvelopeBridgeExecutorFactory] = "createWebGpuRev3BrowserExecutor",
            [GpuProviderMetadataKeys.Rev3EnvelopeBridgeFactory] = "createWebGpuRev3EnvelopeBridge",
            [GpuProviderMetadataKeys.Rev3EnvelopeBridgeGlobal] = "AIKernelWebGpuRev3",
            [GpuProviderMetadataKeys.Rev3EnvelopeSchema] = "aikernel.gpu.rev3.dispatch"
        };

        var validation = GpuCanonicalValidation.ValidateRev3BrowserBridgeMetadata(metadata);
        valid = validation.IsValid;
        return validation;
    }

    private static GpuValidationResult ValidateCanonicalExecutionLayerMetadataSamples(
        out bool valid)
    {
        var webGpuMetadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuProviderMetadataKeys.PassBridge] = "optional-native-or-js",
            [GpuProviderMetadataKeys.RawCaptureSource] = "raw-framebuffer",
            [GpuProviderMetadataKeys.GpuBypass] = "raw-texture-binding",
            [GpuProviderMetadataKeys.NativeJsBridge] = "rev3-envelope-bridge",
            [GpuProviderMetadataKeys.AotCompilerHooks] = "planned-gpu-native-execution",
            [GpuProviderMetadataKeys.DeterministicFrameSampling] = "frame-token-index-sample-ticks",
            [GpuProviderMetadataKeys.ZeroCopyBufferHandling] = "raw-framebuffer-texture"
        };
        var dawnMetadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuProviderMetadataKeys.PassBridge] = "native-dawn-abi",
            [GpuProviderMetadataKeys.RawCaptureSource] = "raw-framebuffer",
            [GpuProviderMetadataKeys.GpuBypass] = "native-dawn-texture-binding",
            [GpuProviderMetadataKeys.NativeJsBridge] = "not-required-native-provider",
            [GpuProviderMetadataKeys.AotCompilerHooks] = "planned-gpu-native-execution",
            [GpuProviderMetadataKeys.DeterministicFrameSampling] = "frame-token-index-sample-ticks",
            [GpuProviderMetadataKeys.ZeroCopyBufferHandling] = "native-dawn-texture"
        };
        var cudaMetadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuProviderMetadataKeys.PassBridge] = "native-abi",
            [GpuProviderMetadataKeys.RawCaptureSource] = "none",
            [GpuProviderMetadataKeys.GpuBypass] = "native-cuda-buffer-dispatch",
            [GpuProviderMetadataKeys.NativeJsBridge] = "not-required-native-provider",
            [GpuProviderMetadataKeys.AotCompilerHooks] = "planned-gpu-native-execution",
            [GpuProviderMetadataKeys.DeterministicFrameSampling] = "host-frame-token-sample-ticks",
            [GpuProviderMetadataKeys.ZeroCopyBufferHandling] = "native-cuda-device-buffer"
        };

        var validation = GpuCanonicalValidation
            .ValidateRev3ExecutionLayerMetadata(webGpuMetadata)
            .Merge(GpuCanonicalValidation.ValidateRev3ExecutionLayerMetadata(dawnMetadata))
            .Merge(GpuCanonicalValidation.ValidateRev3ExecutionLayerMetadata(cudaMetadata));
        valid = validation.IsValid;
        return validation;
    }

    private static GpuValidationResult ValidateCanonicalFrameDiagnosticsSample(
        out bool valid,
        out bool webGpuValid,
        out bool dawnValid,
        out bool cudaValid,
        out GpuRev3PromotionReadiness promotionReadiness)
    {
        var webGpuDiagnostics = CreateCanonicalFrameDiagnostics(
            GpuBackend.WebGpu,
            zeroCopy: true,
            readback: GpuReadbackPolicy.None,
            executionMode: "browser-webgpu-compute",
            passReadiness: "Passes.{Aisthesis,SpatialReasoning,HudComposite}:ShaderBound=false,PipelineCached=false,BuiltInExecutor=true,InjectedExecutor=false,ReadyForBuiltIn=false",
            gpuBypass: "raw-texture-binding",
            nativeJsBridge: "rev3-envelope-bridge",
            passBridge: "optional-native-or-js",
            rawCaptureSource: "raw-framebuffer",
            deterministicFrameSampling: "frame-token-index-sample-ticks",
            zeroCopyBufferHandling: "raw-framebuffer-texture");
        var dawnDiagnostics = CreateCanonicalFrameDiagnostics(
            GpuBackend.Dawn,
            zeroCopy: false,
            readback: GpuReadbackPolicy.RequiredFallback,
            executionMode: "native-dawn-descriptor",
            passReadiness: "Passes.{Aisthesis,SpatialReasoning,HudComposite}:ShaderBound=false,PipelineCached=false,BuiltInExecutor=false,InjectedExecutor=false,ReadyForBuiltIn=false",
            gpuBypass: "native-dawn-texture-binding",
            nativeJsBridge: "not-required-native-provider",
            passBridge: "native-dawn-abi",
            rawCaptureSource: "raw-framebuffer",
            deterministicFrameSampling: "frame-token-index-sample-ticks",
            zeroCopyBufferHandling: "native-dawn-texture");
        var cudaDiagnostics = CreateCanonicalFrameDiagnostics(
            GpuBackend.Cuda,
            zeroCopy: false,
            readback: GpuReadbackPolicy.RequiredFallback,
            executionMode: "native-cuda-abi-probe",
            passReadiness: "Passes.{Aisthesis,SpatialReasoning,HudComposite}:ShaderBound=false,PipelineCached=false,BuiltInExecutor=false,InjectedExecutor=false,ReadyForBuiltIn=false",
            gpuBypass: "native-cuda-buffer-dispatch",
            nativeJsBridge: "not-required-native-provider",
            passBridge: "native-abi",
            rawCaptureSource: "none",
            deterministicFrameSampling: "host-frame-token-sample-ticks",
            zeroCopyBufferHandling: "native-cuda-device-buffer");

        var webGpuValidation = GpuCanonicalValidation.ValidateFrameDiagnostics(webGpuDiagnostics);
        var dawnValidation = GpuCanonicalValidation.ValidateFrameDiagnostics(dawnDiagnostics);
        var cudaValidation = GpuCanonicalValidation.ValidateFrameDiagnostics(cudaDiagnostics);

        webGpuValid = webGpuValidation.IsValid;
        dawnValid = dawnValidation.IsValid;
        cudaValid = cudaValidation.IsValid;
        valid = webGpuValid && dawnValid && cudaValid;
        promotionReadiness = GpuCanonicalValidation.EvaluateRev3PromotionReadiness(
            webGpuDiagnostics.SensorPath.Metadata);

        return webGpuValidation.Merge(dawnValidation).Merge(cudaValidation);
    }

    private static GpuFrameDiagnostics CreateCanonicalFrameDiagnostics(
        GpuBackend backend,
        bool zeroCopy,
        GpuReadbackPolicy readback,
        string executionMode,
        string passReadiness,
        string gpuBypass,
        string nativeJsBridge,
        string passBridge,
        string rawCaptureSource,
        string deterministicFrameSampling,
        string zeroCopyBufferHandling)
        => new()
        {
            GamePath = CreateCanonicalDiagnosticsPath("game", "game", 320 * 200, backend, zeroCopy, readback, executionMode, passReadiness, gpuBypass, nativeJsBridge, passBridge, rawCaptureSource, deterministicFrameSampling, zeroCopyBufferHandling),
            BonsaiPath = CreateCanonicalDiagnosticsPath("bonsai", "bonsai", 320 * 200, backend, zeroCopy, readback, executionMode, passReadiness, gpuBypass, nativeJsBridge, passBridge, rawCaptureSource, deterministicFrameSampling, zeroCopyBufferHandling),
            HudPath = CreateCanonicalDiagnosticsPath("hud", "hud", 320 * 200 * 4, backend, zeroCopy, readback, executionMode, passReadiness, gpuBypass, nativeJsBridge, passBridge, rawCaptureSource, deterministicFrameSampling, zeroCopyBufferHandling),
            SensorPath = CreateCanonicalDiagnosticsPath("sensor", "sensor", 320 * 200, backend, zeroCopy, readback, executionMode, passReadiness, gpuBypass, nativeJsBridge, passBridge, rawCaptureSource, deterministicFrameSampling, zeroCopyBufferHandling)
        };

    private static GpuDiagnosticsPathInfo CreateCanonicalDiagnosticsPath(
        string passId,
        string pathRole,
        long memoryEstimate,
        GpuBackend backend,
        bool zeroCopy,
        GpuReadbackPolicy readback,
        string executionMode,
        string passReadiness,
        string gpuBypass,
        string nativeJsBridge,
        string passBridge,
        string rawCaptureSource,
        string deterministicFrameSampling,
        string zeroCopyBufferHandling)
    {
        var metadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuDiagnosticsMetadataKeys.Rev3AuthoritativeReady] = "false",
            [GpuDiagnosticsMetadataKeys.Rev3CandidateStreak] = "0",
            [GpuDiagnosticsMetadataKeys.Rev3DiagnosticReady] = "true",
            [GpuDiagnosticsMetadataKeys.Rev3DiagnosticStreak] = "1",
            [GpuDiagnosticsMetadataKeys.Rev3ExecutionMode] = executionMode,
            [GpuDiagnosticsMetadataKeys.Rev3FeatureMaskStorageTexture] = "false",
            [GpuDiagnosticsMetadataKeys.Rev3FrameIndex] = "1",
            [GpuDiagnosticsMetadataKeys.Rev3PassId] = passId,
            [GpuDiagnosticsMetadataKeys.Rev3PassReadiness] = passReadiness,
            [GpuDiagnosticsMetadataKeys.Rev3PathRole] = pathRole,
            [GpuDiagnosticsMetadataKeys.Rev3PilotState] = "diagnostic",
            [GpuDiagnosticsMetadataKeys.Rev3PromotionGate] = "trace-candidate",
            [GpuDiagnosticsMetadataKeys.Rev3RequiredStreak] = "3",
            [GpuDiagnosticsMetadataKeys.Rev3SampleTicks] = "100",
            [GpuProviderMetadataKeys.AotCompilerHooks] = "planned-gpu-native-execution",
            [GpuProviderMetadataKeys.DeterministicFrameSampling] = deterministicFrameSampling,
            [GpuProviderMetadataKeys.GpuBypass] = gpuBypass,
            [GpuProviderMetadataKeys.NativeJsBridge] = nativeJsBridge,
            [GpuProviderMetadataKeys.PassBridge] = passBridge,
            [GpuProviderMetadataKeys.RawCaptureSource] = rawCaptureSource,
            [GpuProviderMetadataKeys.ZeroCopyBufferHandling] = zeroCopyBufferHandling
        };

        return new GpuDiagnosticsPathInfo
        {
            Backend = backend.ToString(),
            ZeroCopy = zeroCopy,
            Readback = readback,
            PassId = passId,
            MemoryEstimate = memoryEstimate,
            Metadata = metadata
        };
    }

    private static Result<int> RunKernel(string[] args)
    {
        if (args.Length == 0 || !string.Equals(args[0], "vector-add", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: aik gpu run vector-add --a a.bin --b b.bin [--out out.bin]");
            return Result<int>.Success(1);
        }

        var options = ParseOptions(args.Skip(1).ToArray());
        return
            from paths in ValidateVectorAddPaths(options)
            from left in Try.Run(() => ReadFloatVector(paths.LeftPath))
            from right in Try.Run(() => ReadFloatVector(paths.RightPath))
            from _ in ValidateEqualLength(left, right)
            from output in Result<float[]>.Success(AddVectors(left, right))
            from written in Try.Run(() =>
            {
                File.WriteAllBytes(paths.OutputPath, MemoryMarshal.AsBytes<float>(output.AsSpan()).ToArray());
                return true;
            })
            select PrintVectorAdd(paths.OutputPath, output.Length);
    }

    private static Result<VectorAddPaths> ValidateVectorAddPaths(IReadOnlyDictionary<string, string> options)
    {
        if (!options.TryGetValue("--a", out var aPath) || !options.TryGetValue("--b", out var bPath))
        {
            return Result<VectorAddPaths>.Fail("Usage: aik gpu run vector-add --a a.bin --b b.bin [--out out.bin]");
        }

        var outPath = options.TryGetValue("--out", out var configuredOut)
            ? configuredOut
            : Path.Combine(Environment.CurrentDirectory, "vector-add.out.bin");
        return Result<VectorAddPaths>.Success(new VectorAddPaths(aPath, bPath, outPath));
    }

    private static float[] ReadFloatVector(string path)
        => MemoryMarshal.Cast<byte, float>(File.ReadAllBytes(path)).ToArray();

    private static Result<bool> ValidateEqualLength(IReadOnlyList<float> left, IReadOnlyList<float> right)
        => left.Count == right.Count
            ? Result<bool>.Success(true)
            : Result<bool>.Fail("input vector lengths must match. ErrorCode=CLI_GPU_VECTOR_LENGTH_MISMATCH");

    private static float[] AddVectors(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        var output = new float[left.Count];
        for (var index = 0; index < output.Length; index++)
        {
            output[index] = left[index] + right[index];
        }

        return output;
    }

    private static int PrintVectorAdd(string outPath, int elementCount)
    {
        Console.WriteLine("provider: cpu-fallback");
        Console.WriteLine("operation: vector-add");
        Console.WriteLine($"elements: {elementCount}");
        Console.WriteLine($"output: {outPath}");
        return 0;
    }

    private static string Flag(bool value)
        => value ? "true" : "false";

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index++)
        {
            if (args[index].StartsWith("--", StringComparison.Ordinal) && index + 1 < args.Length)
            {
                options[args[index]] = args[++index];
            }
        }

        return options;
    }

    private static void ShowUsage()
        => Console.WriteLine("""
Usage:
  aik gpu list
  aik gpu verify rev3
  aik gpu pending rev3
  aik gpu verify-native [--provider dawn|cuda13] [--library native-library] [--package provider.nupkg]
  aik gpu run vector-add --a a.bin --b b.bin [--out out.bin]
""");

    private static int ExitCode(Result<int> result, string prefix)
        => result.Match(
            error =>
            {
                Console.WriteLine($"{prefix}: {error.Message}");
                return 1;
            },
            value => value);

    private sealed record VectorAddPaths(string LeftPath, string RightPath, string OutputPath);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr InitializeDawnNativeDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint DawnDispatchDelegate(IntPtr request, uint requestLength, IntPtr response, uint responseLength);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DawnShutdownDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint Cuda13DispatchDelegate(IntPtr request, uint requestLength, IntPtr response, uint responseLength);

    private enum DawnNativeStatusCode : uint
    {
        Ok = 0,
        CpuFallback = 1,
        AdapterUnavailable = 2,
        DeviceLost = 3,
        NotInitialized = 4
    }

    private enum DawnNativeDispatchFailureReason : uint
    {
        None = 0,
        InvalidRequestLength = 1,
        InvalidAbi = 2,
        UnknownPass = 3,
        CpuFallback = 4,
        DeviceLost = 5,
        DeviceUnavailable = 6,
        CommandSubmissionDisabled = 7
    }

    private enum Cuda13NativeDispatchStatusCode : uint
    {
        Ok = 0,
        CpuFallback = 1,
        DeviceUnavailable = 2,
        DeviceLost = 3,
        NotInitialized = 4
    }

    private enum Cuda13NativeDispatchFailureReason : uint
    {
        None = 0,
        InvalidRequestLength = 1,
        InvalidAbi = 2,
        UnknownPass = 3,
        CpuFallback = 4,
        DeviceLost = 5,
        DeviceUnavailable = 6,
        CommandSubmissionDisabled = 7
    }

    private sealed record DawnNativeState
    {
        public uint AbiVersion { get; init; }

        public uint Status { get; init; }

        public ulong EstimatedVramBytes { get; init; }

        public ulong MaxStorageBufferBindingSize { get; init; }

        public bool AdapterReady { get; init; }

        public bool DeviceReady { get; init; }

        public bool CpuFallback { get; init; }
    }

    private sealed record DawnDispatchResponse
    {
        public uint AbiVersion { get; init; }

        public uint HeaderSize { get; init; }

        public uint Status { get; init; }

        public uint FailureReason { get; init; }

        public ulong FrameIndex { get; init; }

        public ulong SampleTicks { get; init; }

        public uint DiagnosticsBytes { get; init; }
    }

    private sealed record Cuda13DispatchResponse
    {
        public uint AbiVersion { get; init; }

        public uint HeaderSize { get; init; }

        public uint Status { get; init; }

        public uint FailureReason { get; init; }

        public ulong FrameIndex { get; init; }

        public ulong SampleTicks { get; init; }

        public uint DiagnosticsBytes { get; init; }
    }
}
