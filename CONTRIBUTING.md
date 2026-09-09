# Contributing

Thanks for helping improve Discord Presence.

## Requesting support for a new app

Please open an **App support request** issue and provide:

- Official application name
- Windows process name
- Several real window-title examples
- How the project/workspace name appears in the title
- An official logo/icon source when available
- Whether you can test a development build

Requests without enough information to identify the process or parse the project name may be marked as needing more information.

## App support flow

```text
App request
    ↓
Validation
    ↓
Accepted
    ↓
Add app profile
    ↓
Add / update project-name parser
    ↓
Prepare Discord Rich Presence asset
    ↓
Test
    ↓
Merge
    ↓
Release
```

## Implementation notes

Adding an app usually involves two parts.

### 1. App profile

Add the Windows process name and Discord Rich Presence metadata to `AppProfiles` in `MainForm.cs`.

Example:

```csharp
["devenv"] = new(
    "Visual Studio",
    "visualstudio",
    "Visual Studio"
),
```

The dictionary key is the process name without `.exe`.

### 2. Project-name detection

If the app exposes its current project/workspace in the window title, add a parser to `ProjectNameDetector.cs`.

The parser should:

- Avoid returning file names when a project/workspace name is available
- Handle common title separators consistently
- Return `null` when the project name cannot be determined reliably
- Be based on real window-title examples from the requested application

## Icon / logo requirements

A new supported app should have a matching Discord Rich Presence asset.

Preferred source image:

- PNG
- Square (1:1)
- 512x512 or larger preferred
- Transparent background preferred
- Official application icon/logo
- No screenshots
- No unrelated fan-made artwork
- Avoid extra text unless it is part of the official logo
- Do not upscale a very small source image just to meet the preferred resolution
- Provide the original/official source URL when possible

The requester or contributor does **not** need to prepare the final Discord asset.
Providing an official source URL is enough. A maintainer can crop, resize, or otherwise prepare the final asset before upload.

### Asset key convention

Discord asset keys should normally be:

- lowercase
- without spaces
- short
- descriptive
- stable across application versions

Good examples:

```text
vscode
blender
visualstudio
rider
androidstudio
godot
pycharm
```

Avoid keys like:

```text
visual_studio_2026_final_icon_v3
new_app_logo_latest
```

Existing project asset keys include:

```text
vscode
blender
unreal_v2
intellij
idle_v2
```

A version suffix such as `_v2` should only be used when there is a practical reason,
such as replacing a cached Discord asset while keeping an older key intact.

## Discord Rich Presence assets

The asset key in code must match the uploaded Discord Developer Application asset key exactly.

Contributors do not need access to the production Discord Developer Application.
A maintainer uploads or replaces production assets before release.

## Testing

Before opening a pull request, verify:

- The application is detected while its main window is open
- The correct project/workspace name is shown
- Switching to another supported app does not reset the work-session timer
- Closing all supported apps switches to Idle
- Unsupported foreground apps do not overwrite the last supported presence
- `dotnet build -c Release` completes without warnings or errors

## Pull requests

Keep app-support changes focused. Avoid unrelated refactors in the same pull request.

Include the related app-request issue in the pull request description.
