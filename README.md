# Discord Presence

A lightweight Windows app that automatically updates your Discord Rich Presence based on the development application you are currently using.

Discord Presence detects supported development tools, project names, and Git repositories, keeps a continuous work-session timer while you switch between applications, and automatically falls back to an Idle presence when no supported application is running.

It uses the **Discord Social SDK** through a small native C++ bridge.

[![Request App Support](https://img.shields.io/badge/Request-App%20Support-5865F2?logo=discord&logoColor=white)](https://github.com/the-real-l1near/DiscordPresence/issues/new?template=app-request.yml)

## Features

- Automatic foreground application detection
- Automatic project name detection
- Automatic Git repository detection
- Dynamic Discord activity name based on the active application
- Continuous elapsed-time tracking across supported applications
- Idle presence when no supported application is running
- Automatic Discord startup detection
- Automatic Rich Presence recovery after Discord restarts
- Automatic Social SDK client reinitialization when needed
- Automatic game override for fullscreen and borderless games
- Suspends custom Rich Presence while gaming
- Restores the previous development presence after leaving a game
- System tray support
- Start minimized
- Start with Windows
- Manual presence refresh and clear
- Per-user settings stored locally
- No Discord bot token required
- Self-contained Windows x64 build

## Supported Applications

| Application | Process | Rich Presence asset |
| --- | --- | --- |
| Visual Studio Code | `Code.exe` | `vscode` |
| Blender | `blender.exe` | `blender` |
| Unreal Engine | `UnrealEditor.exe` | `unreal_v2` |
| IntelliJ IDEA | `idea64.exe` | `intellij` |

When no supported application is running, Discord Presence switches to the `idle_v2` asset.

## How It Works

Discord Presence checks the active Windows foreground application once per second.

When a supported application is detected, the app builds a Discord activity using:

```text
<Application>

Working on <project>
Repo: <repository>

Elapsed time
```

For example:

```text
Visual Studio Code

Working on DiscordPresence
Repo: DiscordPresence

01:32:18 elapsed
```

The activity name itself changes based on the currently detected development application.

Switching between supported applications or projects does **not** reset the work-session timer.

For example:

```text
Visual Studio Code
→ Blender
→ Unreal Engine
→ IntelliJ IDEA
```

can all remain part of the same work session.

The timer resets only after all supported applications have been closed and Discord Presence enters Idle.

If you temporarily switch to an unsupported application such as Discord, Chrome, Explorer, or Spotify while a supported development application is still running, the most recent development presence is kept.

## Idle Presence

When no supported application is running, Discord Presence uses:

```text
Idle
Touching grass...
...allegedly
```

The Idle activity does not use the work-session timer.

## Game Override

Discord Presence automatically suspends its custom Rich Presence when a fullscreen or borderless game becomes the foreground application.

The flow is:

```text
Development app active
→ Discord Presence is shown

Game becomes foreground
→ Discord Presence suspends its Social SDK client
→ Discord can show the game's own activity

Game loses foreground / exits
→ Social SDK client is reinitialized
→ Previous development presence is restored
```

The current work session is preserved while gaming.

For example, if Visual Studio Code has been active for 45 minutes before opening a game, returning from the game restores the same work session rather than starting a new timer.

Game detection currently focuses on fullscreen and borderless foreground applications while excluding common non-game applications such as browsers, Discord, Explorer, supported development tools, and media players.

## Discord Startup and Restart Recovery

Discord Presence can start before the Discord desktop client.

When Discord is detected starting:

```text
Discord process appears
→ Social SDK client is reinitialized
→ current presence is resent for a short recovery window
```

This also allows Rich Presence to recover automatically after Discord is closed, crashes, or is restarted.

The recovery system combines Discord process detection with a short retry window so the app does not continuously retry while Discord is not running.

## Discord Social SDK

Discord Presence uses the Discord Social SDK instead of the legacy Discord RPC library.

The managed C# application communicates with the SDK through:

```text
DiscordSocialClient.cs
        ↓
DiscordSocialBridge.dll
        ↓
discord_partner_sdk.dll
        ↓
Discord Desktop
```

The native bridge is written in C++ and exposes only the small set of functions needed by the application.

## Installation

Download the latest Windows installer from the **Releases** page:

```text
DiscordPresence-Setup-x.x.x.exe
```

For v1.2.0:

```text
DiscordPresence-Setup-1.2.0.exe
```

The installer uses a self-contained Windows x64 build, so a separate .NET Runtime installation is not required.

The default installation directory is:

```text
%LOCALAPPDATA%\Programs\DiscordPresence
```

The installer allows you to choose a different installation directory before installation.

No administrator privileges are required for the default per-user installation.

## Start with Windows

Enable **Start with Windows** inside Discord Presence.

The startup entry is stored under the current user's Windows startup registry key, so administrator privileges are not required.

If you manually move `DiscordPresence.exe` after enabling startup, disable and re-enable **Start with Windows** so the stored path is updated.

## Building From Source

### Requirements

- Windows 10/11 x64
- .NET 10 SDK
- Discord desktop client
- CMake
- Visual Studio 2022 Build Tools
- MSVC v143 C++ toolchain
- Windows SDK
- Inno Setup 7 for building the installer

Clone the repository:

```powershell
git clone https://github.com/the-real-l1near/DiscordPresence.git
cd DiscordPresence
```

### Build the native bridge

Configure:

```powershell
cmake -S .\native\DiscordSocialBridge `
      -B .\native\DiscordSocialBridge\build `
      -G "Visual Studio 17 2022" `
      -A x64
```

Build:

```powershell
cmake --build .\native\DiscordSocialBridge\build --config Release
```

The native bridge will be generated at:

```text
native\DiscordSocialBridge\build\Release\DiscordSocialBridge.dll
```

### Build the application

```powershell
dotnet build -c Release
```

### Run

```powershell
dotnet run
```

### Publish

```powershell
dotnet publish .\DiscordPresence.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true
```

Published files are placed in:

```text
bin\Release\net10.0-windows\win-x64\publish\
```

The published application includes:

```text
DiscordPresence.exe
DiscordSocialBridge.dll
discord_partner_sdk.dll
```

The project intentionally uses a **self-contained build with separate native dependencies** rather than a single-file bundle.

## Building the Installer

Install Inno Setup 7:

```powershell
winget install --id JRSoftware.InnoSetup.7 -e
```

If PowerShell blocks local scripts for the current session:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

Then build the complete installer:

```powershell
.\build-installer.ps1
```

The installer build script automatically:

```text
1. Cleans previous build output
2. Configures the native bridge with CMake
3. Builds DiscordSocialBridge.dll
4. Publishes the self-contained Windows x64 application
5. Verifies the required native DLLs
6. Locates the Inno Setup compiler
7. Builds the installer
8. Cleans temporary build output
```

The finished installer is kept in:

```text
dist\
```

For v1.2.0:

```text
dist\DiscordPresence-Setup-1.2.0.exe
```

Temporary directories such as:

```text
bin\
obj\
native\DiscordSocialBridge\build\
```

are removed after a successful installer build.

## Project Structure

```text
DiscordPresence/
├── ActiveAppDetector.cs
├── AppPresenceProfile.cs
├── AppSettings.cs
├── CloseActionDialog.cs
├── DiscordSocialClient.cs
├── GameDetector.cs
├── GitRepositoryDetector.cs
├── MainForm.cs
├── Program.cs
├── ProjectNameDetector.cs
├── SettingsService.cs
├── StartupService.cs
├── DiscordPresence.csproj
├── icon.ico
├── build-installer.ps1
│
├── installer/
│   └── DiscordPresence.iss
│
└── native/
    ├── DiscordSocialBridge/
    │   ├── DiscordSocialBridge.cpp
    │   ├── DiscordSocialBridge.h
    │   └── CMakeLists.txt
    │
    └── sdk/
        ├── bin/
        │   └── discord_partner_sdk.dll
        ├── include/
        │   ├── cdiscord.h
        │   └── discordpp.h
        └── lib/
            └── discord_partner_sdk.lib
```

## Settings

Application settings are stored locally in:

```text
%LOCALAPPDATA%\DiscordPresence\settings.json
```

Current settings include:

- Show elapsed time
- Start minimized

The **Start with Windows** state is managed through the Windows registry.

## Discord Rich Presence Assets

The current Discord application uses these asset keys:

```text
vscode
blender
unreal_v2
intellij
idle_v2
```

If you fork the project and use your own Discord Application ID, upload matching Rich Presence assets in the Discord Developer Portal or update the asset keys and Application ID in the source code.

## Privacy

Discord Presence operates locally.

It reads only the information required for its features, including:

- running process information
- the current foreground application
- application window titles
- local Git repository information used for repository detection

It does not require a Discord bot token.

It does not read or send Discord messages.

Rich Presence communication is handled locally through the Discord Social SDK and the Discord desktop client.

## Notes

Project-name detection is based on application window titles, so behavior can vary if an application changes its title format.

Git repository detection depends on the local project/repository layout and may not identify every possible repository configuration.

Some applications use multiple background processes. Discord Presence checks for visible top-level application windows when deciding whether supported applications or Discord itself are active.

Fullscreen/borderless detection is intentionally conservative to reduce false game detections.

## License

This project is licensed under the MIT License.
