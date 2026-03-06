#!/usr/bin/env python3
"""Tests for stop.py script."""

import json
import os
import platform
import socket
import pytest
from pathlib import Path
from unittest.mock import Mock, patch, MagicMock
import scripts.stop as stop


class TestConfigLoading:
    """Test configuration file loading."""

    def test_load_config_success(self, tmp_path):
        """Test loading valid configuration."""
        config_path = tmp_path / "config.json"
        config_data = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
            "shutdownTimeoutSeconds": 10,
        }
        config_path.write_text(json.dumps(config_data))

        with patch.object(stop, 'CONFIG_PATH', config_path):
            config = stop.load_config()
            assert config == config_data

    def test_load_config_not_found(self):
        """Test loading missing configuration."""
        with patch.object(stop, 'CONFIG_PATH', Path("/nonexistent/config.json")):
            with pytest.raises(FileNotFoundError, match="Configuration file not found"):
                stop.load_config()

    def test_load_config_invalid_json(self, tmp_path):
        """Test loading invalid JSON configuration."""
        config_path = tmp_path / "config.json"
        config_path.write_text("not valid json")

        with patch.object(stop, 'CONFIG_PATH', config_path):
            with pytest.raises(json.JSONDecodeError):
                stop.load_config()


class TestGameProcessDetection:
    """Test game process detection."""

    @patch('scripts.stop.platform.system', return_value='Darwin')
    @patch('scripts.stop.subprocess.run')
    def test_find_game_processes_found(self, mock_run, mock_platform):
        """Test finding running game processes."""
        mock_result = Mock()
        mock_result.returncode = 0
        mock_result.stdout = "12345\n67890"
        mock_run.return_value = mock_result

        pids = stop.find_game_processes()
        assert pids == [12345, 67890]

    @patch('scripts.stop.platform.system', return_value='Darwin')
    @patch('scripts.stop.subprocess.run')
    def test_find_game_processes_not_found(self, mock_run, mock_platform):
        """Test when game processes are not running."""
        mock_result = Mock()
        mock_result.returncode = 1
        mock_result.stdout = ""
        mock_run.return_value = mock_result

        pids = stop.find_game_processes()
        assert pids == []

    @patch('scripts.stop.platform.system', return_value='Windows')
    def test_find_game_processes_windows_not_implemented(self, mock_platform):
        """Test Windows platform (not yet implemented)."""
        pids = stop.find_game_processes()
        assert pids == []


class TestTcpPortCheck:
    """Test TCP port availability checks."""

    @patch('scripts.stop.socket.socket')
    def test_is_tcp_port_open(self, mock_socket_class):
        """Test checking if TCP port is open."""
        mock_socket = Mock()
        mock_socket.connect_ex.return_value = 0
        mock_socket_class.return_value = mock_socket

        result = stop.is_tcp_port_open("localhost", 8484)
        assert result is True

    @patch('scripts.stop.socket.socket')
    def test_is_tcp_port_closed(self, mock_socket_class):
        """Test checking if TCP port is closed."""
        mock_socket = Mock()
        mock_socket.connect_ex.return_value = 111  # Connection refused
        mock_socket_class.return_value = mock_socket

        result = stop.is_tcp_port_open("localhost", 8484)
        assert result is False

    @patch('scripts.stop.socket.socket')
    def test_is_tcp_port_socket_error(self, mock_socket_class):
        """Test TCP port check with socket error."""
        mock_socket_class.side_effect = OSError("Network error")

        result = stop.is_tcp_port_open("localhost", 8484)
        assert result is False


class TestGracefulShutdown:
    """Test graceful TCP shutdown."""

    @patch('scripts.stop.time.sleep')
    @patch('scripts.stop.time.time')
    @patch('scripts.stop.find_game_processes')
    @patch('scripts.stop.socket.socket')
    def test_graceful_shutdown_success(self, mock_socket_class, mock_find, mock_time, mock_sleep):
        """Test successful graceful shutdown."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b'{"id": "cmd", "type": "response", "status": "ok"}\n'
        mock_socket_class.return_value = mock_socket

        mock_time.side_effect = [0, 0.5, 1.0]
        mock_find.side_effect = [[12345], []]  # Process exists, then gone

        result = stop.graceful_shutdown("localhost", 8484, 10)

        assert result is True
        mock_socket.sendall.assert_called_once()

    @patch('scripts.stop.time.sleep')
    @patch('scripts.stop.time.time')
    @patch('scripts.stop.find_game_processes')
    @patch('scripts.stop.socket.socket')
    def test_graceful_shutdown_timeout(self, mock_socket_class, mock_find, mock_time, mock_sleep):
        """Test graceful shutdown timeout."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b'{"id": "cmd", "type": "response", "status": "ok"}\n'
        mock_socket_class.return_value = mock_socket

        mock_time.side_effect = [0, 5, 11]  # Exceed timeout
        mock_find.return_value = [12345]  # Process still running

        result = stop.graceful_shutdown("localhost", 8484, 10)

        assert result is False

    @patch('scripts.stop.socket.socket')
    def test_graceful_shutdown_connection_failed(self, mock_socket_class):
        """Test graceful shutdown when TCP connection fails."""
        mock_socket_class.side_effect = OSError("Connection failed")

        result = stop.graceful_shutdown("localhost", 8484, 10)

        assert result is False

    @patch('scripts.stop.socket.socket')
    def test_graceful_shutdown_no_acknowledgment(self, mock_socket_class):
        """Test graceful shutdown with no acknowledgment."""
        mock_socket = Mock()
        mock_socket.recv.side_effect = socket.timeout()
        mock_socket_class.return_value = mock_socket

        # Should still try to wait for process exit
        with patch('scripts.stop.time.sleep'), \
             patch('scripts.stop.time.time', side_effect=[0, 0.5]), \
             patch('scripts.stop.find_game_processes', return_value=[]):
            result = stop.graceful_shutdown("localhost", 8484, 10)

            # Process is gone, so it's a success
            assert result is True


