# Plan: RetargetHarmony Installer

## Context

RetargetHarmony currently ships with a 305-line Windows batch script
(`RetargetHarmony/Harmony2-uninstall.bat`) as its only uninstaller, and has no
installer at all — deployment is fully manual. Users must copy files to specific
subdirectories within their game folder. This is error-prone and not suitable for
public release.

### What gets installed today (manually)

| File | Source | Destination (relative to game root) |
|------|--------|-------------------------------------|
| `RetargetHarmony.dll` | `RetargetHarmony/bin/Release/net35/` | `BepInEx/patchers/RetargetHarmony/` |
| `0Harmony109.dll` | `RetargetHarmony/lib/` | `BepInEx/core/` |
| `0Harmony12.dll` | `RetargetHarmony/lib/` | `BepInEx/core/` |
| BepInEx 5 distribution | External download | Game root (`winhttp.dll`, `doorstop_config.ini`, `BepInEx/`) |

### Existing infrastructure to reuse

- **`GamePathFinder`** (`SetupExternal/GamePathFinder.cs`) — multi-platform Steam
  game directory detection via registry (Windows), common paths (Linux), and
  CrossOver bottle scanning (macOS).
- **`VdfParser`** (`SetupExternal/VdfParser.cs`) — parses Steam's
  `libraryfolders.vdf` to find alternate library locations.
- **Mono.Cecil** — already a project dependency, can be used for assembly analysis
  during uninstall to detect which BaseMods reference BepInEx/Harmony 2.

## Goals

1. **Single Avalonia GUI application** that handles both installation and
   uninstallation of BepInEx 5 + RetargetHarmony
2. **Auto-detect game directory** using the same logic as SetupExternal, with a
   manual browse fallback
3. **Bundle BepInEx 5** so users don't need to source it separately
4. **Smart uninstaller** that uses Mono.Cecil to scan `BaseMods/` for DLLs
   referencing BepInEx or Harmony 2+, warns the user, and offers to remove them
5. **Lobotomy Corporation-themed UI** — dark color scheme with amber/gold accents,
   game-style alert dialogs — while keeping text clear and unambiguous
6. **Cross-platform potential** — self-contained `.exe` for Windows (primary),
   framework-dependent builds for macOS/Linux

## Approach

Build an Avalonia desktop app in `RetargetHarmony.Installer/` that presents a
simple mode selector (Install / Uninstall), auto-detects the game path, and
performs the file operations with progress feedback. The app reuses
`GamePathFinder` and `VdfParser` from SetupExternal rather than reimplementing
path detection.

For theming, use Avalonia's styling system to create a custom dark theme with
the game's visual language — dark grays/blacks, amber/gold accents, and
warning-style confirmation dialogs reminiscent of the game's alert UI.

For the uninstaller's assembly analysis, use Mono.Cecil to inspect each DLL in
`BaseMods/` and check whether it references `0Harmony` version 2.0+ or any
BepInEx assembly. This replaces the batch script's audit-log approach with a
reliable, log-independent method.

### Target framework

The installer targets a modern .NET version (net10.0 or whatever the solution's
current modern target is) since it runs on the user's machine outside the game.
It does NOT need to target net35.

## Tasks

### Phase 1: Project scaffolding

- [x] Create `RetargetHarmony.Installer/` Avalonia project
  - Manual csproj setup with `WinExe` output type
  - Added to `LobotomyCorporationMods.sln`
  - Targets `net10.0` with `<Nullable>enable</Nullable>`
  - Avalonia packages (`Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`) added to `Directory.Packages.props`
  - `Mono.Cecil` package reference for assembly scanning
- [x] Add project reference or shared code for `GamePathFinder` and `VdfParser`
  - Chose adapted copies (not linked files) because `GamePathFinder` had a dependency on `Program.DebugLog`
  - `Services/GamePathFinder.cs` and `Services/VdfParser.cs` adapted with `RetargetHarmony.Installer.Services` namespace
  - `GamePathFinder` implements `IGamePathFinder` interface for testability

