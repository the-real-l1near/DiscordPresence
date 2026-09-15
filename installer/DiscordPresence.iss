; DiscordPresence installer
; Build with Inno Setup 7 (or 6.1+) after publishing the framework-dependent win-x64 build.

#define MyAppName "Discord Presence"
#define MyAppExeName "DiscordPresence.exe"
#define MyAppVersion "1.3.4"
#define MyAppId "DiscordPresence.App"
#define DotNetDesktopRuntimeUrl "https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe"
#define DotNetDesktopRuntimeInstaller "windowsdesktop-runtime-10-x64.exe"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}

DefaultDirName={localappdata}\Programs\DiscordPresence
DefaultGroupName={#MyAppName}

; Luôn cho phép user chọn installation directory.
DisableDirPage=no

DisableProgramGroupPage=yes

; Discord Presence itself remains per-user. If .NET 10 Desktop Runtime is missing,
; only the Microsoft runtime installer requests elevation.
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

OutputDir=..\dist
OutputBaseFilename=DiscordPresence-Setup-{#MyAppVersion}
SetupIconFile=..\icon.ico

Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
Uninstallable=yes

CloseApplications=yes
CloseApplicationsFilter=DiscordPresence.exe,DiscordSocialBridge.dll,discord_partner_sdk.dll
RestartApplications=no

VersionInfoVersion=1.3.4.0
VersionInfoProductName={#MyAppName}
VersionInfoDescription=Automatic Discord Rich Presence for supported desktop applications.
VersionInfoProductVersion={#MyAppVersion}

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

; Upgrading from older self-contained releases can leave private .NET runtime files
; beside DiscordPresence.exe. Clear the entire application directory first whenever
; an existing Discord Presence installation is detected, then install a clean package.
; User settings are stored outside {app}, under %LOCALAPPDATA%\DiscordPresence.
[InstallDelete]
Type: filesandordirs; Name: "{app}\*"; Check: ShouldCleanInstallDirectory

[Files]
Source: "..\bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
var
  DownloadPage: TDownloadWizardPage;

function ShouldCleanInstallDirectory: Boolean;
begin
  Result := FileExists(ExpandConstant('{app}\{#MyAppExeName}'));

  if Result then
  begin
    Log('Existing Discord Presence installation detected. Clearing application directory before install.');
  end;
end;

function HasDotNet10DesktopRuntimeInRegistry: Boolean;
var
  ValueNames: TArrayOfString;
  I: Integer;
begin
  Result := False;

  if RegGetValueNames(
    HKLM64,
    'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App',
    ValueNames
  ) then
  begin
    for I := 0 to GetArrayLength(ValueNames) - 1 do
    begin
      if Pos('10.', ValueNames[I]) = 1 then
      begin
        Result := True;
        Exit;
      end;
    end;
  end;
end;

function HasDotNet10DesktopRuntimeInProgramFiles: Boolean;
var
  RuntimeDirectory: String;
  FindRec: TFindRec;
begin
  Result := False;
  RuntimeDirectory := ExpandConstant('{pf64}\dotnet\shared\Microsoft.WindowsDesktop.App');

  if FindFirst(RuntimeDirectory + '\10.*', FindRec) then
  begin
    try
      repeat
        if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
        begin
          Result := True;
          Exit;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

function IsDotNet10DesktopRuntimeInstalled: Boolean;
begin
  Result :=
    HasDotNet10DesktopRuntimeInRegistry or
    HasDotNet10DesktopRuntimeInProgramFiles;
end;

procedure InitializeWizard;
begin
  DownloadPage := CreateDownloadPage(
    'Downloading Microsoft .NET 10 Desktop Runtime',
    'Discord Presence requires the .NET 10 Desktop Runtime. Setup will download it directly from Microsoft.',
    nil
  );
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
  RuntimeInstallerPath: String;
begin
  Result := '';

  if IsDotNet10DesktopRuntimeInstalled then
  begin
    Log('.NET 10 Desktop Runtime x64 is already installed.');
    Exit;
  end;

  Log('.NET 10 Desktop Runtime x64 was not found. Downloading from Microsoft.');

  DownloadPage.Clear;
  DownloadPage.Add(
    '{#DotNetDesktopRuntimeUrl}',
    '{#DotNetDesktopRuntimeInstaller}',
    ''
  );

  DownloadPage.Show;
  try
    try
      DownloadPage.Download;
    except
      Result :=
        'Could not download the Microsoft .NET 10 Desktop Runtime.' + #13#10 +
        GetExceptionMessage;
      Exit;
    end;
  finally
    DownloadPage.Hide;
  end;

  RuntimeInstallerPath :=
    ExpandConstant('{tmp}\{#DotNetDesktopRuntimeInstaller}');

  Log('Installing .NET 10 Desktop Runtime x64.');

  if not ShellExec(
    'runas',
    RuntimeInstallerPath,
    '/install /passive /norestart',
    '',
    SW_SHOW,
    ewWaitUntilTerminated,
    ResultCode
  ) then
  begin
    Result :=
      'Could not start the Microsoft .NET 10 Desktop Runtime installer.' + #13#10 +
      'Windows error code: ' + IntToStr(ResultCode);
    Exit;
  end;

  if (ResultCode = 3010) or (ResultCode = 1641) then
  begin
    NeedsRestart := True;
  end
  else if ResultCode <> 0 then
  begin
    Result :=
      'Microsoft .NET 10 Desktop Runtime installation failed.' + #13#10 +
      'Installer exit code: ' + IntToStr(ResultCode);
    Exit;
  end;

  if not IsDotNet10DesktopRuntimeInstalled then
  begin
    Result :=
      'Microsoft .NET 10 Desktop Runtime installation completed, but Setup could not detect the runtime.' + #13#10 +
      'Please install the .NET 10 Desktop Runtime x64 manually, then run this installer again.';
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    { Remove the startup entry created by the app itself, if present. }
    RegDeleteValue(
      HKCU,
      'Software\Microsoft\Windows\CurrentVersion\Run',
      'DiscordPresence'
    );
  end;
end;
