#!/usr/bin/env python3
"""Tests for launch.py script."""

import json
import platform
import pytest
from pathlib import Path
from unittest.mock import Mock, patch, MagicMock
import scripts.launch as launch


class TestConfigLoading:
    """Test configuration file loading."""

    def test_load_config_success(self, tmp_path):
        """Test loading valid configuration."""
        config_path = tmp_path / "config.json"
        config_data = {
            "gamePath": str(tmp_path / "game"),
            "crossoverBottle": "TestBottle",
            "tcpPort": 8484,
            "launchTimeoutSeconds": 120,
        }
        config_path.write_text(json.dumps(config_data))

        with patch.object(launch, 'CONFIG_PATH', config_path):
            config = launch.load_config()
            assert config["gamePath"] == config_data["gamePath"]
            assert config["crossoverBottle"] == "TestBottle"
            assert config["tcpPort"] == 8484

    def test_load_config_not_found(self):
        """Test loading missing configuration."""
        with patch.object(launch, 'CONFIG_PATH', Path("/nonexistent/config.json")):
            with pytest.raises(FileNotFoundError, match="Configuration file not found"):
                launch.load_config()

    def test_load_config_invalid_json(self, tmp_path):
        """Test loading invalid JSON configuration."""
        config_path = tmp_path / "config.json"
        config_path.write_text("not valid json")

        with patch.object(launch, 'CONFIG_PATH', config_path):
            with pytest.raises(json.JSONDecodeError):
                launch.load_config()


class TestGameProcessDetection:
    """Test game process detection."""

    @patch('scripts.launch.platform.system', return_value='Darwin')
    @patch('scripts.launch.subprocess.run')
    def test_find_game_process_found(self, mock_run, mock_platform):
        """Test finding running game process."""
        mock_result = Mock()
        mock_result.returncode = 0
        mock_result.stdout = "12345\n67890"
        mock_run.return_value = mock_result

        pid = launch.find_game_process()
        assert pid == 12345

    @patch('scripts.launch.platform.system', return_value='Darwin')
    @patch('scripts.launch.subprocess.run')
    def test_find_game_process_not_found(self, mock_run, mock_platform):
        """Test when game process is not running."""
        mock_result = Mock()
        mock_result.returncode = 1
        mock_result.stdout = ""
        mock_run.return_value = mock_result

        pid = launch.find_game_process()
        assert pid is None

    @patch('scripts.launch.platform.system', return_value='Windows')
    def test_find_game_process_windows_not_implemented(self, mock_platform):
        """Test Windows platform (not yet implemented)."""
        pid = launch.find_game_process()
        assert pid is None


class TestTcpPortCheck:
    """Test TCP port availability checks."""

    @patch('scripts.launch.socket.socket')
    def test_is_tcp_port_open(self, mock_socket_class):
        """Test checking if TCP port is open."""
        mock_socket = Mock()
        mock_socket.connect_ex.return_value = 0
        mock_socket_class.return_value = mock_socket

        result = launch.is_tcp_port_open("localhost", 8484)
        assert result is True
        mock_socket.close.assert_called_once()

    @patch('scripts.launch.socket.socket')
    def test_is_tcp_port_closed(self, mock_socket_class):
        """Test checking if TCP port is closed."""
        mock_socket = Mock()
        mock_socket.connect_ex.return_value = 111  # Connection refused
        mock_socket_class.return_value = mock_socket

        result = launch.is_tcp_port_open("localhost", 8484)
        assert result is False

    @patch('scripts.launch.socket.socket')
    def test_is_tcp_port_socket_error(self, mock_socket_class):
        """Test TCP port check with socket error."""
        mock_socket_class.side_effect = OSError("Network error")

        result = launch.is_tcp_port_open("localhost", 8484)
        assert result is False


