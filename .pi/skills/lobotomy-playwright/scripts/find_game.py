#!/usr/bin/env python3
"""Game path auto-detection for Lobotomy Corporation installation."""

import argparse
import json
import os
import platform
import re
import shutil
import subprocess
import sys
from pathlib import Path
from typing import Optional, List, Tuple

# Configuration file path
CONFIG_PATH = Path(__file__).parent.parent / "config.json"

# Constants
GAME_FOLDER_NAME = "LobotomyCorp"
STEAM_APPS_FOLDER = "steamapps"
COMMON_FOLDER = "common"
LIBRARY_FOLDERS_FILE = "libraryfolders.vdf"


def parse_vdf_library_paths(vdf_content: str) -> List[str]:
    """Extract library paths from a Steam libraryfolders.vdf file.

    Args:
        vdf_content: The content of the VDF file.

    Returns:
        A list of Steam library folder paths.
    """
    paths = []

    if not vdf_content or not vdf_content.strip():
        return paths

    lines = vdf_content.splitlines()
    for line in lines:
        trimmed = line.strip()
        # Look for "path" followed by a quoted value
        if trimmed.startswith('"path"'):
            match = re.search(r'"path"\s*"([^"]+)"', trimmed)
            if match:
                path = match.group(1)
                if path:
                    paths.append(path)

    return paths


def convert_crossover_path(windows_path: str) -> Optional[str]:
    """Convert CrossOver Windows-style paths to macOS Unix paths.

    Example: "Z:\\Volumes\\WD_BLACK SN770 1TB\\SteamLibrary" → "/Volumes/WD_BLACK SN770 1TB/SteamLibrary"

    Args:
        windows_path: Windows-style path from CrossOver.

    Returns:
        macOS Unix path, or None if not a CrossOver external drive.
    """
    # CrossOver external drives typically start with Z:, X:, etc.
    if windows_path.startswith("Z:\\"):
        # Remove "Z:\" and convert backslashes to forward slashes
        unix_path = windows_path[3:].replace("\\", "/")
        return "/" + unix_path

    # Other drive letters could be added here (C:, D:, etc.)
    return None


def is_valid_game_path(path: Path) -> bool:
    """Validate that a path is a valid Lobotomy Corporation installation.

    Args:
        path: Path to check.

    Returns:
        True if this is a valid game installation, False otherwise.
    """
    assembly_csharp_path = path / "LobotomyCorp_Data" / "Managed" / "Assembly-CSharp.dll"
    return assembly_csharp_path.exists()


def is_bepinex_installed(path: Path) -> bool:
    """Check if BepInEx is installed at the game path.

    Args:
        path: Game installation path.

    Returns:
        True if BepInEx is installed, False otherwise.
    """
    bepinex_dll = path / "BepInEx" / "core" / "BepInEx.dll"
    doorstop_config = path / "doorstop_config.ini"
    return bepinex_dll.exists() and doorstop_config.exists()


def get_steam_library_paths(steam_path: Path) -> List[Path]:
    """Get Steam library paths from a Steam installation.

    Args:
        steam_path: Path to Steam installation.

    Returns:
        List of candidate game paths.
    """
    paths = []

    if not steam_path.exists():
        return paths

    # Parse libraryfolders.vdf
    library_folders_path = steam_path / STEAM_APPS_FOLDER / LIBRARY_FOLDERS_FILE
    if library_folders_path.exists():
        try:
            vdf_content = library_folders_path.read_text(encoding="utf-8", errors="ignore")
            library_paths = parse_vdf_library_paths(vdf_content)

            for lib_root_path_str in library_paths:
                # Convert CrossOver Windows-style paths to macOS Unix paths
                converted_path_str = convert_crossover_path(lib_root_path_str) or lib_root_path_str
                lib_root_path = Path(converted_path_str)

                game_path = lib_root_path / STEAM_APPS_FOLDER / COMMON_FOLDER / GAME_FOLDER_NAME
                paths.append(game_path)
        except (IOError, OSError):
            pass
    else:
        # No libraryfolders.vdf, add default path
        game_path = steam_path / STEAM_APPS_FOLDER / COMMON_FOLDER / GAME_FOLDER_NAME
        paths.append(game_path)

    return paths