### Phase 2: Bundle BepInEx 5

- [x] Download BepInEx 5.4.23.5 stable release (win_x64)
  - Source: `https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5` — `BepInEx_win_x64_5.4.23.5.zip`
  - Windows-only game, so win_x64 is correct even for CrossOver/Proton users
- [x] Add BepInEx distribution files to the repo under `RetargetHarmony.Installer/Resources/bepinex/`
  - Files placed and configured as content files that get copied to output
  - Version pinned at 5.4.23.5 in `Resources/bepinex/README.md`
- [x] Include RetargetHarmony's own distribution files as content references
  - `0Harmony109.dll`, `0Harmony12.dll` linked from `RetargetHarmony/lib/`
  - `RetargetHarmony.dll` referenced from resources directory (must be placed manually or via build script)

### Phase 3: Core logic (platform-agnostic)

- [x] **`GameDirectoryValidator`** — validates a path is a Lobotomy Corporation install
  - Checks for `LobotomyCorp_Data/Managed/Assembly-CSharp.dll`
  - Returns `GameDirectoryValidationResult` with specific error message if invalid
- [x] **`InstallerService`** — install operation
  - Copies BepInEx files from `Resources/bepinex/` to game root recursively
  - Copies `RetargetHarmony.dll` to `BepInEx/patchers/RetargetHarmony/`
  - Copies `0Harmony109.dll` and `0Harmony12.dll` to `BepInEx/core/`
  - Creates directory structure as needed
  - Returns `InstallResult` with list of files created/overwritten
  - Handles "already installed" via overwrite (detection exposed via `IsBepInExInstalled`/`IsRetargetHarmonyInstalled`)
- [x] **`UninstallerService`** — uninstall operation
  - Removes RetargetHarmony patcher directory
  - Removes Harmony interop DLLs from `BepInEx/core/`
  - Removes BepInEx root files (`winhttp.dll`, `doorstop_config.ini`, `.doorstop_version`)
  - Removes `BepInEx/` directory
  - Returns `UninstallResult` with lists of files and directories removed
- [x] **`BaseModsAnalyzer`** — Mono.Cecil assembly scanning
  - Scans all `.dll` files in `BaseMods/` recursively
  - Uses `Mono.Cecil.AssemblyDefinition.ReadAssembly()` to inspect references
  - Flags DLLs referencing `0Harmony` version `>= 2.0.0.0`
  - Flags DLLs referencing any `BepInEx.*` assembly
  - Returns list of `FlaggedMod` with dependency type and assembly name
  - Gracefully handles corrupt/non-.NET DLLs (catches `BadImageFormatException`, `IOException`, `InvalidOperationException`)

### Phase 4: Avalonia UI

- [x] **Main window** — single-window design
  - Two prominent buttons: "Install" (amber/gold accent) and "Uninstall" (red danger)
  - Game path auto-detected on startup with Browse and Auto-Detect buttons
  - Status/version info displayed
- [x] **Install flow**
  - Shows installation status and BepInEx/RetargetHarmony detection
  - Success/failure result with file list details in log area
  - Handles "already installed" — shows current status, overwrites on install
- [x] **Uninstall flow**
  - BaseMods analysis results shown in warning panel
  - Lists flagged mods with dependency details
  - Success/failure result with removed files/directories
- [x] **Game path selection**
  - Auto-detect using `GamePathFinder` on startup
  - "Browse..." button with native folder picker dialog
  - Validation feedback (green/red text with message)

### Phase 5: Lobotomy Corporation theming

- [x] **Custom Avalonia theme/styles** (`Themes/LobotomyTheme.axaml`)
  - Background: dark gray/near-black (`#1a1a2e`)
  - Accent color: amber/gold (`#d4a017`) matching the game's UI
  - Text: light gray/white (`#e0e0e0`) for readability
  - Borders/separators: subtle dark borders (`#2a2a4e`)