class TestMainFunction:
    """Test main stop script functionality."""

    @patch('scripts.stop.find_game_processes')
    @patch('scripts.stop.load_config')
    @patch('sys.argv', ['stop.py'])
    def test_main_not_running(self, mock_find, mock_load_config):
        """Test main when game is not running."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
            "shutdownTimeoutSeconds": 10,
        }
        mock_find.return_value = []

        with pytest.raises(SystemExit) as exc_info:
            stop.main()

        assert exc_info.value.code == 0

    @patch('scripts.stop.time.sleep')
    @patch('scripts.stop.find_game_processes')
    @patch('scripts.stop.graceful_shutdown')
    @patch('scripts.stop.load_config')
    @patch('sys.argv', ['stop.py'])
    def test_main_graceful_shutdown_success(self, mock_sleep, mock_find, mock_graceful, mock_load_config):
        """Test main with successful graceful shutdown."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
            "shutdownTimeoutSeconds": 10,
        }
        mock_find.side_effect = [[12345], []]
        mock_graceful.return_value = True

        with pytest.raises(SystemExit) as exc_info:
            stop.main()

        assert exc_info.value.code == 0

    @patch('scripts.stop.find_game_processes')
    @patch('scripts.stop.force_kill')
    @patch('scripts.stop.graceful_shutdown')
    @patch('scripts.stop.is_tcp_port_open')
    @patch('scripts.stop.load_config')
    @patch('sys.argv', ['stop.py'])
    def test_main_force_kill_fallback(self, mock_find, mock_force, mock_graceful, mock_port, mock_load_config):
        """Test main falling back to force kill."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
            "shutdownTimeoutSeconds": 10,
        }
        mock_port.return_value = True
        mock_graceful.return_value = False
        mock_force.return_value = True
        mock_find.side_effect = [[12345], []]

        with pytest.raises(SystemExit) as exc_info:
            stop.main()

        assert exc_info.value.code == 0
        assert mock_graceful.called
        assert mock_force.called

    @patch('scripts.stop.find_game_processes')
    @patch('scripts.stop.force_kill')
    @patch('scripts.stop.graceful_shutdown')
    @patch('scripts.stop.is_tcp_port_open')
    @patch('scripts.stop.load_config')
    @patch('sys.argv', ['stop.py', '--force'])
    def test_main_force_only(self, mock_find, mock_force, mock_graceful, mock_port, mock_load_config):
        """Test main with --force flag."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
            "shutdownTimeoutSeconds": 10,
        }
        mock_force.return_value = True
        mock_find.side_effect = [[12345], []]

        with pytest.raises(SystemExit) as exc_info:
            stop.main()

        assert exc_info.value.code == 0
        assert not mock_graceful.called
        assert mock_force.called

    @patch('scripts.stop.time.sleep')
    @patch('scripts.stop.find_game_processes')
    @patch('scripts.stop.force_kill')
    @patch('scripts.stop.graceful_shutdown')
    @patch('scripts.stop.is_tcp_port_open')
    @patch('scripts.stop.load_config')
    @patch('sys.argv', ['stop.py'])
    def test_main_with_wait(self, mock_sleep, mock_find, mock_force, mock_graceful, mock_port, mock_load_config):
        """Test main with --wait flag."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
            "shutdownTimeoutSeconds": 10,
        }
        mock_port.return_value = True
        mock_graceful.return_value = False
        mock_force.return_value = True
        mock_find.return_value = []  # No processes

        with pytest.raises(SystemExit) as exc_info:
            stop.main()

        assert exc_info.value.code == 0

    @patch('scripts.stop.find_game_processes')
    @patch('scripts.stop.force_kill')
    @patch('scripts.stop.graceful_shutdown')
    @patch('scripts.stop.is_tcp_port_open')
    @patch('scripts.stop.load_config')
    @patch('sys.argv', ['stop.py'])
    def test_main_partial_failure(self, mock_find, mock_force, mock_graceful, mock_port, mock_load_config):
        """Test main with partial shutdown failure."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
            "shutdownTimeoutSeconds": 10,
        }
        mock_port.return_value = False
        mock_graceful.return_value = False
        mock_force.return_value = False
        mock_find.side_effect = [[12345, 67890], [12345]]  # Some remain

        with pytest.raises(SystemExit) as exc_info:
            stop.main()

        assert exc_info.value.code == 1

    @patch('scripts.stop.load_config')
    @patch('sys.argv', ['stop.py'])
    def test_main_config_not_found(self, mock_load_config):
        """Test main with missing config."""
        mock_load_config.side_effect = FileNotFoundError("Config not found")

        with pytest.raises(SystemExit) as exc_info:
            stop.main()

        assert exc_info.value.code == 1


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