def get_macos_paths() -> List[Tuple[Path, Optional[str]]]:
    """Get candidate game paths on macOS (CrossOver bottles).

    Returns:
        List of (game_path, bottle_name) tuples.
    """
    candidates = []

    bottles_path = Path.home() / "Library" / "Application Support" / "CrossOver" / "Bottles"

    if not bottles_path.exists():
        print(f"Bottles directory not found: {bottles_path}")
        return candidates

    print(f"Searching CrossOver bottles in: {bottles_path}")

    # Search all bottles for the game directory
    for bottle_dir in sorted(bottles_path.iterdir()):
        if not bottle_dir.is_dir():
            continue

        bottle_name = bottle_dir.name
        drive_c = bottle_dir / "drive_c"
        if not drive_c.exists():
            continue

        print(f"Checking bottle: {bottle_name}")

        # Check for Steam installation in bottle
        steam_path = drive_c / "Program Files (x86)" / "Steam"
        if steam_path.exists():
            print(f"  Steam installation found in bottle: {steam_path}")
            library_paths = get_steam_library_paths(steam_path)
            for game_path in library_paths:
                candidates.append((game_path, bottle_name))

        # Fallback to recursive search for game directory
        game_path = find_game_path_recursive(drive_c)
        if game_path:
            candidates.append((game_path, bottle_name))

    return candidates


def find_game_path_recursive(root: Path, max_depth: int = 3) -> Optional[Path]:
    """Recursively search for the game directory in a given root path.

    Args:
        root: Root path to search from.
        max_depth: Maximum recursion depth.

    Returns:
        Path to game if found, None otherwise.
    """
    if max_depth < 0:
        return None

    # Check if current directory is the game directory
    if is_valid_game_path(root):
        return root

    # Recursively search subdirectories
    try:
        for subdir in sorted(root.iterdir()):
            if subdir.is_dir():
                result = find_game_path_recursive(subdir, max_depth - 1)
                if result:
                    return result
    except (PermissionError, OSError):
        # Skip directories we can't access
        pass

    return None


def find_game_path() -> Optional[Tuple[Path, Optional[str]]]:
    """Attempt to locate the game installation directory.

    Returns:
        Tuple of (game_path, bottle_name), or None if not found.
    """
    print("Starting game path search...")

    candidates = []

    if platform.system() == "Darwin":  # macOS
        candidates = get_macos_paths()
    elif platform.system() == "Windows":
        # TODO: Implement Windows path detection
        print("Windows path detection not yet implemented")
    elif platform.system() == "Linux":
        # TODO: Implement Linux path detection
        print("Linux path detection not yet implemented")
    else:
        print(f"Unsupported platform: {platform.system()}")

    print(f"Found {len(candidates)} candidate paths to check")

    for game_path, bottle_name in candidates:
        print(f"Checking: {game_path}")
        if is_valid_game_path(game_path):
            print(f"Found valid game path: {game_path}")
            return game_path, bottle_name

    print("No valid game path found")
    return None


def create_config(game_path: Path, bottle_name: Optional[str] = None, port: int = 8484) -> None:
    """Create the config.json file with game path and settings.

    Args:
        game_path: Path to game installation.
        bottle_name: CrossOver bottle name (if on macOS).
        port: TCP port for the plugin.
    """
    config = {
        "gamePath": str(game_path),
        "crossoverBottle": bottle_name,
        "tcpPort": port,
        "launchTimeoutSeconds": 120,
        "shutdownTimeoutSeconds": 10,
    }

    CONFIG_PATH.write_text(json.dumps(config, indent=2), encoding="utf-8")
    print(f"Configuration written to: {CONFIG_PATH}")