- [x] **Alert/warning styling**
  - Warning panel with red border (`#d44017`) and dark red background (`#2e1a1a`)
  - Success panel with green border (`#17d440`)
  - Game-inspired bordered panel look with `.panel`, `.warning`, `.success` style classes
- [x] **Typography**
  - Inter font via `Avalonia.Fonts.Inter` for clean readability
  - Styled section headers in amber/gold
  - Monospace log area with `Cascadia Mono`/`Consolas` fallback
- [x] **Optional touches**
  - Window title: "RetargetHarmony Installer" (clear, not confusing)
  - Styled buttons with hover/pressed/disabled states
  - Danger-styled uninstall button, accent-styled install button

### Phase 6: Publishing and distribution

- [x] **Publish profiles** (in `Properties/PublishProfiles/`)
  - `win-x64` self-contained, single file, trimmed
  - `win-x86` self-contained, single file, trimmed
  - `linux-x64` framework-dependent
  - `osx-x64` / `osx-arm64` framework-dependent
- [x] **Build script** (`scripts/publish-installer.sh`)
  - Builds RetargetHarmony (Release) and stages DLL into installer Resources
  - Publishes for all or specific platforms: `./scripts/publish-installer.sh [win-x64|osx-arm64|...]`
  - Self-contained + single-file + trimmed for Windows; framework-dependent for Linux/macOS
  - `RetargetHarmony.dll` is a build artifact (gitignored in Resources/, staged at publish time)

### Phase 7: Tests

- [x] Create `RetargetHarmony.Installer.Test/` project
  - Targets `net10.0`, xUnit + AwesomeAssertions + Moq (matching project conventions)
  - Added to solution, 29 tests all passing
- [x] Test `GameDirectoryValidator` — empty path, null path, nonexistent dir, missing DLL, valid path
- [x] Test `InstallerService` — BepInEx detection, RetargetHarmony detection, file copy operations, BepInEx distribution copy, empty resources
- [x] Test `UninstallerService` — patcher removal, interop DLL removal, root file removal, BepInEx directory removal, flagged mod removal, no-op uninstall
- [x] Test `BaseModsAnalyzer` — Harmony 2 refs, Harmony 1 refs (not flagged), BepInEx refs, clean DLLs, corrupt DLLs, subdirectory scanning

## Risks & Considerations

### File permissions
- Users may have installed the game in `Program Files`, requiring elevated
  permissions to write. The installer should detect this and prompt for elevation
  (on Windows, request UAC if needed) or show a clear error message.

### Antivirus false positives
- Self-contained .NET executables and DLL injection tools (`winhttp.dll` proxy)
  are commonly flagged by antivirus software. This is a known issue in the
  BepInEx community. Document this for users.

### BaseMods assembly scanning edge cases
- Some DLLs in `BaseMods/` may not be .NET assemblies (native DLLs, data files
  with `.dll` extension). `Mono.Cecil` will throw on these — catch and skip.
- Mods may reference Harmony 1.x (which RetargetHarmony is specifically designed
  to handle). Only flag Harmony 2.0+ references since those are the ones that
  actually depend on the BepInEx Harmony 2 runtime.

### BepInEx version management
- If a user already has BepInEx installed (possibly a different version), the
  installer should detect this and handle accordingly — offer to update, skip,
  or warn about version mismatch.

### Existing batch script
- The `Harmony2-uninstall.bat` can be deprecated once the installer is stable.
  Consider keeping it temporarily for users who already have it, but stop
  distributing it with new releases.

### Self-contained publish size
- Avalonia + .NET self-contained can be 50-150MB+. `PublishTrimmed` and
  `PublishSingleFile` can reduce this significantly but need testing to ensure
  nothing breaks at runtime (Avalonia uses reflection heavily).
  `PublishAot` is another option if Avalonia supports it.
