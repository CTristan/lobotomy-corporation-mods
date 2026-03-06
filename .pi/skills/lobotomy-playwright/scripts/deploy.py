#!/usr/bin/env python3
"""Build and deploy LobotomyPlaywright plugin DLLs to the game."""

import argparse
import json
import os
import shutil
import subprocess
import sys
from pathlib import Path
from typing import Dict

# Configuration file path
CONFIG_PATH = Path(__file__).parent.parent / "config.json"

# DLL paths
PLUGIN_DLL_NAME = "LobotomyPlaywright.Plugin.dll"
RETARGET_HARMONY_DLL_NAME = "RetargetHarmony.dll"

# Harmony interop DLLs (vendored from BepInEx/HarmonyInteropDlls)
HARMONY_INTEROP_DLLS = ["0Harmony109.dll", "0Harmony12.dll"]

# BepInEx destination directories
PLUGINS_DIR = "plugins"
PATCHERS_DIR = "patchers"
CORE_DIR = "core"


def load_config() -> Dict:
    """Load the config.json file.

    Returns:
        Configuration dictionary.

    Raises:
        FileNotFoundError: If config file doesn't exist.
        json.JSONDecodeError: If config file is invalid JSON.
    """
    if not CONFIG_PATH.exists():
        raise FileNotFoundError(
            f"Configuration file not found: {CONFIG_PATH}\n"
            f"Run 'python {Path(__file__).parent / 'find_game.py'}' to auto-detect game path."
        )

    with open(CONFIG_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


def run_command(cmd: list[str], cwd: Path, capture: bool = True) -> subprocess.CompletedProcess:
    """Run a command and return the result.

    Args:
        cmd: Command and arguments to run.
        cwd: Working directory for the command.
        capture: Whether to capture stdout/stderr.

    Returns:
        CompletedProcess result.

    Raises:
        subprocess.CalledProcessError: If command fails.
    """
    print(f"Running: {' '.join(cmd)}")

    if capture:
        result = subprocess.run(
            cmd,
            cwd=cwd,
            capture_output=True,
            text=True,
            check=False,
        )
        if result.returncode != 0:
            print(f"STDOUT:\n{result.stdout}")
            print(f"STDERR:\n{result.stderr}")
            result.check_returncode()
        return result
    else:
        subprocess.run(cmd, cwd=cwd, check=True)
        return subprocess.CompletedProcess(cmd, 0, b"", b"")


def build_project(project_path: Path, configuration: str = "Release") -> Path:
    """Build a .NET project and return the path to the built DLL.

    Args:
        project_path: Path to the .csproj file.
        configuration: Build configuration (Debug or Release).

    Returns:
        Path to the built DLL.

    Raises:
        subprocess.CalledProcessError: If build fails.
        FileNotFoundError: If DLL is not found after build.
    """
    project_name = project_path.stem
    print(f"\nBuilding {project_name}...")

    result = run_command(
        ["dotnet", "build", str(project_path), "--configuration", configuration],
        cwd=project_path.parent.parent,
    )

    # Find the output DLL
    dll_path = (
        project_path.parent
        / "bin"
        / configuration
        / "net35"
        / project_name
        / (project_name + ".dll")
    )

    if not dll_path.exists():
        # Try alternate naming (sometimes the project name isn't repeated in the path)
        dll_path = (
            project_path.parent / "bin" / configuration / "net35" / (project_name + ".dll")
        )

    if not dll_path.exists():
        raise FileNotFoundError(
            f"Built DLL not found: {dll_path}\n"
            f"Build output:\n{result.stdout}"
        )

    print(f"Built: {dll_path}")
    print(f"Size: {dll_path.stat().st_size:,} bytes")

    return dll_path


def deploy_dll(source_dll: Path, game_path: Path, dest_subdir: str) -> Path:
    """Deploy a DLL to the BepInEx plugins directory.

    Args:
        source_dll: Path to the source DLL.
        game_path: Path to the game installation.
        dest_subdir: Subdirectory under BepInEx (plugins or patchers).

    Returns:
        Path to the deployed DLL.

    Raises:
        OSError: If copy fails.
    """
    bepinex_path = game_path / "BepInEx"
    dest_dir = bepinex_path / dest_subdir
    dest_dll = dest_dir / source_dll.name

    # Create destination directory if it doesn't exist
    dest_dir.mkdir(parents=True, exist_ok=True)

    print(f"\nDeploying {source_dll.name} to {dest_dir}...")

    # Copy the DLL
    shutil.copy2(source_dll, dest_dll)

    # Verify deployment
    if not dest_dll.exists():
        raise OSError(f"Failed to copy {source_dll.name} to {dest_dll}")

    if dest_dll.stat().st_size == 0:
        raise OSError(f"Deployed DLL is empty: {dest_dll}")

    print(f"Deployed: {dest_dll}")
    print(f"Size: {dest_dll.stat().st_size:,} bytes")

    return dest_dll


def deploy_interop_dlls(repo_root: Path, game_path: Path) -> list[Path]:
    """Deploy Harmony interop DLLs from RetargetHarmony/lib/ to BepInEx/core/.

    Args:
        repo_root: Path to the repository root.
        game_path: Path to the game installation.

    Returns:
        List of paths to deployed DLLs.

    Raises:
        FileNotFoundError: If an interop DLL is not found in RetargetHarmony/lib/.
        OSError: If copy fails.
    """
    lib_dir = repo_root / "RetargetHarmony" / "lib"
    deployed = []

    for dll_name in HARMONY_INTEROP_DLLS:
        source_dll = lib_dir / dll_name
        if not source_dll.exists():
            raise FileNotFoundError(
                f"Harmony interop DLL not found: {source_dll}\n"
                f"Expected vendored DLLs from BepInEx/HarmonyInteropDlls in RetargetHarmony/lib/"
            )

        dest_path = deploy_dll(source_dll, game_path, CORE_DIR)
        deployed.append(dest_path)

    return deployed


def main():
    parser = argparse.ArgumentParser(
        description="Build and deploy LobotomyPlaywright plugin DLLs"
    )
    parser.add_argument(
        "--configuration",
        choices=["Debug", "Release"],
        default="Release",
        help="Build configuration (default: Release)",
    )
    parser.add_argument(
        "--skip-build",
        action="store_true",
        help="Skip building and just deploy existing DLLs",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Show what would be done without actually doing it",
    )

    args = parser.parse_args()

    # Load configuration
    try:
        config = load_config()
    except FileNotFoundError as e:
        print(f"ERROR: {e}")
        sys.exit(1)
    except json.JSONDecodeError as e:
        print(f"ERROR: Invalid JSON in config file: {e}")
        sys.exit(1)

    game_path = Path(config["gamePath"])

    # Verify game path exists
    if not game_path.exists():
        print(f"ERROR: Game path does not exist: {game_path}")
        print("The volume may not be mounted. Run 'python find_game.py' to reconfigure.")
        sys.exit(1)

    # Repository root (parent of .pi directory)
    repo_root = Path(__file__).parent.parent.parent.parent.parent

    # Project paths
    plugin_project = repo_root / "LobotomyPlaywright.Plugin" / "LobotomyPlaywright.Plugin.csproj"
    retharmony_project = repo_root / "RetargetHarmony" / "RetargetHarmony.csproj"

    # Verify projects exist
    if not plugin_project.exists():
        print(f"ERROR: Plugin project not found: {plugin_project}")
        sys.exit(1)

    if not retharmony_project.exists():
        print(f"ERROR: RetargetHarmony project not found: {retharmony_project}")
        sys.exit(1)

    # Build projects
    plugin_dll_path = None
    retharmony_dll_path = None

    if not args.skip_build:
        print("=" * 60)
        print("Building Projects")
        print("=" * 60)

        try:
            plugin_dll_path = build_project(plugin_project, args.configuration)
            retharmony_dll_path = build_project(retharmony_project, args.configuration)
        except subprocess.CalledProcessError as e:
            print(f"\nERROR: Build failed")
            sys.exit(1)
        except FileNotFoundError as e:
            print(f"\nERROR: {e}")
            sys.exit(1)
    else:
        # Use existing DLLs
        print("=" * 60)
        print("Skipping Build (using existing DLLs)")
        print("=" * 60)

        plugin_dll_path = (
            plugin_project.parent
            / "bin"
            / args.configuration
            / "net35"
            / PLUGIN_DLL_NAME
        )

        retharmony_dll_path = (
            retharmony_project.parent
            / "bin"
            / args.configuration
            / "net35"
            / RETARGET_HARMONY_DLL_NAME
        )

        if not plugin_dll_path.exists():
            print(f"ERROR: Plugin DLL not found: {plugin_dll_path}")
            print("Run without --skip-build to build the project first.")
            sys.exit(1)

        if not retharmony_dll_path.exists():
            print(f"ERROR: RetargetHarmony DLL not found: {retharmony_dll_path}")
            print("Run without --skip-build to build the project first.")
            sys.exit(1)

        print(f"Using existing plugin DLL: {plugin_dll_path}")
        print(f"Using existing RetargetHarmony DLL: {retharmony_dll_path}")

    # Deploy DLLs
    print("\n" + "=" * 60)
    print("Deploying DLLs")
    print("=" * 60)

    if args.dry_run:
        print(f"\nWould deploy {PLUGIN_DLL_NAME} to:")
        print(f"  {game_path / 'BepInEx' / PLUGINS_DIR / PLUGIN_DLL_NAME}")
        print(f"\nWould deploy {RETARGET_HARMONY_DLL_NAME} to:")
        print(f"  {game_path / 'BepInEx' / PATCHERS_DIR / RETARGET_HARMONY_DLL_NAME}")
        for dll_name in HARMONY_INTEROP_DLLS:
            print(f"\nWould deploy {dll_name} to:")
            print(f"  {game_path / 'BepInEx' / CORE_DIR / dll_name}")
        print("\nDry run complete. Remove --dry-run to actually deploy.")
        sys.exit(0)

    try:
        deploy_plugin_path = deploy_dll(plugin_dll_path, game_path, PLUGINS_DIR)
        deploy_retharmony_path = deploy_dll(retharmony_dll_path, game_path, PATCHERS_DIR)
        deployed_interop = deploy_interop_dlls(repo_root, game_path)
    except (OSError, FileNotFoundError) as e:
        print(f"\nERROR: Deployment failed: {e}")
        sys.exit(1)

    # Summary
    print("\n" + "=" * 60)
    print("Deployment Summary")
    print("=" * 60)
    print(f"Plugin: {deploy_plugin_path}")
    print(f"Size: {deploy_plugin_path.stat().st_size:,} bytes")
    print(f"RetargetHarmony: {deploy_retharmony_path}")
    print(f"Size: {deploy_retharmony_path.stat().st_size:,} bytes")
    for interop_path in deployed_interop:
        print(f"Interop: {interop_path}")
        print(f"Size: {interop_path.stat().st_size:,} bytes")
    print("=" * 60)
    print("\nDeployment successful!")
    print("\nNext steps:")
    print("  python scripts/launch.py   # Launch the game")
    print("  python scripts/status.py   # Check game status")


if __name__ == "__main__":
    main()
