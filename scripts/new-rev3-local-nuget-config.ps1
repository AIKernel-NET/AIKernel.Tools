param(
    [int]$LocalPackageBuildNumber = 1,
    [string]$WorkspaceRoot,
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

$toolsRepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
if ([string]::IsNullOrWhiteSpace($WorkspaceRoot)) {
    $WorkspaceRoot = (Resolve-Path (Join-Path $toolsRepoRoot "..")).Path
}
else {
    $WorkspaceRoot = (Resolve-Path $WorkspaceRoot).Path
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $WorkspaceRoot "artifacts\NuGet.rev3-local.v0.1.3-dev$LocalPackageBuildNumber.config"
}

$localFeed = Join-Path $WorkspaceRoot "artifacts\local-nuget\v0.1.3-dev$LocalPackageBuildNumber"
if (-not (Test-Path -LiteralPath $localFeed)) {
    throw "Local package feed was not found: $localFeed"
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
}

$escapedLocalFeed = [System.Security.SecurityElement]::Escape((Resolve-Path $localFeed).Path)
$content = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="aikernel-local-v0.1.3-dev$LocalPackageBuildNumber" value="$escapedLocalFeed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@

Set-Content -LiteralPath $OutputPath -Value $content -Encoding UTF8
Write-Host "local-nuget-config: $OutputPath"
Write-Host "local-feed: $localFeed"
