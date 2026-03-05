#!/usr/bin/env python3
"""TCP client library for LobotomyPlaywright plugin."""

import json
import socket
from typing import Any, Dict, Optional


class LobotomyPlaywrightClient:
    """TCP client for communicating with LobotomyPlaywright plugin."""

    def __init__(self, host: str = "localhost", port: int = 8484, timeout: float = 5.0):
        """Initialize client connection parameters."""
        self.host = host
        self.port = port
        self.timeout = timeout
        self._socket: Optional[socket.socket] = None
        self._request_id = 0

    def connect(self) -> None:
        """Establish TCP connection to game plugin."""
        self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._socket.settimeout(self.timeout)
        self._socket.connect((self.host, self.port))

    def disconnect(self) -> None:
        """Close TCP connection."""
        if self._socket:
            self._socket.close()
            self._socket = None

    def _generate_request_id(self) -> str:
        """Generate a unique request ID."""
        self._request_id += 1
        return f"req-{self._request_id}"

    def _send(self, request: Dict[str, Any]) -> None:
        """Send a JSON request to the server."""
        if not self._socket:
            raise ConnectionError("Not connected to server")

        message = json.dumps(request) + "\n"
        self._socket.sendall(message.encode("utf-8"))

    def _receive(self) -> Dict[str, Any]:
        """Receive a JSON response from the server."""
        if not self._socket:
            raise ConnectionError("Not connected to server")

        # Read until newline (JSON-line protocol)
        data = b""
        while True:
            chunk = self._socket.recv(4096)
            if not chunk:
                raise ConnectionError("Connection closed by server")
            data += chunk
            if b"\n" in data:
                break

        message = data.decode("utf-8").strip()
        return json.loads(message)

    def query(self, target: str, params: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        """Send a query request and return response."""
        request_id = self._generate_request_id()
        request = {
            "id": request_id,
            "type": "query",
            "target": target,
            "params": params or {},
        }

        self._send(request)
        response = self._receive()

        if response.get("id") != request_id:
            raise ValueError(f"Response ID mismatch: expected {request_id}, got {response.get('id')}")

        if response.get("status") != "ok":
            raise RuntimeError(f"Query failed: {response.get('error')} (code: {response.get('code')})")

        return response.get("data")

    def __enter__(self):
        """Context manager entry."""
        self.connect()
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager exit."""
        self.disconnect()
