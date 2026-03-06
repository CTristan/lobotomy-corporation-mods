#!/usr/bin/env python3
"""Tests for find_game.py script."""

import json
import platform
import pytest
from pathlib import Path
from unittest.mock import Mock, patch, MagicMock
import scripts.find_game as find_game


class TestVdfParsing:
    """Test VDF parsing functionality."""

    def test_parse_vdf_library_paths_empty(self):
        """Test parsing empty VDF content."""
        assert find_game.parse_vdf_library_paths("") == []
        assert find_game.parse_vdf_library_paths(None) == []
        assert find_game.parse_vdf_library_paths("   ") == []

    def test_parse_vdf_library_paths_valid(self):
        """Test parsing valid VDF content."""
        vdf_content = '''
"libraryfolders"
{
  "0"
  {
    "path"		"C:\\Program Files (x86)\\Steam"
    "label"		""
  }
  "1"
  {
    "path"		"D:\\Games\\Steam"
  }
}
'''
        paths = find_game.parse_vdf_library_paths(vdf_content)
        assert len(paths) == 2
        assert "C:\\Program Files (x86)\\Steam" in paths
        assert "D:\\Games\\Steam" in paths


class TestCrossOverPathConversion:
    """Test CrossOver Windows-to-macOS path conversion."""

    def test_convert_crossover_path_z_drive(self):
        """Test converting Z: drive paths."""
        # Note: In Python strings, \\ is a single backslash
        assert find_game.convert_crossover_path("Z:\\Volumes\\MyDrive\\Steam") == "/Volumes/MyDrive/Steam"
        assert find_game.convert_crossover_path("Z:\\Volumes\\WD_BLACK SN770 1TB\\SteamLibrary") == "/Volumes/WD_BLACK SN770 1TB/SteamLibrary"

    def test_convert_crossover_path_other_drive(self):
        """Test that other drives return None."""
        assert find_game.convert_crossover_path("C:\\\\Program Files\\\\Steam") is None
        assert find_game.convert_crossover_path("D:\\\\Games\\\\Steam") is None

    def test_convert_crossover_path_empty(self):
        """Test converting empty string."""
        assert find_game.convert_crossover_path("") is None


class TestPathValidation:
    """Test game path validation."""

    def test_is_valid_game_path_nonexistent(self, tmp_path):
        """Test validation with non-existent path."""
        assert not find_game.is_valid_game_path(tmp_path / "nonexistent")

    def test_is_valid_game_path_without_dll(self, tmp_path):
        """Test validation without required DLL."""
        assert not find_game.is_valid_game_path(tmp_path)

    def test_is_valid_game_path_with_dll(self, tmp_path):
        """Test validation with required DLL present."""
        managed_dir = tmp_path / "LobotomyCorp_Data" / "Managed"
        managed_dir.mkdir(parents=True)
        (managed_dir / "Assembly-CSharp.dll").touch()
        assert find_game.is_valid_game_path(tmp_path)

    def test_is_bepinex_installed_missing(self, tmp_path):
        """Test BepInEx detection when not installed."""
        assert not find_game.is_bepinex_installed(tmp_path)

    def test_is_bepinex_installed_partial(self, tmp_path):
        """Test BepInEx detection with only one component."""
        bepinex_dir = tmp_path / "BepInEx" / "core"  # Note: capital 'E'
        bepinex_dir.mkdir(parents=True)
        (bepinex_dir / "BepInEx.dll").touch()
        assert not find_game.is_bepinex_installed(tmp_path)

    def test_is_bepinex_installed_complete(self, tmp_path):
        """Test BepInEx detection with both components."""
        bepinex_dir = tmp_path / "BepInEx" / "core"  # Note: capital 'E'
        bepinex_dir.mkdir(parents=True)
        (bepinex_dir / "BepInEx.dll").touch()
        (tmp_path / "doorstop_config.ini").touch()
        assert find_game.is_bepinex_installed(tmp_path)


