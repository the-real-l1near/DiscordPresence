DiscordPresence Installer Kit

Put these files into the root of your DiscordPresence repo:

DiscordPresence\
├── build-installer.ps1
├── installer\
│   └── DiscordPresence.iss
├── DiscordPresence.csproj
├── icon.ico
└── ...

Then run:

    .\build-installer.ps1

If PowerShell blocks local scripts for this session:

    Set-ExecutionPolicy -Scope Process Bypass

Then run the build script again.

The script:
1. Publishes the self-contained win-x64 build.
2. Finds Inno Setup 7 or 6.
3. Builds dist\DiscordPresence-Setup-1.0.0.exe

The installer:
- Installs per-user to %LOCALAPPDATA%\Programs\DiscordPresence
- Does not require admin rights
- Includes the complete self-contained publish folder
- Creates a Start Menu shortcut
- Offers an optional desktop shortcut
- Adds an uninstaller
- Removes the app's HKCU startup entry during uninstall
- Leaves user settings in %LOCALAPPDATA%\DiscordPresence intact
