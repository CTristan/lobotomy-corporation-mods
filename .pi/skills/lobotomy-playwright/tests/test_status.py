#!/usr/bin/env python3
"""Tests for status.py script."""

import json
import platform
import pytest
from pathlib import Path
from unittest.mock import Mock, patch
import scripts.status as status


class TestConfigLoading:
    """Test configuration file loading."""

    def test_load_config_success(self, tmp_path):
        """Test loading valid configuration."""
        config_path = tmp_path / "config.json"
        config_data = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
        }
        config_path.write_text(json.dumps(config_data))

        with patch.object(status, 'CONFIG_PATH', config_path):
            config = status.load_config()
            assert config == config_data

    def test_load_config_not_found(self):
        """Test loading missing configuration."""
        with patch.object(status, 'CONFIG_PATH', Path("/nonexistent/config.json")):
            with pytest.raises(FileNotFoundError, match="Configuration file not found"):
                status.load_config()

    def test_load_config_invalid_json(self, tmp_path):
        """Test loading invalid JSON configuration."""
        config_path = tmp_path / "config.json"
        config_path.write_text("not valid json")

        with patch.object(status, 'CONFIG_PATH', config_path):
            with pytest.raises(json.JSONDecodeError):
                status.load_config()


class TestGameProcessDetection:
    """Test game process detection."""

    @patch('scripts.status.platform.system', return_value='Darwin')
    @patch('scripts.status.subprocess.run')
    def test_find_game_process_found(self, mock_run, mock_platform):
        """Test finding running game process."""
        mock_result = Mock()
        mock_result.returncode = 0
        mock_result.stdout = "12345\n67890"
        mock_run.return_value = mock_result

        pid = status.find_game_process()
        assert pid == 12345

    @patch('scripts.status.platform.system', return_value='Darwin')
    @patch('scripts.status.subprocess.run')
    def test_find_game_process_not_found(self, mock_run, mock_platform):
        """Test when game process is not running."""
        mock_result = Mock()
        mock_result.returncode = 1
        mock_result.stdout = ""
        mock_run.return_value = mock_result

        pid = status.find_game_process()
        assert pid is None

    @patch('scripts.status.platform.system', return_value='Windows')
    def test_find_game_process_windows_not_implemented(self, mock_platform):
        """Test Windows platform (not yet implemented)."""
        pid = status.find_game_process()
        assert pid is None


class TestTcpPortCheck:
    """Test TCP port availability checks."""

    @patch('scripts.status.socket.socket')
    def test_is_tcp_port_open(self, mock_socket_class):
        """Test checking if TCP port is open."""
        mock_socket = Mock()
        mock_socket.connect_ex.return_value = 0
        mock_socket_class.return_value = mock_socket

        result = status.is_tcp_port_open("localhost", 8484)
        assert result is True

    @patch('scripts.status.socket.socket')
    def test_is_tcp_port_closed(self, mock_socket_class):
        """Test checking if TCP port is closed."""
        mock_socket = Mock()
        mock_socket.connect_ex.return_value = 111  # Connection refused
        mock_socket_class.return_value = mock_socket

        result = status.is_tcp_port_open("localhost", 8484)
        assert result is False

    @patch('scripts.status.socket.socket')
    def test_is_tcp_port_socket_error(self, mock_socket_class):
        """Test TCP port check with socket error."""
        mock_socket_class.side_effect = OSError("Network error")

        result = status.is_tcp_port_open("localhost", 8484)
        assert result is False


