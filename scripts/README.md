# Helper Scripts

This directory contains helper scripts to streamline common development workflows.

## tool-reinstall.sh

Reinstalls **all** local dotnet tools (ci, playwright) with proper cache clearing.

```bash
./scripts/tool-reinstall.sh              # Reinstall all local tools (default)
./scripts/tool-reinstall.sh all         # Reinstall all local tools
./scripts/tool-reinstall.sh ci          # Reinstall ci only
./scripts/tool-reinstall.sh playwright  # Reinstall playwright only
```

### What it does

For each tool:

1. Cleans and rebuilds the project
2. Packs the new version to `nupkg/`
3. Removes the NuGet cache (`~/.nuget/packages/lobotomycorporationmods.{tool}/1.0.0`)
4. Uninstalls the local tool
5. Reinstalls from the packed nupkg

### Why this script is necessary

When you update any local dotnet tool, the following steps are required:

1. **Clean and rebuild** - Changes need to be compiled
2. **Repack** - The nupkg needs the new compiled DLL
3. **Clear NuGet cache** - Dotnet caches packages and won't pick up the new version otherwise
4. **Uninstall** - The old version must be removed
5. **Reinstall** - Install the new version

Without clearing the NuGet cache, `dotnet tool install --local` will reinstall the cached old version instead of the newly built one.

### When to use

**Use this script whenever you update local tools:**

- After modifying `CI/` project code → run `./scripts/tool-reinstall.sh`
- After modifying `LobotomyPlaywright/` project code → run `./scripts/tool-reinstall.sh`
- After modifying both → run `./scripts/tool-reinstall.sh` (no arguments, reinstalls all)

## setup-reinstall.sh

Reinstalls the **setup** local dotnet tool with proper cache clearing.

```bash
./scripts/setup-reinstall.sh              # Reinstall setup (default)
./scripts/setup-reinstall.sh setup       # Reinstall setup explicitly
```

This follows the same workflow as `tool-reinstall.sh` but for the SetupExternal project.

### When to use

**Use this script whenever you update the setup tool:**

- After modifying `SetupExternal/` project code → run `./scripts/setup-reinstall.sh`

## Usage Example

```bash
# After making changes to LobotomyPlaywright or its plugins:
./scripts/tool-reinstall.sh

# After making changes to CI:
./scripts/tool-reinstall.sh

# After making changes to SetupExternal:
./scripts/setup-reinstall.sh

# Now the tools are ready to use with the latest changes:
dotnet ci
dotnet playwright deploy
dotnet playwright launch
dotnet setup
```
