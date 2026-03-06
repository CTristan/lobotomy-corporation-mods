#!/usr/bin/env python3
"""Launch Lobotomy Corporation game and wait for TCP server readiness."""

import argparse
import json
import os
import platform
import socket
import subprocess
import sys
import time
from pathlib import Path
from typing import Optional, Tuple

# Configuration file path
CONFIG_PATH = Path(__file__).parent.parent / "config.json"


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
            f"Run 'python {Path(__file__).parent / 'find_game.py'}' to auto-detect game path."
        )

    with open(CONFIG_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


def find_game_process() -> Optional[int]:
    """Find the Lobotomy Corporation game process.

    Returns:
        Process ID if found, None otherwise.
    """
    try:
        if platform.system() == "Darwin":  # macOS
            # Look for Wine/CrossOver processes running LobotomyCorp.exe
            result = subprocess.run(
                [
                    "pgrep",
                    "-f",
                    "LobotomyCorp.exe",
                ],
                capture_output=True,
                text=True,
            )
            if result.returncode == 0 and result.stdout.strip():
                pids = result.stdout.strip().split("\n")
                return int(pids[0])  # Return first match
        elif platform.system() == "Windows":
            # TODO: Implement Windows process detection
            pass
        elif platform.system() == "Linux":
            # TODO: Implement Linux process detection
            pass
    except (subprocess.SubprocessError, ValueError):
        pass

    return None


def is_tcp_port_open(host: str, port: int, timeout: float = 1.0) -> bool:
    """Check if a TCP port is open and accepting connections.

    Args:
        host: Host to connect to.
        port: Port to check.
        timeout: Connection timeout in seconds.

    Returns:
        True if port is open, False otherwise.
    """
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(timeout)
        result = sock.connect_ex((host, port))
        sock.close()
        return result == 0
    except (socket.error, OSError):
        return False


def check_plugin_responsive(host: str, port: int, timeout: float = 1.0) -> bool:
    """Check if the plugin is responding to queries.

    Args:
        host: Host to connect to.
        port: Port to check.
        timeout: Connection timeout in seconds.

    Returns:
        True if plugin responds, False otherwise.
    """
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(timeout)
        sock.connect((host, port))

        # Send a simple query
        query = {"id": "ping", "type": "query", "target": "game", "params": {}}
        message = json.dumps(query) + "\n"
        sock.sendall(message.encode("utf-8"))

        # Wait for response
        sock.settimeout(timeout)
        response_data = sock.recv(4096)
        sock.close()

        if response_data:
            response_str = response_data.decode("utf-8").strip()
            try:
                response = json.loads(response_str)
                return response.get("status") == "ok"
            except json.JSONDecodeError:
                return False

        return False
    except (socket.error, OSError, json.JSONDecodeError):
        return False


def get_cxstart_path() -> Optional[Path]:
    """Find the CrossOver cxstart executable.

    Returns:
        Path to cxstart if found, None otherwise.
    """
    # Default CrossOver installation path
    cxstart_path = Path(
        "/Applications/CrossOver.app/Contents/SharedSupport/CrossOver/bin/cxstart"
    )

    if cxstart_path.exists():
        return cxstart_path

    # Search for CrossOver in Applications
    apps_path = Path("/Applications")
    for item in apps_path.iterdir():
        if item.name.startswith("CrossOver") and item.is_dir():
            potential_path = (
                item
                / "Contents"
                / "SharedSupport"
                / "CrossOver"
                / "bin"
                / "cxstart"
            )
            if potential_path.exists():
                return potential_path

    return None


def launch_game(game_path: Path, bottle_name: Optional[str] = None) -> subprocess.Popen:
    """Launch the game via CrossOver.

    Args:
        game_path: Path to the game installation.
        bottle_name: CrossOver bottle name (for macOS).

    Returns:
        Subprocess object for the launched game.

    Raises:
        FileNotFoundError: If cxstart is not found.
        subprocess.SubprocessError: If launch fails.
    """
    game_exe = game_path / "LobotomyCorp.exe"

    if not game_exe.exists():
        raise FileNotFoundError(f"Game executable not found: {game_exe}")

    if platform.system() == "Darwin":  # macOS
        cxstart_path = get_cxstart_path()
        if not cxstart_path:
            raise FileNotFoundError(
                "CrossOver cxstart not found.\n"
                "Install CrossOver from https://www.codeweavers.com/crossover\n"
                "or set CROSSOVER_APP environment variable to your CrossOver installation."
            )

        cmd = [
            str(cxstart_path),
            "--bottle",
            bottle_name or "LobotomyCorp",
            "--workdir",
            str(game_path),
            "--dll",
            "winhttp=n,b",
            str(game_exe),
        ]

        print(f"Launching via CrossOver...")
        print(f"  Bottle: {bottle_name or 'LobotomyCorp'}")
        print(f"  Game: {game_exe}")

        # Launch in background
        process = subprocess.Popen(
            cmd,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            start_new_session=True,
        )

        return process

    elif platform.system() == "Windows":
        # TODO: Implement Windows launch
        raise NotImplementedError("Windows launch not yet implemented")

    elif platform.system() == "Linux":
        # TODO: Implement Linux launch (via Proton)
        raise NotImplementedError("Linux launch not yet implemented")

    else:
        raise NotImplementedError(f"Unsupported platform: {platform.system()}")


def wait_for_readiness(
    host: str,
    port: int,
    timeout_seconds: int,
    poll_interval: float = 2.0,
) -> Tuple[bool, Optional[dict]]:
    """Wait for the game to be ready (TCP server responding).

    Args:
        host: Host to connect to.
        port: Port to check.
        timeout_seconds: Maximum time to wait.
        poll_interval: Seconds between polls.

    Returns:
        Tuple of (success, game_state) where game_state is the current game state
        if successful, None otherwise.
    """
    start_time = time.time()
    elapsed = 0

    print(f"\nWaiting for game to be ready (timeout: {timeout_seconds}s)...")
    print("")

    while elapsed < timeout_seconds:
        if is_tcp_port_open(host, port):
            # Port is open, check if plugin is responsive
            if check_plugin_responsive(host, port, timeout=1.0):
                print(f"Game is ready! ({elapsed:.1f}s elapsed)")

                # Get game state
                try:
                    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                    sock.settimeout(2.0)
                    sock.connect((host, port))

                    query = {"id": "status", "type": "query", "target": "game", "params": {}}
                    message = json.dumps(query) + "\n"
                    sock.sendall(message.encode("utf-8"))

                    response_data = sock.recv(8192)
                    sock.close()

                    if response_data:
                        response_str = response_data.decode("utf-8").strip()
                        response = json.loads(response_str)
                        game_state = response.get("data")
                        return True, game_state
                except (socket.error, OSError, json.JSONDecodeError):
                    pass

                return True, None
            else:
                print(f"[{elapsed:.0f}s] Port open but plugin not responding yet...")

        # Check if process died
        if not find_game_process():
            print(f"\nERROR: Game process terminated unexpectedly")
            return False, None

        print(f"[{elapsed:.0f}s] Waiting for TCP server...", end="\r")
        sys.stdout.flush()

        time.sleep(poll_interval)
        elapsed = time.time() - start_time

    print()  # New line after progress

    return False, None


def main():
    parser = argparse.ArgumentParser(
        description="Launch Lobotomy Corporation and wait for TCP readiness"
    )
    parser.add_argument(
        "--host",
        default="localhost",
        help="TCP host (default: localhost)",
    )
    parser.add_argument(
        "--port",
        type=int,
        help="TCP port (overrides config file)",
    )
    parser.add_argument(
        "--timeout",
        type=int,
        help="Launch timeout in seconds (overrides config file)",
    )
    parser.add_argument(
        "--no-wait",
        action="store_true",
        help="Launch game without waiting for readiness",
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="Launch even if game is already running",
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
    bottle_name = config.get("crossoverBottle")
    port = args.port or config.get("tcpPort", 8484)
    timeout = args.timeout or config.get("launchTimeoutSeconds", 60)

    # Verify game path exists
    if not game_path.exists():
        print(f"ERROR: Game path does not exist: {game_path}")
        print("The volume may not be mounted. Run 'python find_game.py' to reconfigure.")
        sys.exit(1)

    # Check if game is already running
    existing_pid = find_game_process()
    if existing_pid and not args.force:
        print(f"Game is already running (PID: {existing_pid})")

        if args.no_wait:
            sys.exit(0)

        # Check if it's ready
        if is_tcp_port_open(args.host, port):
            print("TCP server is already responsive")
            success, game_state = wait_for_readiness(
                args.host, port, 5, poll_interval=1.0
            )
            if success and game_state:
                print("\nGame State:")
                print(f"  Day: {game_state.get('day', 'N/A')}")
                print(f"  Phase: {game_state.get('gameState', 'N/A')}")
                print(f"  Energy: {game_state.get('energy', 0):.1f}")
                sys.exit(0)
            elif success:
                print("\nGame is ready")
                sys.exit(0)

        print("Use --force to relaunch (will kill existing process)")
        sys.exit(1)

    # Kill existing process if --force
    if existing_pid and args.force:
        print(f"Killing existing game process (PID: {existing_pid})...")
        try:
            subprocess.run(["kill", str(existing_pid)], check=False)
            time.sleep(1.0)
        except subprocess.SubprocessError:
            pass

    # Launch the game
    print("=" * 60)
    print("Launching Lobotomy Corporation")
    print("=" * 60)

    try:
        launch_game(game_path, bottle_name)
    except FileNotFoundError as e:
        print(f"\nERROR: {e}")
        sys.exit(1)
    except (subprocess.SubprocessError, NotImplementedError) as e:
        print(f"\nERROR: Failed to launch game: {e}")
        sys.exit(1)

    # Wait for readiness
    if not args.no_wait:
        success, game_state = wait_for_readiness(args.host, port, timeout)

        if not success:
            print("\nERROR: Game did not become ready within timeout")
            print("\nPossible issues:")
            print("  - BepInEx may not be installed correctly")
            print("  - Plugin may not be loaded (check BepInEx log)")
            print("  - Game may have crashed (check for error dialogs)")
            print("\nSuggestions:")
            print("  - Run 'python scripts/status.py' to check game status")
            print("  - Check <game_path>/BepInEx/LogOutput.log for errors")
            print("  - Try launching the game manually to see any error messages")
            sys.exit(1)

        # Print game state
        if game_state:
            print("\n" + "=" * 60)
            print("Game State")
            print("=" * 60)
            print(f"Day: {game_state.get('day', 'N/A')}")
            print(f"Phase: {game_state.get('gameState', 'N/A')}")
            print(f"Speed: {game_state.get('gameSpeed', 1)}x")
            print(f"Energy: {game_state.get('energy', 0):.1f}/{game_state.get('energyQuota', 0):.1f}")
            print(f"Emergency: {game_state.get('emergencyLevel', 'NORMAL')}")
            print(f"Management Started: {game_state.get('managementStarted', False)}")
            print(f"Paused: {game_state.get('isPaused', False)}")
            print("=" * 60)

        print("\nGame is ready!")
        print("\nNext steps:")
        print("  python scripts/query.py agents    # Query game state")
        print("  python scripts/status.py         # Check status")

    else:
        print("\nGame launched. Use --wait or run 'python scripts/status.py' to check readiness.")


if __name__ == "__main__":
    main()
