$ErrorActionPreference = "Stop"

# ============================================================
# Paths
# ============================================================

$Root =
    Split-Path -Parent $MyInvocation.MyCommand.Path

Set-Location $Root

$ProjectFile =
    Join-Path $Root "DiscordPresence.csproj"

$NativeSourceDir =
    Join-Path $Root `
        "native\DiscordSocialBridge"

$NativeBuildDir =
    Join-Path $NativeSourceDir `
        "build"

$NativeDll =
    Join-Path $NativeBuildDir `
        "Release\DiscordSocialBridge.dll"

$PublishDir =
    Join-Path $Root `
        "bin\Release\net10.0-windows\win-x64\publish"

$PublishedExe =
    Join-Path $PublishDir `
        "DiscordPresence.exe"

$PublishedBridgeDll =
    Join-Path $PublishDir `
        "DiscordSocialBridge.dll"

$PublishedSdkDll =
    Join-Path $PublishDir `
        "discord_partner_sdk.dll"

$InstallerScript =
    Join-Path $Root `
        "installer\DiscordPresence.iss"

$DistDir =
    Join-Path $Root `
        "dist"

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
# Validate required files
# ============================================================

if (-not (Test-Path $ProjectFile))
{
    throw "DiscordPresence.csproj was not found: $ProjectFile"
}

if (-not (Test-Path $NativeSourceDir))
{
    throw "Native bridge source directory was not found: $NativeSourceDir"
}

if (-not (Test-Path $InstallerScript))
{
    throw "Installer script was not found: $InstallerScript"
}

# ============================================================
# Clean old build output
# ============================================================

Write-Host "[1/5] Cleaning old build output..." `
    -ForegroundColor Yellow

$OldBuildDirectories =
    @(
        (Join-Path $Root "bin")
        (Join-Path $Root "obj")
        $NativeBuildDir
        $DistDir
    )

foreach ($Directory in $OldBuildDirectories)
{
    if (Test-Path $Directory)
    {
        Remove-Item `
            $Directory `
            -Recurse `
            -Force
    }
}

New-Item `
    -ItemType Directory `
    -Path $DistDir `
    -Force |
    Out-Null

Write-Host "Old build output cleaned." `
    -ForegroundColor Green

Write-Host ""

# ============================================================
# Build native Discord Social SDK bridge
# ============================================================

Write-Host "[2/5] Building native Social SDK bridge..." `
    -ForegroundColor Yellow

cmake `
    -S $NativeSourceDir `
    -B $NativeBuildDir `
    -G "Visual Studio 17 2022" `
    -A x64

if ($LASTEXITCODE -ne 0)
{
    throw "CMake configure failed."
}

cmake `
    --build $NativeBuildDir `
    --config Release

if ($LASTEXITCODE -ne 0)
{
    throw "Native bridge build failed."
}

if (-not (Test-Path $NativeDll))
{
    throw "DiscordSocialBridge.dll was not found: $NativeDll"
}

Write-Host ""
Write-Host "Native bridge build complete." `
    -ForegroundColor Green

Write-Host ""

# ============================================================
# Publish application
# ============================================================

Write-Host "[3/5] Publishing application..." `
    -ForegroundColor Yellow

dotnet publish $ProjectFile `
    -c Release `
    -r win-x64 `
    --self-contained true

if ($LASTEXITCODE -ne 0)
{
    throw "dotnet publish failed."
}

if (-not (Test-Path $PublishedExe))
{
    throw "DiscordPresence.exe was not found in: $PublishDir"
}

if (-not (Test-Path $PublishedBridgeDll))
{
    throw "DiscordSocialBridge.dll was not copied to publish output."
}

if (-not (Test-Path $PublishedSdkDll))
{
    throw "discord_partner_sdk.dll was not copied to publish output."
}

Write-Host ""
Write-Host "Publish complete." `
    -ForegroundColor Green

Write-Host ""

# ============================================================
# Find Inno Setup
# ============================================================

Write-Host "[4/5] Locating Inno Setup compiler..." `
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
# Build installer
# ============================================================

Write-Host "[5/5] Compiling installer..." `
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
# Cleanup build output
# ============================================================

Write-Host ""
Write-Host "Cleaning temporary build output..." `
    -ForegroundColor Yellow

$CleanupDirectories =
    @(
        (Join-Path $Root "bin")
        (Join-Path $Root "obj")
        $NativeBuildDir
    )

foreach ($Directory in $CleanupDirectories)
{
    if (Test-Path $Directory)
    {
        Remove-Item `
            $Directory `
            -Recurse `
            -Force
    }
}

Write-Host "Temporary build output cleaned." `
    -ForegroundColor Green

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