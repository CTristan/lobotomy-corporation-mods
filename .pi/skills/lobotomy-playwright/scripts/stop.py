#!/usr/bin/env python3
"""Stop Lobotomy Corporation game with graceful TCP shutdown or force-kill."""

import argparse
import json
import platform
import signal
import socket
import subprocess
import sys
import time
from pathlib import Path
from typing import Optional

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


def find_game_processes() -> list[int]:
    """Find all Lobotomy Corporation game processes.

    Returns:
        List of process IDs (may be empty).
    """
    try:
        if platform.system() == "Darwin":  # macOS
            # Look for Wine/CrossOver processes running LobotomyCorp.exe
            result = subprocess.run(
                ["pgrep", "-f", "LobotomyCorp.exe"],
                capture_output=True,
                text=True,
            )
            if result.returncode == 0 and result.stdout.strip():
                pids_str = result.stdout.strip().split("\n")
                return [int(pid) for pid in pids_str if pid.strip()]
        elif platform.system() == "Windows":
            # TODO: Implement Windows process detection
            pass
        elif platform.system() == "Linux":
            # TODO: Implement Linux process detection
            pass
    except (subprocess.SubprocessError, ValueError):
        pass

    return []


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


def graceful_shutdown(host: str, port: int, timeout: float) -> bool:
    """Attempt graceful shutdown via TCP command.

    Args:
        host: Host to connect to.
        port: Port to connect to.
        timeout: Maximum time to wait for process to exit.

    Returns:
        True if graceful shutdown succeeded, False otherwise.
    """
    print("Attempting graceful TCP shutdown...")

    try:
        # Connect and send shutdown command
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(2.0)
        sock.connect((host, port))

        command = {"type": "command", "action": "shutdown", "params": {}}
        message = json.dumps(command) + "\n"
        sock.sendall(message.encode("utf-8"))

        # Wait for acknowledgment
        try:
            response_data = sock.recv(4096)
            if response_data:
                response_str = response_data.decode("utf-8").strip()
                response = json.loads(response_str)
                if response.get("status") == "ok":
                    print("  Shutdown command acknowledged")
        except (socket.timeout, socket.error, json.JSONDecodeError):
            pass

        sock.close()

        # Wait for process to exit
        print(f"  Waiting for process to exit (timeout: {timeout}s)...")
        start_time = time.time()

        while time.time() - start_time < timeout:
            pids = find_game_processes()
            if not pids:
                print("  ✓ Game stopped gracefully")
                return True
            time.sleep(0.5)

        print("  ⚠ Timeout exceeded, proceeding to force kill")
        return False

    except (socket.error, OSError) as e:
        print(f"  ⚠ TCP connection failed: {e}")
        print("  Proceeding to force kill")
        return False


def force_kill(pids: list[int]) -> bool:
    """Force kill game processes.

    Args:
        pids: List of process IDs to kill.

    Returns:
        True if all processes were killed, False otherwise.
    """
    import os

    print(f"Force killing processes: {pids}")

    for pid in pids:
        try:
            # Try SIGTERM first
            os.kill(pid, signal.SIGTERM)
        except (ProcessLookupError, PermissionError) as e:
            print(f"  Warning: Failed to send SIGTERM to PID {pid}: {e}")

    # Wait briefly
    time.sleep(1.0)

    # Check if processes are still running
    remaining_pids = [p for p in pids if process_exists(p)]

    if not remaining_pids:
        print("  ✓ All processes stopped with SIGTERM")
        return True

    # Force kill remaining
    for pid in remaining_pids:
        try:
            os.kill(pid, signal.SIGKILL)
        except (ProcessLookupError, PermissionError) as e:
            print(f"  Warning: Failed to send SIGKILL to PID {pid}: {e}")

    # Verify
    time.sleep(0.5)
    final_remaining = [p for p in pids if process_exists(p)]

    if final_remaining:
        print(f"  ✗ Failed to kill processes: {final_remaining}")
        return False

    print("  ✓ All processes force-killed")
    return True


def process_exists(pid: int) -> bool:
    """Check if a process exists.

    Args:
        pid: Process ID to check.

    Returns:
        True if process exists, False otherwise.
    """
    import os

    try:
        os.kill(pid, 0)
        return True
    except (ProcessLookupError, PermissionError):
        return False


def main():
    import os  # Import here to avoid conflicts at module level

    parser = argparse.ArgumentParser(
        description="Stop Lobotomy Corporation game"
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
        type=float,
        help="Graceful shutdown timeout in seconds (overrides config file)",
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="Skip graceful shutdown and force kill immediately",
    )
    parser.add_argument(
        "--wait",
        action="store_true",
        help="Wait for processes to fully exit before returning",
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
    timeout = args.timeout or config.get("shutdownTimeoutSeconds", 10)

    # Find game processes
    pids = find_game_processes()

    if not pids:
        print("Game is not running")
        sys.exit(0)

    print(f"Found {len(pids)} game process(es): {pids}")

    # Stop the game
    print("\n" + "=" * 60)
    print("Stopping Lobotomy Corporation")
    print("=" * 60)

    success = False

    if not args.force:
        # Try graceful shutdown first
        if is_tcp_port_open(args.host, port):
            if graceful_shutdown(args.host, port, timeout):
                success = True
        else:
            print("TCP server not available, proceeding to force kill")

    # Force kill if graceful failed or was skipped
    if not success:
        success = force_kill(pids)

    # Wait for processes to exit if requested
    if args.wait:
        print("\nWaiting for processes to exit...")
        for i in range(10):
            remaining = find_game_processes()
            if not remaining:
                print("  ✓ All processes exited")
                break
            time.sleep(0.5)
        else:
            remaining = find_game_processes()
            if remaining:
                print(f"  ⚠ Processes still running: {remaining}")

    # Final status
    remaining_pids = find_game_processes()

    print("\n" + "=" * 60)
    if success and not remaining_pids:
        print("✓ Game stopped successfully")
        print("=" * 60)
        sys.exit(0)
    else:
        print("✗ Failed to stop game completely")
        if remaining_pids:
            print(f"  Remaining processes: {remaining_pids}")
        print("=" * 60)
        sys.exit(1)


if __name__ == "__main__":
    main()