class TestPluginResponsiveness:
    """Test plugin responsiveness checks."""

    @patch('scripts.status.socket.socket')
    def test_check_plugin_responsive_success(self, mock_socket_class):
        """Test successful plugin responsiveness check."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b'{"id": "ping", "type": "response", "status": "ok", "data": {}}\n'
        mock_socket_class.return_value = mock_socket

        result = status.check_plugin_responsive("localhost", 8484)
        assert result is True

    @patch('scripts.status.socket.socket')
    def test_check_plugin_responsive_error_response(self, mock_socket_class):
        """Test plugin returning error response."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b'{"id": "ping", "type": "response", "status": "error"}\n'
        mock_socket_class.return_value = mock_socket

        result = status.check_plugin_responsive("localhost", 8484)
        assert result is False

    @patch('scripts.status.socket.socket')
    def test_check_plugin_responsive_no_data(self, mock_socket_class):
        """Test plugin not returning data."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b""
        mock_socket_class.return_value = mock_socket

        result = status.check_plugin_responsive("localhost", 8484)
        assert result is False

    @patch('scripts.status.socket.socket')
    def test_check_plugin_responsive_socket_error(self, mock_socket_class):
        """Test plugin check with socket error."""
        mock_socket_class.side_effect = OSError("Connection failed")

        result = status.check_plugin_responsive("localhost", 8484)
        assert result is False


class TestGetGameState:
    """Test getting game state."""

    @patch('scripts.status.socket.socket')
    def test_get_game_state_success(self, mock_socket_class):
        """Test successful game state retrieval."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b'{"id": "status", "type": "response", "status": "ok", "data": {"day": 1, "gameState": "PLAYING"}}\n'
        mock_socket_class.return_value = mock_socket

        game_state = status.get_game_state("localhost", 8484)

        assert game_state == {"day": 1, "gameState": "PLAYING"}

    @patch('scripts.status.socket.socket')
    def test_get_game_state_socket_error(self, mock_socket_class):
        """Test game state retrieval with socket error."""
        mock_socket_class.side_effect = OSError("Connection failed")

        game_state = status.get_game_state("localhost", 8484)

        assert game_state is None

    @patch('scripts.status.socket.socket')
    def test_get_game_state_invalid_json(self, mock_socket_class):
        """Test game state retrieval with invalid JSON."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b'invalid json'
        mock_socket_class.return_value = mock_socket

        game_state = status.get_game_state("localhost", 8484)

        assert game_state is None


class TestGetGameStatus:
    """Test comprehensive game status retrieval."""

    @patch('scripts.status.get_game_state')
    @patch('scripts.status.check_plugin_responsive')
    @patch('scripts.status.is_tcp_port_open')
    @patch('scripts.status.find_game_process')
    def test_status_stopped(self, mock_find, mock_port, mock_responsive, mock_state):
        """Test STOPPED status."""
        mock_find.return_value = None

        game_status, pid, game_state = status.get_game_status("localhost", 8484)

        assert game_status == status.GameStatus.STOPPED
        assert pid is None
        assert game_state is None

    @patch('scripts.status.get_game_state')
    @patch('scripts.status.check_plugin_responsive')
    @patch('scripts.status.is_tcp_port_open')
    @patch('scripts.status.find_game_process')
    def test_status_starting(self, mock_find, mock_port, mock_responsive, mock_state):
        """Test STARTING status."""
        mock_find.return_value = 12345
        mock_port.return_value = False

        game_status, pid, game_state = status.get_game_status("localhost", 8484)

        assert game_status == status.GameStatus.STARTING
        assert pid == 12345
        assert game_state is None

    @patch('scripts.status.get_game_state')
    @patch('scripts.status.check_plugin_responsive')
    @patch('scripts.status.is_tcp_port_open')
    @patch('scripts.status.find_game_process')
    def test_status_unresponsive(self, mock_find, mock_port, mock_responsive, mock_state):
        """Test UNRESPONSIVE status."""
        mock_find.return_value = 12345
        mock_port.return_value = True
        mock_responsive.return_value = False

        game_status, pid, game_state = status.get_game_status("localhost", 8484)

        assert game_status == status.GameStatus.UNRESPONSIVE
        assert pid == 12345
        assert game_state is None

    @patch('scripts.status.get_game_state')
    @patch('scripts.status.check_plugin_responsive')
    @patch('scripts.status.is_tcp_port_open')
    @patch('scripts.status.find_game_process')
    def test_status_ready(self, mock_find, mock_port, mock_responsive, mock_state):
        """Test READY status."""
        mock_find.return_value = 12345
        mock_port.return_value = True
        mock_responsive.return_value = True
        mock_state.return_value = {"day": 1, "gameState": "PLAYING"}

        game_status, pid, game_state = status.get_game_status("localhost", 8484)

        assert game_status == status.GameStatus.READY
        assert pid == 12345
        assert game_state == {"day": 1, "gameState": "PLAYING"}


class TestFormatStatus:
    """Test status output formatting."""

    def test_format_status_stopped(self):
        """Test formatting STOPPED status."""
        output = status.format_status(
            status.GameStatus.STOPPED, None, "localhost", 8484
        )

        assert "STOPPED" in output
        assert "Not running" in output
        assert "127.0.0.1:8484" not in output

    def test_format_status_starting(self):
        """Test formatting STARTING status."""
        output = status.format_status(
            status.GameStatus.STARTING, 12345, "localhost", 8484
        )

        assert "STARTING" in output
        assert "Process ID: 12345" in output
        assert "Waiting for plugin to load" in output

    def test_format_status_ready(self):
        """Test formatting READY status."""
        game_state = {
            "day": 5,
            "gameState": "PLAYING",
            "gameSpeed": 2,
            "energy": 123.45,
            "energyQuota": 200.0,
            "managementStarted": True,
            "isPaused": False,
        }
        output = status.format_status(
            status.GameStatus.READY, 12345, "localhost", 8484, game_state
        )

        assert "✅" in output
        assert "READY" in output
        assert "Day: 5" in output
        assert "Speed: 2x" in output
        # Energy is formatted to one decimal place
        assert "Energy: 123.5/200.0" in output

    def test_format_status_unresponsive(self):
        """Test formatting UNRESPONSIVE status."""
        output = status.format_status(
            status.GameStatus.UNRESPONSIVE, 12345, "localhost", 8484
        )

        assert "UNRESPONSIVE" in output
        assert "⚠" in output
        assert "plugin not responding" in output


class TestMainFunction:
    """Test main status script functionality."""

    @patch('scripts.status.format_status')
    @patch('scripts.status.get_game_status')
    @patch('scripts.status.load_config')
    @patch('sys.argv', ['status.py'])
    def test_main_success(self, mock_format, mock_get_status, mock_load_config):
        """Test successful status check."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
        }

        mock_get_status.return_value = (
            status.GameStatus.READY,
            12345,
            {"day": 1},
        )
        mock_format.return_value = "Formatted status"

        status.main()

        assert mock_format.called

    @patch('scripts.status.get_game_status')
    @patch('scripts.status.load_config')
    @patch('sys.argv', ['status.py', '--json'])
    def test_main_json_output(self, mock_get_status, mock_load_config, capsys):
        """Test JSON output mode."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
        }

        mock_get_status.return_value = (
            status.GameStatus.READY,
            12345,
            {"day": 1},
        )

        status.main()

        captured = capsys.readouterr()
        output = json.loads(captured.out)

        assert output["status"] == "READY"
        assert output["pid"] == 12345
        assert output["gameState"] == {"day": 1}

    @patch('scripts.status.get_game_status')
    @patch('scripts.status.load_config')
    @patch('sys.argv', ['status.py', '--exit-code'])
    def test_main_exit_code_not_ready(self, mock_get_status, mock_load_config):
        """Test exit code when status is not READY."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
        }

        mock_get_status.return_value = (
            status.GameStatus.STARTING,
            12345,
            None,
        )

        with pytest.raises(SystemExit) as exc_info:
            status.main()

        assert exc_info.value.code == 1

    @patch('scripts.status.get_game_status')
    @patch('scripts.status.load_config')
    @patch('sys.argv', ['status.py', '--exit-code'])
    def test_main_exit_code_ready(self, mock_get_status, mock_load_config):
        """Test exit code when status is READY (should not raise)."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
        }

        mock_get_status.return_value = (
            status.GameStatus.READY,
            12345,
            {"day": 1},
        )

        # When status is READY with --exit-code, should not raise SystemExit
        status.main()  # Should complete without raising

    @patch('time.sleep')
    @patch('time.time')
    @patch('scripts.status.get_game_status')
    @patch('scripts.status.load_config')
    @patch('sys.argv', ['status.py', '--wait-for', 'ready'])
    def test_main_wait_for_ready(self, mock_sleep, mock_time, mock_get_status, mock_load_config):
        """Test waiting for READY status."""
        mock_load_config.return_value = {
            "gamePath": "/path/to/game",
            "tcpPort": 8484,
        }

        # Simulate waiting: STARTING -> READY
        # First call is in the wait-for loop, second call is after
        mock_time.side_effect = [0, 1, 2]
        mock_get_status.side_effect = [
            (status.GameStatus.STARTING, 12345, None),  # In loop
            (status.GameStatus.READY, 12345, {"day": 1}),   # In loop, breaks
            (status.GameStatus.READY, 12345, {"day": 1}),   # After loop (for output)
        ]

        status.main()

        assert mock_sleep.call_count == 1

    @patch('scripts.status.load_config')
    @patch('sys.argv', ['status.py'])
    def test_main_config_not_found(self, mock_load_config):
        """Test main with missing config."""
        mock_load_config.side_effect = FileNotFoundError("Config not found")

        with pytest.raises(SystemExit) as exc_info:
            status.main()

        assert exc_info.value.code == 1


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
