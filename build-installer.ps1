$ErrorActionPreference = "Stop"

# ============================================================
# Paths
# ============================================================

$Root =
    Split-Path -Parent $MyInvocation.MyCommand.Path

Set-Location $Root

$ProjectFile =
    Join-Path $Root "DiscordPresence.csproj"

$PublishDir =
    Join-Path $Root `
        "bin\Release\net10.0-windows\win-x64\publish"

$InstallerScript =
    Join-Path $Root `
        "installer\DiscordPresence.iss"

$DistDir =
    Join-Path $Root "dist"

# ============================================================
# Header
# ============================================================

Write-Host ""
Write-Host "========================================" `
    -ForegroundColor Cyan

Write-Host " DiscordPresence Installer Build" `
    -ForegroundColor Cyan

Write-Host "========================================" `
    -ForegroundColor Cyan

Write-Host ""

# ============================================================
# Validate project files
# ============================================================

if (-not (Test-Path $ProjectFile))
{
    throw "DiscordPresence.csproj was not found: $ProjectFile"
}

if (-not (Test-Path $InstallerScript))
{
    throw "Installer script was not found: $InstallerScript"
}

# ============================================================
# 1. Publish application
# ============================================================

Write-Host "[1/3] Publishing application..." `
    -ForegroundColor Yellow

dotnet publish $ProjectFile `
    -c Release `
    -r win-x64 `
    --self-contained true

if ($LASTEXITCODE -ne 0)
{
    throw "dotnet publish failed."
}

$PublishedExe =
    Join-Path $PublishDir "DiscordPresence.exe"

if (-not (Test-Path $PublishedExe))
{
    throw "DiscordPresence.exe was not found in: $PublishDir"
}

Write-Host ""
Write-Host "Publish complete." `
    -ForegroundColor Green

Write-Host ""

# ============================================================
# 2. Find Inno Setup
# ============================================================

Write-Host "[2/3] Locating Inno Setup compiler..." `
    -ForegroundColor Yellow

$ISCC =
    @(
        "$env:ProgramFiles\Inno Setup 7\ISCC.exe"
        "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe"
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
    ) |
    Where-Object {
        $_ -and (Test-Path $_)
    } |
    Select-Object -First 1

# Try PATH as fallback.
if (-not $ISCC)
{
    $Command =
        Get-Command "ISCC.exe" `
            -ErrorAction SilentlyContinue

    if ($Command)
    {
        $ISCC =
            $Command.Source
    }
}

if (-not $ISCC)
{
    Write-Host ""
    Write-Host "Inno Setup was not found." `
        -ForegroundColor Red

    Write-Host ""
    Write-Host "Install it with:" `
        -ForegroundColor Yellow

    Write-Host ""
    Write-Host `
        "winget install --id JRSoftware.InnoSetup.7 -e -s winget -i"

    Write-Host ""

    throw "Inno Setup compiler not installed."
}

Write-Host "Using:" `
    -ForegroundColor DarkGray

Write-Host $ISCC `
    -ForegroundColor DarkGray

Write-Host ""

# ============================================================
# Clean dist
# ============================================================

if (Test-Path $DistDir)
{
    Remove-Item `
        $DistDir `
        -Recurse `
        -Force
}

New-Item `
    -ItemType Directory `
    -Path $DistDir `
    -Force |
    Out-Null

# ============================================================
# 3. Build installer
# ============================================================

Write-Host "[3/3] Compiling installer..." `
    -ForegroundColor Yellow

& $ISCC $InstallerScript

if ($LASTEXITCODE -ne 0)
{
    throw "Inno Setup compilation failed."
}

# ============================================================
# Find generated installer
# ============================================================

$Installer =
    Get-ChildItem `
        -Path $DistDir `
        -Filter "DiscordPresence-Setup-*.exe" `
        -File |
    Sort-Object LastWriteTime `
        -Descending |
    Select-Object -First 1

if (-not $Installer)
{
    throw "Installer build finished, but no installer was found in: $DistDir"
}

# ============================================================
# Done
# ============================================================

Write-Host ""
Write-Host "========================================" `
    -ForegroundColor Green

Write-Host " Installer build complete!" `
    -ForegroundColor Green

Write-Host "========================================" `
    -ForegroundColor Green

Write-Host ""

Write-Host "Installer:" `
    -ForegroundColor Green

Write-Host $Installer.FullName `
    -ForegroundColor Cyan

Write-Host ""