class TestPluginResponsiveness:
    """Test plugin responsiveness checks."""

    @patch('scripts.launch.socket.socket')
    def test_check_plugin_responsive_success(self, mock_socket_class):
        """Test successful plugin responsiveness check."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b'{"id": "ping", "type": "response", "status": "ok", "data": {}}\n'
        mock_socket_class.return_value = mock_socket

        result = launch.check_plugin_responsive("localhost", 8484)
        assert result is True

    @patch('scripts.launch.socket.socket')
    def test_check_plugin_responsive_error_response(self, mock_socket_class):
        """Test plugin returning error response."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b'{"id": "ping", "type": "response", "status": "error"}\n'
        mock_socket_class.return_value = mock_socket

        result = launch.check_plugin_responsive("localhost", 8484)
        assert result is False

    @patch('scripts.launch.socket.socket')
    def test_check_plugin_responsive_no_data(self, mock_socket_class):
        """Test plugin not returning data."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b""
        mock_socket_class.return_value = mock_socket

        result = launch.check_plugin_responsive("localhost", 8484)
        assert result is False

    @patch('scripts.launch.socket.socket')
    def test_check_plugin_responsive_socket_error(self, mock_socket_class):
        """Test plugin check with socket error."""
        mock_socket_class.side_effect = OSError("Connection failed")

        result = launch.check_plugin_responsive("localhost", 8484)
        assert result is False

    @patch('scripts.launch.socket.socket')
    def test_check_plugin_responsive_invalid_json(self, mock_socket_class):
        """Test plugin check with invalid JSON response."""
        mock_socket = Mock()
        mock_socket.recv.return_value = b'{"invalid json'
        mock_socket_class.return_value = mock_socket

        result = launch.check_plugin_responsive("localhost", 8484)
        assert result is False


class TestGetCxstartPath:
    """Test CrossOver cxstart executable detection."""

    def test_get_cxstart_path_default(self):
        """Test finding cxstart at default location."""
        with patch('scripts.launch.Path.exists') as mock_exists:
            mock_exists.return_value = True

            path = launch.get_cxstart_path()
            assert path is not None
            assert "cxstart" in str(path)

    def test_get_cxstart_path_not_found(self):
        """Test when cxstart is not found."""
        with patch('scripts.launch.Path.exists', return_value=False):
            path = launch.get_cxstart_path()
            assert path is None


class TestLaunchGame:
    """Test game launching functionality."""

    @patch('scripts.launch.platform.system', return_value='Darwin')
    @patch('scripts.launch.get_cxstart_path')
    @patch('scripts.launch.subprocess.Popen')
    def test_launch_game_success(self, mock_popen, mock_cxstart, mock_platform, tmp_path):
        """Test successful game launch."""
        game_path = tmp_path / "LobotomyCorp"
        game_path.mkdir()
        (game_path / "LobotomyCorp.exe").touch()

        mock_cxstart.return_value = Path("/Applications/CrossOver.app/.../cxstart")
        mock_process = Mock()
        mock_popen.return_value = mock_process

        process = launch.launch_game(game_path, "TestBottle")

        assert process == mock_process
        assert mock_popen.called

    @patch('scripts.launch.platform.system', return_value='Darwin')
    @patch('scripts.launch.get_cxstart_path')
    def test_launch_game_exe_not_found(self, mock_cxstart, mock_platform, tmp_path):
        """Test launch with missing executable."""
        game_path = tmp_path / "LobotomyCorp"
        game_path.mkdir()
        # Don't create LobotomyCorp.exe

        with pytest.raises(FileNotFoundError, match="Game executable not found"):
            launch.launch_game(game_path, "TestBottle")

    @patch('scripts.launch.platform.system', return_value='Darwin')
    @patch('scripts.launch.get_cxstart_path', return_value=None)
    def test_launch_game_crossover_not_found(self, mock_cxstart, mock_platform, tmp_path):
        """Test launch with CrossOver not installed."""
        game_path = tmp_path / "LobotomyCorp"
        game_path.mkdir()
        (game_path / "LobotomyCorp.exe").touch()

        with pytest.raises(FileNotFoundError, match="CrossOver cxstart not found"):
            launch.launch_game(game_path, "TestBottle")

    @patch('scripts.launch.platform.system', return_value='Windows')
    def test_launch_game_windows_not_implemented(self, mock_platform, tmp_path):
        """Test Windows launch (not yet implemented)."""
        game_path = tmp_path / "LobotomyCorp"
        game_path.mkdir()
        (game_path / "LobotomyCorp.exe").touch()

        with pytest.raises(NotImplementedError, match="Windows launch not yet implemented"):
            launch.launch_game(game_path)


class TestWaitForReadiness:
    """Test waiting for game readiness."""

    @patch('scripts.launch.time.time')
    @patch('scripts.launch.time.sleep')
    @patch('scripts.launch.find_game_process')
    @patch('scripts.launch.check_plugin_responsive')
    @patch('scripts.launch.is_tcp_port_open')
    def test_wait_for_readiness_success(
        self, mock_port_open, mock_responsive, mock_find, mock_sleep, mock_time
    ):
        """Test successful wait for readiness."""
        # First poll: not responsive yet, second poll: responsive
        mock_time.side_effect = [0, 2, 4]
        mock_find.return_value = 12345  # Game process is running
        mock_port_open.side_effect = [True, True]
        mock_responsive.side_effect = [False, True]

        success, game_state = launch.wait_for_readiness("localhost", 8484, 60, poll_interval=2.0)

        assert success is True
        assert mock_sleep.call_count >= 1  # Should have slept between polls

    @patch('scripts.launch.time.time')
    @patch('scripts.launch.time.sleep')
    @patch('scripts.launch.find_game_process')
    @patch('scripts.launch.check_plugin_responsive')
    @patch('scripts.launch.is_tcp_port_open')
    def test_wait_for_readiness_timeout(
        self, mock_port_open, mock_responsive, mock_process, mock_sleep, mock_time
    ):
        """Test wait timeout."""
        mock_time.side_effect = [0, 60, 61]  # Start, timeout reached
        mock_port_open.return_value = True
        mock_responsive.return_value = False
        mock_process.return_value = 12345

        success, game_state = launch.wait_for_readiness("localhost", 8484, 60)

        assert success is False
        assert game_state is None

    @patch('scripts.launch.time.time')
    @patch('scripts.launch.time.sleep')
    @patch('scripts.launch.find_game_process')
    @patch('scripts.launch.is_tcp_port_open')
    def test_wait_for_readiness_process_died(
        self, mock_port_open, mock_process, mock_sleep, mock_time
    ):
        """Test when game process dies during wait."""
        mock_time.side_effect = [0, 5, 10]
        mock_port_open.return_value = False
        mock_process.side_effect = [12345, None]  # Process found, then died

        success, game_state = launch.wait_for_readiness("localhost", 8484, 60)

        assert success is False
        assert game_state is None


class TestMainFunction:
    """Test main launch script functionality."""

    @patch('scripts.launch.wait_for_readiness')
    @patch('scripts.launch.launch_game')
    @patch('scripts.launch.find_game_process')
    @patch('scripts.launch.load_config')
    @patch('sys.argv', ['launch.py'])
    def test_main_success(self, mock_wait, mock_launch, mock_find, mock_load_config, tmp_path):
        """Test successful launch workflow."""
        game_path = tmp_path / "game"
        game_path.mkdir()
        (game_path / "LobotomyCorp.exe").touch()

        mock_load_config.return_value = {
            "gamePath": str(game_path),
            "crossoverBottle": "TestBottle",
            "tcpPort": 8484,
            "launchTimeoutSeconds": 120,
        }

        mock_find.return_value = None  # Game not already running
        mock_launch.return_value = Mock()
        mock_wait.return_value = (True, {"day": 1, "gameState": "PLAYING"})

        launch.main()

        assert mock_launch.called
        assert mock_wait.called

    @patch('scripts.launch.load_config')
    @patch('sys.argv', ['launch.py'])
    def test_main_config_not_found(self, mock_load_config):
        """Test main with missing config."""
        mock_load_config.side_effect = FileNotFoundError("Config not found")

        with pytest.raises(SystemExit) as exc_info:
            launch.main()

        assert exc_info.value.code == 1

    @patch('scripts.launch.find_game_process')
    @patch('scripts.launch.load_config')
    @patch('sys.argv', ['launch.py'])
    def test_main_already_running(self, mock_find, mock_load_config, tmp_path):
        """Test main when game is already running."""
        game_path = tmp_path / "game"
        game_path.mkdir()

        mock_load_config.return_value = {
            "gamePath": str(game_path),
            "tcpPort": 8484,
        }

        mock_find.return_value = 12345  # Game is running

        with pytest.raises(SystemExit) as exc_info:
            launch.main()

        assert exc_info.value.code == 1

    @patch('scripts.launch.find_game_process')
    @patch('scripts.launch.subprocess.run')
    @patch('scripts.launch.load_config')
    @patch('sys.argv', ['launch.py', '--force'])
    def test_main_force_relaunch(self, mock_find, mock_run, mock_load_config, tmp_path):
        """Test main with --force flag."""
        game_path = tmp_path / "game"
        game_path.mkdir()
        (game_path / "LobotomyCorp.exe").touch()

        mock_load_config.return_value = {
            "gamePath": str(game_path),
            "tcpPort": 8484,
            "launchTimeoutSeconds": 120,
        }

        mock_find.side_effect = [12345, None]  # Found, then killed
        mock_run.return_value = Mock()

        with patch('scripts.launch.launch_game') as mock_launch:
            mock_launch.return_value = Mock()
            with patch('scripts.launch.wait_for_readiness') as mock_wait:
                mock_wait.return_value = (True, None)

                launch.main()

                # Verify kill was attempted
                mock_run.assert_called()

    @patch('scripts.launch.launch_game')
    @patch('scripts.launch.find_game_process')
    @patch('scripts.launch.load_config')
    @patch('sys.argv', ['launch.py', '--no-wait'])
    def test_main_no_wait(self, mock_launch, mock_find, mock_load_config, tmp_path):
        """Test main with --no-wait flag."""
        game_path = tmp_path / "game"
        game_path.mkdir()
        (game_path / "LobotomyCorp.exe").touch()

        mock_load_config.return_value = {
            "gamePath": str(game_path),
            "tcpPort": 8484,
        }

        mock_find.return_value = None
        mock_launch.return_value = Mock()

        launch.main()

        assert mock_launch.called
        # wait_for_readiness should not be called


class TestGetGameState:
    """Test getting game state (implemented in status.py, tested there)."""

    # Note: get_game_state is implemented in status.py, so these tests
    # are moved to test_status.py. This test class is kept as a placeholder
    # for documentation purposes.

    pass


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
