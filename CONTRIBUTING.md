# Contributing

## Development

### Initial setup

The original game files are required but aren't included in this repository. To
set up your development environment:

1. Clone the repository:
   ```sh
   git clone https://github.com/ctristan/lobotomy-corporation-mods.git
   cd lobotomy-corporation-mods
   ```

2. Install the .NET 10 SDK (required for test projects and CI parity):
   ```sh
   dotnet --version
   ```

   The output should start with `10.`.

3. Install ilspycmd (decompiler tool):
   ```sh
   dotnet tool install --global ilspycmd
   ```

4. Run the SetupExternal tool to copy game DLLs and decompile the main assembly:
   ```sh
   dotnet tool restore
   dotnet setup
   ```

   Optional: Enable debug logging with `--debug` flag for troubleshooting:
   ```sh
   dotnet setup --debug
   ```

   The tool will automatically search for your Lobotomy Corporation installation
   on:

- **Windows**: `C:\Program Files (x86)\Steam\steamapps\common\LobotomyCorp`
- **Linux**: `~/.steam/steam/steamapps/common/LobotomyCorp/` or
  `~/.local/share/Steam/steamapps/common/LobotomyCorp/`
- **macOS (CrossOver)**: CrossOver bottles AND external Steam libraries (
  mounted volumes like `/Volumes/*`)

If the game isn't found in these locations, specify the path manually:

   ```sh
   dotnet setup --path "/path/to/LobotomyCorp"
   ```

5. Build the solution:
   ```sh
   dotnet build LobotomyCorporationMods.sln
   ```

### Environment setup

The SetupExternal tool copies the required game DLLs to the `external/`
directory and creates a decompiled view of
`Assembly-CSharp.dll` in `external/decompiled/` for reference while developing
mods.

My current environment setup is a "`src`" folder in the BaseMods folder that I
placed the repo in, so
my folder structure for the repo is
LobotomyCorp_Data/BaseMods/src/lobotomy-corporation-mods. If you follow this
same structure, then references should use the game's files and will build
the output to the appropriate BaseMod folder e.g.,
LobotomyCorp_Data/BaseMods/LobotomyCorporationMods.BadLuckProtectionForGifts.

If you're running Linux, make sure mono-devel is installed. You should be able
to open the solution in VSCode or VSCodium and build with xbuild. That said, I
wouldn't recommend it as I haven't found a way to be able to debug Unity .NET
Framework DLLs on Linux like I can on Windows with dnSpy, especially since the
game has to run in Proton. If you're able to get debugging working on Linux, I
would love to hear about it!

### CrossOver (macOS) configuration

If you're running the game through CrossOver on macOS, you need to configure the
Wine bottle so that Doorstop (the mod loader used by BepInEx/BaseMods) can
bootstrap correctly. Without this, the `winhttp.dll` proxy that Doorstop ships
will be ignored and mods won't load.

1. Open CrossOver and select the bottle containing Lobotomy Corporation.
2. Open the bottle settings and run **winecfg**.
3. Go to the **Libraries** tab.
4. In the "New override for library" field, type `winhttp` and click **Add**.
5. Select `winhttp` in the list, click **Edit**, and set the load order to
   **Native then Builtin** (`native,builtin`).
6. Click **OK** to save and close winecfg.

This is the standard Doorstop/BepInEx fix referenced by the
[BepInEx documentation](https://docs.bepinex.dev/articles/advanced/steam_interop.html)
and community maintainers. It ensures Wine uses the proxy `winhttp.dll` shipped
with the mod loader instead of its own built-in version.

### CI Checks

To run the same checks that the GitHub Actions workflow runs, use the CI tool:

- Run checks locally (with auto-fix):
  ```sh
  dotnet ci
  ```

- Run checks in verify mode (no auto-fix):
  ```sh
  dotnet ci --check
  ```

- Install the pre-commit git hook to run these checks automatically before every
  commit:
  ```sh
  dotnet ci --setup-hooks
  ```

#### Coverage Thresholds

The CI tool enforces code coverage thresholds based on the configuration in
`coverlet.json` at the repository root. If the file is missing, coverage thresholds
will not be enforced.

The configuration supports three coverage types (line, branch, and method) with
independent thresholds:

```json
{
  "lineThreshold": 80,
  "branchThreshold": 70,
  "methodThreshold": 75
}
```

- `lineThreshold`: Minimum percentage of lines that must be covered (default: 80%)
- `branchThreshold`: Minimum percentage of branches that must be covered (default: 70%)
- `methodThreshold`: Minimum percentage of methods that must be covered (default: 75%)

The CI tool calculates coverage across all test projects and fails the build if **any** module's coverage falls below the configured thresholds.

### Rebuilding local dotnet tools

The repository includes local dotnet tools (CI and SetupExternal) that are
packaged as NuGet packages. Use the helper scripts in `scripts/` to rebuild and
update these tools automatically with proper cache clearing:

```sh
# Reinstall all local tools (CI) - use this after updating any tool
./scripts/tool-reinstall.sh

# Reinstall a specific tool
./scripts/tool-reinstall.sh ci

# Reinstall the setup tool
./scripts/setup-reinstall.sh
```

The reinstall scripts exist because `dotnet tool install --local` will silently use
the NuGet cache instead of a freshly packed nupkg unless the cache is cleared first.
Always use these scripts when updating local tools to ensure the latest changes are picked up.

For more details, see `scripts/README.md`.

### Coding standards

Due to nature of Harmony patching, there are going to be some quirks on how
to do things that you wouldn't do in a reasonable codebase.

- Mod projects need to use .NET Framework 3.5.
  - The original game was created an old Unity version that used .NET Framework
    3.5, so we need to use the same version for our mods.
- Every mod needs to have a "Harmony_Patch" class in the project root.
  - Basemod requires this name for the class that initializes Harmony
    patches that the mod will load.
- Harmony patches must be Postfix unless the patch will not work if it's not a
  Prefix. If Prefix is required, a comment needs to display why Postfix won't
  work.
  - Prefix methods are potentially dangerous for both the game and other mods
    since you're changing game state before the method runs. Running our Postfix
    patch allows us to do things after the method ran so that the game and other
    mods are much less likely to misbehave.
  - That said, patches only need to be Postfix when shared with others. I
    would recommend creating your patches using Prefix for the best-case
    scenario, then when everything works as expected, changing all of your
    patches to Postfix and re-testing your mod to make sure they don't break.
