#!/usr/bin/env python3
"""Tests for LobotomyPlaywright TCP client library."""

import json
import socket
import pytest
from unittest.mock import Mock, MagicMock, patch
from scripts.client import LobotomyPlaywrightClient


class TestLobotomyPlaywrightClient:
    """Test suite for LobotomyPlaywrightClient class."""

    def test_init_with_defaults(self):
        """Test client initialization with default parameters."""
        client = LobotomyPlaywrightClient()
        assert client.host == "localhost"
        assert client.port == 8484
        assert client.timeout == 5.0
        assert client._socket is None
        assert client._request_id == 0

    def test_init_with_custom_params(self):
        """Test client initialization with custom parameters."""
        client = LobotomyPlaywrightClient(host="127.0.0.1", port=9000, timeout=10.0)
        assert client.host == "127.0.0.1"
        assert client.port == 9000
        assert client.timeout == 10.0

    def test_connect_creates_socket_and_connects(self):
        """Test that connect creates a socket and connects to the server."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient(port=8484)
            client.connect()

            mock_socket_class.assert_called_once_with(socket.AF_INET, socket.SOCK_STREAM)
            mock_socket.settimeout.assert_called_once_with(5.0)
            mock_socket.connect.assert_called_once_with(("localhost", 8484))

    def test_disconnect_closes_socket(self):
        """Test that disconnect closes the socket."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()
            client.disconnect()

            mock_socket.close.assert_called_once()
            assert client._socket is None

    def test_disconnect_when_not_connected_does_nothing(self):
        """Test that disconnect when not connected does not raise an error."""
        client = LobotomyPlaywrightClient()
        client.disconnect()  # Should not raise

    def test_generate_request_id_increments(self):
        """Test that request ID generation increments properly."""
        client = LobotomyPlaywrightClient()
        assert client._generate_request_id() == "req-1"
        assert client._generate_request_id() == "req-2"
        assert client._generate_request_id() == "req-3"

    def test_send_without_connection_raises_error(self):
        """Test that send without connection raises ConnectionError."""
        client = LobotomyPlaywrightClient()
        request = {"id": "req-1", "type": "query", "target": "agents"}

        with pytest.raises(ConnectionError, match="Not connected to server"):
            client._send(request)

    def test_send_with_valid_connection(self):
        """Test that send sends JSON data to the server."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()

            request = {"id": "req-1", "type": "query", "target": "agents"}
            client._send(request)

            expected_message = json.dumps(request) + "\n"
            mock_socket.sendall.assert_called_once_with(expected_message.encode("utf-8"))

    def test_receive_without_connection_raises_error(self):
        """Test that receive without connection raises ConnectionError."""
        client = LobotomyPlaywrightClient()

        with pytest.raises(ConnectionError, match="Not connected to server"):
            client._receive()

    def test_receive_reads_until_newline(self):
        """Test that receive reads until newline delimiter."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket.recv.side_effect = [b'{"id":"req-1","status":"ok"}\n']
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()

            response = client._receive()

            assert response == {"id": "req-1", "status": "ok"}
            assert mock_socket.recv.call_count == 1

    def test_receive_handles_multi_chunk_response(self):
        """Test that receive handles response across multiple chunks."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            # Split response across multiple chunks
            mock_socket.recv.side_effect = [
                b'{"id":"req-1","status',
                b'":"ok"}\n'
            ]
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()

            response = client._receive()

            assert response == {"id": "req-1", "status": "ok"}
            assert mock_socket.recv.call_count == 2

    def test_receive_when_server_closes_connection_raises_error(self):
        """Test that receive raises ConnectionError when server closes."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket.recv.return_value = b""  # Empty bytes means closed
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()

            with pytest.raises(ConnectionError, match="Connection closed by server"):
                client._receive()

    def test_query_sends_request_and_returns_data(self):
        """Test that query sends request and returns response data."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket.recv.return_value = b'{"id":"req-1","type":"response","status":"ok","data":[]}\n'
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()

            data = client.query("agents")

            assert data == []
            assert client._request_id == 1

    def test_query_with_params_sends_params(self):
        """Test that query with parameters sends them in the request."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket.recv.return_value = b'{"id":"req-1","type":"response","status":"ok","data":{}}\n'
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()

            data = client.query("agents", {"id": 123})

            # Check that the request was sent with params
            sent_data = mock_socket.sendall.call_args[0][0].decode("utf-8")
            sent_json = json.loads(sent_data)
            assert sent_json["params"]["id"] == 123

    def test_query_with_error_response_raises_runtime_error(self):
        """Test that query with error response raises RuntimeError."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket.recv.return_value = b'{"id":"req-1","type":"response","status":"error","error":"Not found","code":"NOT_FOUND"}\n'
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()

            with pytest.raises(RuntimeError) as exc_info:
                client.query("agents")

            assert "Query failed: Not found" in str(exc_info.value)
            assert "(code: NOT_FOUND)" in str(exc_info.value)

    def test_query_with_mismatched_response_id_raises_error(self):
        """Test that query with mismatched response ID raises ValueError."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket.recv.return_value = b'{"id":"req-999","type":"response","status":"ok","data":[]}\n'
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()

            with pytest.raises(ValueError, match="Response ID mismatch"):
                client.query("agents")

    def test_context_manager_connects_and_disconnects(self):
        """Test that context manager connects on enter and disconnects on exit."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket_class.return_value = mock_socket

            with LobotomyPlaywrightClient() as client:
                assert client._socket is not None
                mock_socket.connect.assert_called_once()

            mock_socket.close.assert_called_once()
            assert client._socket is None

    def test_context_manager_handles_exceptions(self):
        """Test that context manager disconnects even if exception occurs."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket_class.return_value = mock_socket

            with pytest.raises(RuntimeError):
                with LobotomyPlaywrightClient() as client:
                    raise RuntimeError("Test error")

            mock_socket.close.assert_called_once()

    def test_connection_timeout_handling(self):
        """Test that connection timeout is properly set."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient(timeout=2.5)
            client.connect()

            mock_socket.settimeout.assert_called_once_with(2.5)

    def test_send_large_request(self):
        """Test that send handles large requests."""
        with patch("socket.socket") as mock_socket_class:
            mock_socket = Mock()
            mock_socket_class.return_value = mock_socket

            client = LobotomyPlaywrightClient()
            client.connect()

            # Create a large request
            large_params = {"data": "x" * 10000}
            request = {"id": "req-1", "type": "query", "target": "agents", "params": large_params}
            client._send(request)

            assert mock_socket.sendall.called
            sent_data = mock_socket.sendall.call_args[0][0]
            assert len(sent_data) > 10000


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
