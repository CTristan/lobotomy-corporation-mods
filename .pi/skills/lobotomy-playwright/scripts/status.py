#!/usr/bin/env python3
"""Check the status of Lobotomy Corporation game and TCP server."""

import argparse
import json
import platform
import socket
import subprocess
import sys
from enum import Enum
from pathlib import Path
from typing import Optional, Tuple

# Configuration file path
CONFIG_PATH = Path(__file__).parent.parent / "config.json"


class GameStatus(Enum):
    """Game status enumeration."""

    STOPPED = "STOPPED"
    STARTING = "STARTING"
    READY = "READY"
    UNRESPONSIVE = "UNRESPONSIVE"


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


def get_game_state(host: str, port: int, timeout: float = 2.0) -> Optional[dict]:
    """Get the current game state.

    Args:
        host: Host to connect to.
        port: Port to connect to.
        timeout: Connection timeout in seconds.

    Returns:
        Game state dictionary, or None if unavailable.
    """
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(timeout)
        sock.connect((host, port))

        query = {"id": "status", "type": "query", "target": "game", "params": {}}
        message = json.dumps(query) + "\n"
        sock.sendall(message.encode("utf-8"))

        response_data = sock.recv(8192)
        sock.close()

        if response_data:
            response_str = response_data.decode("utf-8").strip()
            response = json.loads(response_str)
            return response.get("data")

        return None
    except (socket.error, OSError, json.JSONDecodeError):
        return None


def get_game_status(host: str, port: int) -> Tuple[GameStatus, Optional[int], Optional[dict]]:
    """Get the comprehensive game status.

    Args:
        host: Host to connect to.
        port: Port to connect to.

    Returns:
        Tuple of (status, pid, game_state).
    """
    pid = find_game_process()

    if pid is None:
        return GameStatus.STOPPED, None, None

    # Game process is running
    if not is_tcp_port_open(host, port):
        return GameStatus.STARTING, pid, None

    # TCP port is open, check if plugin is responsive
    if not check_plugin_responsive(host, port):
        return GameStatus.UNRESPONSIVE, pid, None

    # Plugin is responsive, get game state
    game_state = get_game_state(host, port)
    return GameStatus.READY, pid, game_state


def format_status(status: GameStatus, pid: Optional[int], host: str, port: int, game_state: Optional[dict] = None) -> str:
    """Format status output for display.

    Args:
        status: Game status.
        pid: Process ID (if running).
        host: TCP host.
        port: TCP port.
        game_state: Game state (if available).

    Returns:
        Formatted status string.
    """
    lines = [
        "=" * 60,
        "Lobotomy Corporation Status",
        "=" * 60,
    ]

    # Status
    status_emoji = {
        GameStatus.STOPPED: "⏹",
        GameStatus.STARTING: "⏳",
        GameStatus.READY: "✅",
        GameStatus.UNRESPONSIVE: "⚠️",
    }
    lines.append(f"Status: {status_emoji.get(status, '?')} {status.value}")

    # Process ID
    if pid:
        lines.append(f"Process ID: {pid}")
    else:
        lines.append("Process: Not running")

    # TCP Server
    if status in (GameStatus.STARTING, GameStatus.READY, GameStatus.UNRESPONSIVE):
        lines.append(f"TCP Server: {host}:{port}")

        if status == GameStatus.STARTING:
            lines.append("  → Waiting for plugin to load...")
        elif status == GameStatus.UNRESPONSIVE:
            lines.append("  ⚠ Port open but plugin not responding")
        elif status == GameStatus.READY:
            lines.append("  → Connected and responsive")

    # Game State (only if ready)
    if status == GameStatus.READY and game_state:
        lines.extend([
            "",
            "Game State:",
            f"  Day: {game_state.get('day', 'N/A')}",
            f"  Phase: {game_state.get('gameState', 'N/A')}",
            f"  Speed: {game_state.get('gameSpeed', 1)}x",
            f"  Energy: {game_state.get('energy', 0):.1f}/{game_state.get('energyQuota', 0):.1f}",
            f"  Emergency: {game_state.get('emergencyLevel', 'NORMAL')}",
            f"  Management Started: {game_state.get('managementStarted', False)}",
            f"  Paused: {game_state.get('isPaused', False)}",
        ])

    lines.append("=" * 60)

    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(
        description="Check the status of Lobotomy Corporation and TCP server"
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
        "--json",
        action="store_true",
        help="Output status as JSON",
    )
    parser.add_argument(
        "--exit-code",
        action="store_true",
        help="Exit with non-zero code if not READY",
    )
    parser.add_argument(
        "--wait-for",
        type=str,
        choices=["stopped", "starting", "ready", "unresponsive"],
        help="Wait for the specified status before exiting",
    )
    parser.add_argument(
        "--timeout",
        type=int,
        default=60,
        help="Max wait time in seconds for --wait-for (default: 60)",
    )
    parser.add_argument(
        "--poll",
        type=float,
        default=1.0,
        help="Poll interval in seconds for --wait-for (default: 1.0)",
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

    port = args.port or config.get("tcpPort", 8484)

    # Wait for specific status
    if args.wait_for:
        import time

        target_status = GameStatus(args.wait_for.upper())
        start_time = time.time()

        while time.time() - start_time < args.timeout:
            status, pid, game_state = get_game_status(args.host, port)

            if status == target_status:
                break

            time.sleep(args.poll)

    # Get current status
    status, pid, game_state = get_game_status(args.host, port)

    # Output
    if args.json:
        output = {
            "status": status.value,
            "pid": pid,
            "host": args.host,
            "port": port,
            "gameState": game_state,
        }
        print(json.dumps(output, indent=2))
    else:
        print(format_status(status, pid, args.host, port, game_state))

    # Exit code
    if args.exit_code and status != GameStatus.READY:
        sys.exit(1)


if __name__ == "__main__":
    main()
