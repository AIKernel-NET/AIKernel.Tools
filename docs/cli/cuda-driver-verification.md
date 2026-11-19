# Verify the standalone CUDA Driver ABI

Use the new selector for Kernel's native package:

```text
aik gpu verify-native --provider cuda-driver --package AIKernel.Kernel.<version>.nupkg
aik gpu verify-native --provider cuda-driver --library <native-library>
aik gpu verify-native --provider cuda-driver --package <package> --library <native-library>
```

`--package` requires both the Windows x64 DLL and Linux x64 SO under NuGet's
`runtimes/<rid>/native/` layout, checks their image architecture, and rejects
empty/truncated assets, duplicate ZIP entries, and Torch artifacts. It also
requires the Kernel managed assembly and native license. This is structural
validation, not proof of GPU execution or package authenticity.

`--library` loads the explicitly selected library, requires all 21 `ak_*`
exports, accepts only ABI version 1, checks host softmax values (including
non-finite output rejection), and requires invalid arguments to be rejected.
Exit code is 0 only when every requested check passes; failures return 1.
The output says `gpu-test: not-run`. No device discovery, CUDA context creation,
GPU allocation, or `AIKERNEL_LIBTORCH_PATH` lookup occurs.

The legacy `--provider cuda13` selector retains its old LibTorch bridge check;
it must not be used to validate `aikernel_cuda_driver`. Dawn validation is also
unchanged. NET's legacy `CudaLibTorchPath*` keys remain available to legacy
consumers, with no new use in this verifier.

The native test fixtures abort if a device-related export is called. The
fixture test checks a valid ABI and rejects version mismatch, missing export,
incorrect numerical results, and acceptance of invalid arguments. Run it with:

```text
dotnet test tests/AIKernel.Tools.Tests/AIKernel.Tools.Tests.csproj -c Release
cmake -S tests/native/cuda-driver -B artifacts/cuda-verifier -DCMAKE_BUILD_TYPE=Release
cmake --build artifacts/cuda-verifier --config Release
python tests/native/cuda-driver/verify.py src/AIKernel.CLI/bin/Release/net10.0/aik.dll artifacts/cuda-verifier
```

`.github/workflows/cuda-driver-verifier.yml` runs these checks on Windows and
Ubuntu for dev pushes and PRs. The fixture libraries are test data and must
never be included in runtime packages.
With a Visual Studio generator, add `-A x64` to the CMake configure command.