def load_config() -> dict:
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
            f"Run 'python {Path(__file__).name}' to auto-detect game path, "
            f"or create the file manually with --path."
        )

    with open(CONFIG_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


def main():
    parser = argparse.ArgumentParser(
        description="Auto-detect Lobotomy Corporation game installation path"
    )
    parser.add_argument(
        "--path",
        type=str,
        help="Manually specify the game path (overrides auto-detection)",
    )
    parser.add_argument(
        "--bottle",
        type=str,
        help="Manually specify the CrossOver bottle name (for macOS)",
    )
    parser.add_argument(
        "--port",
        type=int,
        default=8484,
        help="TCP port for the plugin (default: 8484)",
    )
    parser.add_argument(
        "--check",
        action="store_true",
        help="Check if current config is valid (don't create new config)",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Show detailed output during search",
    )

    args = parser.parse_args()

    if args.check:
        try:
            config = load_config()
            game_path = Path(config["gamePath"])
            bottle_name = config.get("crossoverBottle")

            print("Current configuration:")
            print(f"  Game Path: {game_path}")
            print(f"  CrossOver Bottle: {bottle_name or 'N/A'}")
            print(f"  TCP Port: {config.get('tcpPort', 8484)}")

            if not game_path.exists():
                print("\nERROR: Game path does not exist")
                sys.exit(1)

            if not is_valid_game_path(game_path):
                print("\nERROR: Game path is not a valid Lobotomy Corporation installation")
                sys.exit(1)

            if not is_bepinex_installed(game_path):
                print("\nWARNING: BepInEx does not appear to be installed at this path")
                print("  Expected: BepInEx/core/BepInEx.dll and doorstop_config.ini")
            else:
                print("\nBepInEx installation: OK")

            print("\nConfiguration is valid")
            sys.exit(0)

        except FileNotFoundError as e:
            print(f"\nERROR: {e}")
            sys.exit(1)
        except json.JSONDecodeError as e:
            print(f"\nERROR: Invalid JSON in config file: {e}")
            sys.exit(1)

    # Auto-detect or use manual path
    if args.path:
        game_path = Path(args.path)
        bottle_name = args.bottle

        if not game_path.exists():
            print(f"ERROR: Path does not exist: {game_path}")
            sys.exit(1)

        if not is_valid_game_path(game_path):
            print(f"ERROR: Path is not a valid Lobotomy Corporation installation: {game_path}")
            sys.exit(1)

        print(f"Using manual game path: {game_path}")
    else:
        result = find_game_path()
        if not result:
            print("\nERROR: Could not auto-detect game path")
            print("Use --path to manually specify the game installation directory")
            sys.exit(1)

        game_path, bottle_name = result

    # Check BepInEx installation
    print(f"\nValidating BepInEx installation...")
    if is_bepinex_installed(game_path):
        print("BepInEx installation: OK")
    else:
        print("WARNING: BepInEx does not appear to be installed")
        print("  Expected: BepInEx/core/BepInEx.dll and doorstop_config.ini")
        print("  The plugin will not work without BepInEx")

    # Create config
    print(f"\nCreating configuration...")
    create_config(game_path, bottle_name, args.port)

    # Summary
    print("\n" + "=" * 60)
    print("Configuration Summary:")
    print("=" * 60)
    print(f"Game Path: {game_path}")
    print(f"CrossOver Bottle: {bottle_name or 'N/A'}")
    print(f"TCP Port: {args.port}")
    print(f"Launch Timeout: {120}s")
    print(f"Shutdown Timeout: {10}s")
    print("=" * 60)
    print("\nYou can now use:")
    print("  python scripts/deploy.py   # Build and deploy the plugin")
    print("  python scripts/launch.py   # Launch the game")
    print("  python scripts/status.py   # Check game status")
    print("  python scripts/stop.py     # Stop the game")


if __name__ == "__main__":
    main()
