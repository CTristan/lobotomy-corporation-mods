# Helper Scripts

This directory contains helper scripts to streamline common development workflows.

## tool-reinstall.sh

Reinstalls the **playwright** local dotnet tool with proper cache clearing.

```bash
./scripts/tool-reinstall.sh              # Reinstall playwright (default)
./scripts/tool-reinstall.sh playwright   # Reinstall playwright explicitly
```

### What it does

1. Cleans and rebuilds the LobotomyPlaywright project
2. Packs the new version to `nupkg/`
3. Removes the NuGet cache (`~/.nuget/packages/lobotomycorporationmods.playwright/1.0.0`)
4. Uninstalls the local tool
5. Reinstalls from the packed nupkg

### Why this script is necessary

When you update the LobotomyPlaywright dotnet tool, the following steps are required:

1. **Clean and rebuild** - Changes need to be compiled
2. **Repack** - The nupkg needs the new compiled DLL
3. **Clear NuGet cache** - Dotnet caches packages and won't pick up the new version otherwise
4. **Uninstall** - The old version must be removed
5. **Reinstall** - Install the new version

Without clearing the NuGet cache, `dotnet tool install --local` will reinstall the cached old version instead of the newly built one.

## setup-reinstall.sh

Reinstalls the **setup** local dotnet tool with proper cache clearing.

```bash
./scripts/setup-reinstall.sh              # Reinstall setup (default)
./scripts/setup-reinstall.sh setup       # Reinstall setup explicitly
```

This follows the same workflow as `tool-reinstall.sh` but for the SetupExternal project.

## Usage Example

```bash
# After making changes to LobotomyPlaywright or its plugins:
./scripts/tool-reinstall.sh

# After making changes to SetupExternal:
./scripts/setup-reinstall.sh

# Now the tool is ready to use with the latest changes:
dotnet playwright deploy
dotnet playwright launch
```
