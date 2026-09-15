$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $PSScriptRoot "IconPacker\IconPacker.csproj"

dotnet run `
    --project $project `
    --configuration Release `
    --no-launch-profile `
    -- $repoRoot

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Embedded icon resource generated successfully."
