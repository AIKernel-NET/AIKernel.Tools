param(
    [string]$Configuration = "Debug",
    [string]$PackageBuildNumber = "1",
    [string]$LocalFeed,
    [string]$NuGetConfigPath,
    [string]$DawnNativeLibrary,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$WorkspaceRoot = Resolve-Path (Join-Path $RepoRoot "..")

if ([string]::IsNullOrWhiteSpace($LocalFeed)) {
    $LocalFeed = Join-Path $WorkspaceRoot "artifacts\local-nuget\v0.1.3-dev$PackageBuildNumber"
}

if ([string]::IsNullOrWhiteSpace($NuGetConfigPath)) {
    $NuGetConfigPath = Join-Path $WorkspaceRoot "artifacts\NuGet.rev3-local.v0.1.3-dev$PackageBuildNumber.config"
}

if ([string]::IsNullOrWhiteSpace($DawnNativeLibrary)) {
    if ($IsLinux) {
        $DawnNativeLibrary = Join-Path $WorkspaceRoot "AIKernel.Dawn\native\DawnCore\build_linux\libcustom_bridge.so"
    }
    else {
        $DawnNativeLibrary = Join-Path $WorkspaceRoot "AIKernel.Dawn\native\DawnCore\build_windows\custom_bridge.dll"
    }
}

if (-not (Test-Path -LiteralPath $LocalFeed)) {
    throw "Local package feed was not found: $LocalFeed"
}

if (-not (Test-Path -LiteralPath $NuGetConfigPath)) {
    & (Join-Path $PSScriptRoot "new-rev3-local-nuget-config.ps1") `
        -LocalPackageBuildNumber ([int]$PackageBuildNumber) `
        -WorkspaceRoot $WorkspaceRoot `
        -OutputPath $NuGetConfigPath
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to generate rev3 local NuGet config: $NuGetConfigPath"
    }
}

if (-not (Test-Path -LiteralPath $DawnNativeLibrary)) {
    throw "Dawn native fixture was not found: $DawnNativeLibrary. Run AIKernel.Dawn\native\DawnCore\docker-build.ps1 -BuildNative first."
}

$ProjectPath = Join-Path $RepoRoot "src\AIKernel.CLI\AIKernel.CLI.csproj"
$CliAssembly = Join-Path $RepoRoot "src\AIKernel.CLI\bin\$Configuration\net10.0\aik.dll"

if (-not $NoBuild) {
    dotnet restore $ProjectPath `
        --configfile $NuGetConfigPath `
        -p:UseLocalPackageVersion=true `
        -p:LocalPackageBuildNumber=$PackageBuildNumber

    if ($LASTEXITCODE -ne 0) {
        throw "AIKernel.Tools CLI restore failed."
    }

    dotnet build $ProjectPath `
        -c $Configuration `
        --no-restore `
        -p:UseLocalPackageVersion=true `
        -p:LocalPackageBuildNumber=$PackageBuildNumber

    if ($LASTEXITCODE -ne 0) {
        throw "AIKernel.Tools CLI build failed."
    }
}

if (-not (Test-Path -LiteralPath $CliAssembly)) {
    throw "AIKernel.Tools CLI assembly was not found: $CliAssembly"
}

dotnet $CliAssembly gpu verify-native --provider dawn --library $DawnNativeLibrary
if ($LASTEXITCODE -ne 0) {
    throw "Dawn native fixture verification failed."
}