class TestGetSteamLibraryPaths:
    """Test Steam library path extraction."""

    def test_get_steam_library_paths_nonexistent_steam(self, tmp_path):
        """Test with non-existent Steam installation."""
        steam_path = tmp_path / "Steam"
        assert find_game.get_steam_library_paths(steam_path) == []

    @patch('scripts.find_game.parse_vdf_library_paths')
    def test_get_steam_library_paths_with_vdf(self, mock_parse, tmp_path):
        """Test with libraryfolders.vdf present."""
        steam_path = tmp_path / "Steam"
        steam_path.mkdir()

        steamapps_dir = steam_path / "steamapps"
        steamapps_dir.mkdir()

        libraryfolders = steamapps_dir / "libraryfolders.vdf"
        libraryfolders.write_text('"path" "Z:\\\\Volumes\\\\Drive\\\\Steam"')

        mock_parse.return_value = ["Z:\\\\Volumes\\\\Drive\\\\Steam"]

        paths = find_game.get_steam_library_paths(steam_path)
        assert len(paths) == 1
        assert paths[0].name == "LobotomyCorp"

    @patch('scripts.find_game.parse_vdf_library_paths')
    def test_get_steam_library_paths_without_vdf(self, mock_parse, tmp_path):
        """Test without libraryfolders.vdf (default path)."""
        steam_path = tmp_path / "Steam"
        steam_path.mkdir()
        (steam_path / "steamapps").mkdir()

        mock_parse.return_value = []

        paths = find_game.get_steam_library_paths(steam_path)
        assert len(paths) == 1
        assert paths[0].name == "LobotomyCorp"


class TestRecursiveSearch:
    """Test recursive game path search."""

    def test_find_game_path_recursive_at_root(self, tmp_path):
        """Test finding game at root path."""
        managed_dir = tmp_path / "LobotomyCorp_Data" / "Managed"
        managed_dir.mkdir(parents=True)
        (managed_dir / "Assembly-CSharp.dll").touch()

        result = find_game.find_game_path_recursive(tmp_path)
        assert result == tmp_path

    def test_find_game_path_recursive_in_subdir(self, tmp_path):
        """Test finding game in subdirectory."""
        game_dir = tmp_path / "Games" / "LobotomyCorp"
        managed_dir = game_dir / "LobotomyCorp_Data" / "Managed"
        managed_dir.mkdir(parents=True)
        (managed_dir / "Assembly-CSharp.dll").touch()

        result = find_game.find_game_path_recursive(tmp_path)
        assert result == game_dir

    def test_find_game_path_recursive_not_found(self, tmp_path):
        """Test search when game not found."""
        result = find_game.find_game_path_recursive(tmp_path, max_depth=2)
        assert result is None

    def test_find_game_path_recursive_permission_error(self, tmp_path):
        """Test search handles permission errors gracefully."""
        game_dir = tmp_path / "LobotomyCorp"
        game_dir.mkdir()

        # Mock iterdir to raise PermissionError
        with patch.object(Path, 'iterdir', side_effect=PermissionError):
            result = find_game.find_game_path_recursive(tmp_path)
            assert result is None


