Discord Presence

A small Windows app that automatically updates your Discord Rich Presence based on the development application you are currently using.

It detects supported apps and project names, keeps one continuous work-session timer while you switch between tools, and falls back to an Idle presence when no supported application is running.

Features

Automatic foreground application detection

Automatic project name detection

Continuous elapsed-time tracking across supported apps

Idle presence when no supported application is running

System tray support

Start minimized

Start with Windows

Manual presence refresh and clear

Per-user settings stored locally

No bot token required

Uses Discord's local Rich Presence / RPC connection

Supported Applications

Application

Process

Rich Presence asset

Visual Studio Code

Code.exe

vscode

Blender

blender.exe

blender

Unreal Engine

UnrealEditor.exe

unreal_v2

IntelliJ IDEA

idea64.exe

intellij

When no supported app is running, Discord Presence switches to the idle_v2 asset.

How It Works

The app checks the active Windows foreground application once per second.

When a supported app is detected, it updates Discord with:

Working on <project>
<application>
Elapsed time

Example:

Working on BasicRotor
Visual Studio Code
01:32:18 elapsed

Switching between supported apps or projects does not reset the work-session timer.

The timer resets only after all supported applications have been closed and the app enters Idle.

If you switch temporarily to an unsupported app such as Discord, Chrome, Explorer, or Spotify while a supported app is still running, the most recent development presence is kept.

Installation

Download the latest Windows installer from the Releases page and run:

DiscordPresence-Setup-x.x.x.exe

The installer uses a self-contained Windows x64 build, so a separate .NET Runtime installation is not required.

By default the app is installed per-user under:

%LOCALAPPDATA%\Programs\DiscordPresence

No administrator privileges are required.

Start with Windows

Enable Start with Windows inside the app.

The startup entry is stored under the current user's Windows startup registry key, so administrator privileges are not required.

If you manually move DiscordPresence.exe after enabling startup, disable and re-enable Start with Windows so the stored path is updated.

Building From Source

Requirements

Windows 10/11 x64

.NET 10 SDK

Discord desktop client

Inno Setup 7 if you want to build the installer

Clone the repository:

git clone https://github.com/YOUR_USERNAME/DiscordPresence.git
cd DiscordPresence

Build:

dotnet build -c Release

Run:

dotnet run

Publish a self-contained Windows x64 build:

dotnet publish -c Release -r win-x64 --self-contained true

Published files will be placed in:

bin\Release\net10.0-windows\win-x64\publish\

The project intentionally uses a self-contained build with separate dependencies rather than a single-file bundle.

Building the Installer

Install Inno Setup 7:

winget install --id JRSoftware.InnoSetup.7 -e -s winget -i

Then run:

.\build-installer.ps1

The script will:

Publish the self-contained Windows x64 build

Locate the Inno Setup compiler

Build the installer

The installer output is written to:

dist\

Example:

dist\DiscordPresence-Setup-1.0.0.exe

Project Structure

DiscordPresence/
├── ActiveAppDetector.cs
├── AppPresenceProfile.cs
├── AppSettings.cs
├── CloseActionDialog.cs
├── MainForm.cs
├── Program.cs
├── ProjectNameDetector.cs
├── SettingsService.cs
├── StartupService.cs
├── DiscordPresence.csproj
├── icon.ico
├── build-installer.ps1
└── installer/
    └── DiscordPresence.iss

Settings

Application settings are stored locally in:

%LOCALAPPDATA%\DiscordPresence\settings.json

Current settings include:

Show elapsed time

Start minimized

The Start with Windows state is managed through the Windows registry.

Discord Rich Presence Assets

The current Discord application uses these asset keys:

vscode
blender
unreal_v2
intellij
idle_v2

If you fork the project and use your own Discord Application ID, upload matching Rich Presence assets in the Discord Developer Portal or update the keys in MainForm.cs.

Privacy

Discord Presence only reads local information needed to determine the currently active supported application and its window title.

It does not require a Discord bot token and does not read or send Discord messages.

Notes

Project-name detection is based on application window titles, so behavior can vary if an application changes its title format.

Some applications use multiple background processes. The app checks for a visible application window when deciding whether a supported app is still running.

License

This project is licensed under the MIT License.