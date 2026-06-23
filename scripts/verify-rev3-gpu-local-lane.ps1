param(
    [string]$Configuration = "Release",
    [int]$LocalPackageBuildNumber = 1,
    [string]$WorkspaceRoot,
    [string]$LocalFeed,
    [string]$NuGetConfigPath,
    [string]$DawnNativeLibrary,
    [string]$Cuda13BridgePath,
    [string]$LibTorchPath,
    [string]$CudaRuntimePath,
    [switch]$SkipToolsTests,
    [switch]$SkipWasmPackage,
    [switch]$SkipDawnPackage,
    [switch]$SkipDawnNativeFixture,
    [switch]$SkipCuda13Package,
    [switch]$SkipCuda13Library,
    [switch]$RequireFreshCuda13NativeBridge,
    [switch]$RequireCuda13LibraryLoad
)

$ErrorActionPreference = "Stop"

$toolsRepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
if ([string]::IsNullOrWhiteSpace($WorkspaceRoot)) {
    $WorkspaceRoot = (Resolve-Path (Join-Path $toolsRepoRoot "..")).Path
}
else {
    $WorkspaceRoot = (Resolve-Path $WorkspaceRoot).Path
}

if ([string]::IsNullOrWhiteSpace($LocalFeed)) {
    $LocalFeed = Join-Path $WorkspaceRoot "artifacts\local-nuget\v0.1.3-dev$LocalPackageBuildNumber"
}

if ([string]::IsNullOrWhiteSpace($NuGetConfigPath)) {
    $NuGetConfigPath = Join-Path $WorkspaceRoot "artifacts\NuGet.rev3-local.v0.1.3-dev$LocalPackageBuildNumber.config"
}

function Invoke-LaneStep {
    param(
        [string]$Name,
        [scriptblock]$Body
    )

    Write-Host ""
    Write-Host "== $Name =="
    & $Body
    if ($LASTEXITCODE -ne 0) {
        throw "$Name failed with exit code $LASTEXITCODE."
    }
}

if (-not (Test-Path -LiteralPath $LocalFeed)) {
    throw "Local package feed was not found: $LocalFeed"
}

if (-not (Test-Path -LiteralPath $NuGetConfigPath)) {
    & (Join-Path $toolsRepoRoot "scripts\new-rev3-local-nuget-config.ps1") `
        -LocalPackageBuildNumber $LocalPackageBuildNumber `
        -WorkspaceRoot $WorkspaceRoot `
        -OutputPath $NuGetConfigPath
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to generate rev3 local NuGet config: $NuGetConfigPath"
    }
}

$toolsDebugBuilt = $false

if (-not $SkipToolsTests) {
    Invoke-LaneStep "AIKernel.Tools smoke tests" {
        dotnet test (Join-Path $toolsRepoRoot "tests\AIKernel.Tools.Tests\AIKernel.Tools.Tests.csproj") `
            -c $Configuration `
            --no-restore `
            --logger "console;verbosity=minimal"
    }
}

if (-not $SkipWasmPackage) {
    Invoke-LaneStep "AIKernel.Wasm WebGPU package smoke" {
        & (Join-Path $WorkspaceRoot "AIKernel.Wasm\scripts\verify-webgpu-package.ps1") `
            -Configuration $Configuration `
            -LocalPackageBuildNumber $LocalPackageBuildNumber
    }
}

if (-not $SkipDawnPackage) {
    Invoke-LaneStep "AIKernel.Dawn native package smoke" {
        & (Join-Path $WorkspaceRoot "AIKernel.Dawn\scripts\verify-native-package.ps1") `
            -Configuration $Configuration `
            -PackageBuildNumber $LocalPackageBuildNumber
    }
}

if (-not $SkipDawnNativeFixture) {
    Invoke-LaneStep "AIKernel.Dawn native fixture smoke" {
        $dawnArgs = @{
            Configuration = "Debug"
            PackageBuildNumber = "$LocalPackageBuildNumber"
            LocalFeed = $LocalFeed
            NuGetConfigPath = $NuGetConfigPath
        }

        if (-not [string]::IsNullOrWhiteSpace($DawnNativeLibrary)) {
            $dawnArgs["DawnNativeLibrary"] = $DawnNativeLibrary
        }

        & (Join-Path $toolsRepoRoot "scripts\verify-dawn-native-fixture.ps1") @dawnArgs
        $script:toolsDebugBuilt = $true
    }
}

if (-not $SkipCuda13Package) {
    Invoke-LaneStep "AIKernel.Cuda13 native package smoke" {
        $cudaPackageArgs = @{
            Configuration = $Configuration
            LocalPackageBuildNumber = $LocalPackageBuildNumber
        }

        if ($RequireFreshCuda13NativeBridge) {
            $cudaPackageArgs["RequireFreshNativeBridge"] = $true
        }

        & (Join-Path $WorkspaceRoot "AIKernel.Cuda13.0\scripts\verify-native-package.ps1") @cudaPackageArgs
    }
}

if (-not $SkipCuda13Library) {
    Invoke-LaneStep "AIKernel.Cuda13 native library smoke" {
        $cudaArgs = @{
            Configuration = "Debug"
            LocalPackageBuildNumber = $LocalPackageBuildNumber
            ToolsRepoRoot = $toolsRepoRoot
            LocalFeed = $LocalFeed
            NuGetConfigPath = $NuGetConfigPath
        }

        if (-not [string]::IsNullOrWhiteSpace($Cuda13BridgePath)) {
            $cudaArgs["BridgePath"] = $Cuda13BridgePath
        }

        if (-not [string]::IsNullOrWhiteSpace($LibTorchPath)) {
            $cudaArgs["LibTorchPath"] = $LibTorchPath
        }

        if (-not [string]::IsNullOrWhiteSpace($CudaRuntimePath)) {
            $cudaArgs["CudaRuntimePath"] = $CudaRuntimePath
        }

        if (-not $RequireCuda13LibraryLoad) {
            $cudaArgs["AllowLoadFailure"] = $true
        }

        if ($toolsDebugBuilt) {
            $cudaArgs["NoBuildTools"] = $true
        }

        & (Join-Path $WorkspaceRoot "AIKernel.Cuda13.0\scripts\verify-native-library.ps1") @cudaArgs
    }
}

Write-Host ""
Write-Host "AIKernel rev3 GPU local lane: ok"