class TestConfigManagement:
    """Test configuration file management."""

    def test_create_config(self, tmp_path):
        """Test creating configuration file."""
        game_path = tmp_path / "LobotomyCorp"
        game_path.mkdir()

        with patch.object(find_game, 'CONFIG_PATH', tmp_path / "config.json"):
            find_game.create_config(game_path, "TestBottle", 9000)

            config_path = tmp_path / "config.json"
            assert config_path.exists()

            with open(config_path) as f:
                config = json.load(f)

            assert config["gamePath"] == str(game_path)
            assert config["crossoverBottle"] == "TestBottle"
            assert config["tcpPort"] == 9000
            assert config["launchTimeoutSeconds"] == 120
            assert config["shutdownTimeoutSeconds"] == 10

    def test_load_config_success(self, tmp_path):
        """Test loading valid configuration."""
        config_path = tmp_path / "config.json"
        config_data = {
            "gamePath": "/path/to/game",
            "crossoverBottle": "TestBottle",
            "tcpPort": 8484,
        }
        config_path.write_text(json.dumps(config_data))

        with patch.object(find_game, 'CONFIG_PATH', config_path):
            config = find_game.load_config()
            assert config == config_data

    def test_load_config_not_found(self):
        """Test loading missing configuration."""
        with patch.object(find_game, 'CONFIG_PATH', Path("/nonexistent/config.json")):
            with pytest.raises(FileNotFoundError, match="Configuration file not found"):
                find_game.load_config()

    def test_load_config_invalid_json(self, tmp_path):
        """Test loading invalid JSON configuration."""
        config_path = tmp_path / "config.json"
        config_path.write_text("not valid json")

        with patch.object(find_game, 'CONFIG_PATH', config_path):
            with pytest.raises(json.JSONDecodeError):
                find_game.load_config()


class TestFindGamePath:
    """Test game path auto-detection."""

    @patch('scripts.find_game.platform.system', return_value='Windows')
    def test_find_game_path_windows_not_implemented(self, mock_platform):
        """Test Windows platform (not yet implemented)."""
        result = find_game.find_game_path()
        assert result is None

    @patch('scripts.find_game.platform.system', return_value='Linux')
    def test_find_game_path_linux_not_implemented(self, mock_platform):
        """Test Linux platform (not yet implemented)."""
        result = find_game.find_game_path()
        assert result is None

    @patch('scripts.find_game.platform.system', return_value='Darwin')
    @patch('scripts.find_game.get_macos_paths', return_value=[])
    def test_find_game_path_macos_no_candidates(self, mock_get_paths, mock_platform):
        """Test macOS with no candidate paths."""
        result = find_game.find_game_path()
        assert result is None

    @patch('scripts.find_game.platform.system', return_value='Darwin')
    @patch('scripts.find_game.is_valid_game_path')
    @patch('scripts.find_game.get_macos_paths')
    def test_find_game_path_macos_success(self, mock_get_paths, mock_is_valid, mock_platform, tmp_path):
        """Test macOS with valid game path."""
        game_path = tmp_path / "LobotomyCorp"
        mock_get_paths.return_value = [(game_path, "TestBottle")]
        mock_is_valid.return_value = True

        result = find_game.find_game_path()
        assert result == (game_path, "TestBottle")


class TestGetMacOSPaths:
    """Test macOS CrossOver bottle scanning."""

    @patch('scripts.find_game.Path.home')
    def test_get_macos_paths_bottles_not_found(self, mock_home, tmp_path):
        """Test with non-existent bottles directory."""
        mock_home.return_value = tmp_path
        bottles_dir = tmp_path / "Library" / "Application Support" / "CrossOver" / "Bottles"
        # Don't create it

        result = find_game.get_macos_paths()
        assert result == []

    @patch('scripts.find_game.Path.home')
    @patch('scripts.find_game.get_steam_library_paths')
    @patch('scripts.find_game.find_game_path_recursive')
    def test_get_macos_paths_with_bottles(
        self, mock_home, mock_steam, mock_recursive, tmp_path
    ):
        """Test with valid bottles."""
        # Create the bottles directory structure
        library_dir = tmp_path / "Library"
        bottles_dir = library_dir / "Application Support" / "CrossOver" / "Bottles"
        bottles_dir.mkdir(parents=True)

        bottle1 = bottles_dir / "Bottle1"
        bottle1.mkdir()

        # Create the Steam directory structure that get_macos_paths looks for
        drive_c = bottle1 / "drive_c"
        steam_dir = drive_c / "Program Files (x86)" / "Steam"
        steam_dir.mkdir(parents=True)

        game_path = tmp_path / "game"
        mock_steam.return_value = [game_path]
        mock_recursive.return_value = None

        mock_home.return_value = tmp_path

        result = find_game.get_macos_paths()
        assert len(result) > 0


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
