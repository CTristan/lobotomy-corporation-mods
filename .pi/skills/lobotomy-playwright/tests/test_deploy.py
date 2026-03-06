#!/usr/bin/env python3
"""Tests for deploy.py script."""

import json
import subprocess
import pytest
import sys
from pathlib import Path
from unittest.mock import Mock, patch, MagicMock
import scripts.deploy as deploy


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

        with patch.object(deploy, 'CONFIG_PATH', config_path):
            config = deploy.load_config()
            assert config == config_data

    def test_load_config_not_found(self):
        """Test loading missing configuration."""
        with patch.object(deploy, 'CONFIG_PATH', Path("/nonexistent/config.json")):
            with pytest.raises(FileNotFoundError, match="Configuration file not found"):
                deploy.load_config()

    def test_load_config_invalid_json(self, tmp_path):
        """Test loading invalid JSON configuration."""
        config_path = tmp_path / "config.json"
        config_path.write_text("not valid json")

        with patch.object(deploy, 'CONFIG_PATH', config_path):
            with pytest.raises(json.JSONDecodeError):
                deploy.load_config()


class TestBuildProject:
    """Test .NET project building."""

    @patch('scripts.deploy.subprocess.run')
    def test_build_project_success(self, mock_run, tmp_path):
        """Test successful project build."""
        project_path = tmp_path / "TestProject.csproj"
        project_path.touch()

        # Mock subprocess.run to return success
        mock_result = MagicMock(spec=subprocess.CompletedProcess)
        mock_result.returncode = 0
        mock_result.stdout = "Build succeeded"
        mock_result.stderr = ""
        mock_run.return_value = mock_result

        # Create the output directory structure
        bin_dir = tmp_path / "bin" / "Release" / "net35"
        bin_dir.mkdir(parents=True)
        dll_path = bin_dir / "TestProject" / "TestProject.dll"
        dll_path.parent.mkdir(parents=True, exist_ok=True)
        dll_path.touch()

        result = deploy.build_project(project_path, "Release")

        assert result == dll_path
        assert mock_run.called

    @patch('scripts.deploy.subprocess.run')
    def test_build_project_alternate_path(self, mock_run, tmp_path):
        """Test project build with alternate DLL output path."""
        project_path = tmp_path / "TestProject.csproj"
        project_path.touch()

        mock_result = MagicMock(spec=subprocess.CompletedProcess)
        mock_result.returncode = 0
        mock_result.stdout = "Build succeeded"
        mock_run.return_value = mock_result

        # Create DLL in alternate location (not in project-named folder)
        bin_dir = tmp_path / "bin" / "Release" / "net35"
        bin_dir.mkdir(parents=True)
        dll_path = bin_dir / "TestProject.dll"
        dll_path.touch()

        result = deploy.build_project(project_path, "Release")
        assert result == dll_path

    @patch('scripts.deploy.subprocess.run')
    def test_build_project_failure(self, mock_run, tmp_path):
        """Test failed project build."""
        project_path = tmp_path / "TestProject.csproj"
        project_path.touch()

        # Mock subprocess.run to return failure
        def check_returncode():
            raise subprocess.CalledProcessError(1, "cmd")

        mock_result = MagicMock(spec=subprocess.CompletedProcess)
        mock_result.returncode = 1
        mock_result.stdout = "Build failed"
        mock_result.stderr = "Error"
        mock_result.check_returncode.side_effect = check_returncode
        mock_run.return_value = mock_result

        with pytest.raises(subprocess.CalledProcessError):
            deploy.build_project(project_path, "Release")

    @patch('scripts.deploy.subprocess.run')
    def test_build_project_dll_not_found(self, mock_run, tmp_path):
        """Test when built DLL is not found."""
        project_path = tmp_path / "TestProject.csproj"
        project_path.touch()

        mock_result = MagicMock(spec=subprocess.CompletedProcess)
        mock_result.returncode = 0
        mock_result.stdout = "Build succeeded"
        mock_run.return_value = mock_result

        with pytest.raises(FileNotFoundError, match="Built DLL not found"):
            deploy.build_project(project_path, "Release")


class TestDeployDll:
    """Test DLL deployment to BepInEx directories."""

    def test_deploy_dll_success(self, tmp_path):
        """Test successful DLL deployment."""
        source_dll = tmp_path / "source" / "Test.dll"
        source_dll.parent.mkdir(parents=True)
        source_dll.write_text("test content")

        game_path = tmp_path / "game"
        game_path.mkdir()

        result = deploy.deploy_dll(source_dll, game_path, "plugins")

        expected_dest = game_path / "BepInEx" / "plugins" / "Test.dll"
        assert result == expected_dest
        assert expected_dest.exists()
        assert expected_dest.read_text() == "test content"

    def test_deploy_dll_empty_failure(self, tmp_path):
        """Test deployment fails for empty DLL."""
        source_dll = tmp_path / "Test.dll"
        source_dll.touch() # empty

        game_path = tmp_path / "game"
        game_path.mkdir()

        with pytest.raises(OSError, match="Deployed DLL is empty"):
            deploy.deploy_dll(source_dll, game_path, "plugins")

    def test_deploy_dll_creates_bepinex_structure(self, tmp_path):
        """Test that deployment creates BepInEx directory structure."""
        source_dll = tmp_path / "Test.dll"
        source_dll.write_text("content")

        game_path = tmp_path / "game"
        game_path.mkdir()

        deploy.deploy_dll(source_dll, game_path, "plugins")

        bepinex_dir = game_path / "BepInEx"
        plugins_dir = bepinex_dir / "plugins"
        assert bepinex_dir.exists()
        assert plugins_dir.exists()

    @patch('scripts.deploy.shutil.copy2')
    def test_deploy_dll_copy_failure(self, mock_copy, tmp_path):
        """Test deployment handles copy failure gracefully."""
        source_dll = tmp_path / "Test.dll"
        source_dll.write_text("content")

        game_path = tmp_path / "game"
        game_path.mkdir()

        mock_copy.side_effect = OSError("Copy failed")

        with pytest.raises(OSError, match="Copy failed"):
            deploy.deploy_dll(source_dll, game_path, "plugins")


class TestMainFunction:
    """Test main deploy script functionality."""

    @patch('scripts.deploy.deploy_interop_dlls')
    @patch('scripts.deploy.deploy_dll')
    @patch('scripts.deploy.build_project')
    @patch('scripts.deploy.load_config')
    @patch('scripts.deploy.Path.exists')
    @patch('sys.argv', ['deploy.py'])
    def test_main_success(self, mock_deploy_interop, mock_deploy, mock_build, mock_load_config, mock_exists, tmp_path):
        """Test successful deployment workflow."""
        mock_load_config.return_value = {"gamePath": str(tmp_path / "game")}
        mock_exists.return_value = True

        plugin_dll = MagicMock(spec=Path)
        plugin_dll.name = "plugin.dll"
        plugin_dll.stat.return_value.st_size = 1024
        
        retharmony_dll = MagicMock(spec=Path)
        retharmony_dll.name = "retharmony.dll"
        retharmony_dll.stat.return_value.st_size = 512
        
        mock_build.side_effect = [plugin_dll, retharmony_dll]
        mock_deploy.side_effect = [plugin_dll, retharmony_dll]
        mock_deploy_interop.return_value = [plugin_dll]

        # Call main
        deploy.main()

        assert mock_build.call_count == 2
        assert mock_deploy.call_count == 2
        assert mock_deploy_interop.called

    @patch('scripts.deploy.load_config')
    @patch('sys.argv', ['deploy.py'])
    def test_main_game_path_not_exist(self, mock_load_config, tmp_path):
        """Test main with non-existent game path."""
        mock_load_config.return_value = {"gamePath": str(tmp_path / "nonexistent")}
        
        with pytest.raises(SystemExit) as exc:
            deploy.main()
        assert exc.value.code == 1

    @patch('scripts.deploy.load_config')
    @patch('scripts.deploy.Path.exists')
    @patch('sys.argv', ['deploy.py', '--dry-run'])
    def test_main_dry_run(self, mock_load_config, mock_exists, tmp_path):
        """Test dry run."""
        game_path = tmp_path / "game"
        game_path.mkdir()
        mock_load_config.return_value = {"gamePath": str(game_path)}
        
        # Mock exists to return True for everything
        mock_exists.return_value = True

        with pytest.raises(SystemExit) as exc:
            with patch('scripts.deploy.build_project') as mock_build:
                mock_build.return_value = MagicMock(spec=Path)
                deploy.main()
        assert exc.value.code == 0

    @patch('scripts.deploy.deploy_interop_dlls')
    @patch('scripts.deploy.deploy_dll')
    @patch('scripts.deploy.load_config')
    @patch('scripts.deploy.Path.exists')
    @patch('sys.argv', ['deploy.py', '--skip-build'])
    def test_main_skip_build(self, mock_deploy_interop, mock_deploy, mock_load_config, mock_exists, tmp_path):
        """Test skip build flag."""
        game_path = tmp_path / "game"
        game_path.mkdir()
        mock_load_config.return_value = {"gamePath": str(game_path)}
        mock_exists.return_value = True

        dll_mock = MagicMock(spec=Path)
        dll_mock.stat.return_value.st_size = 1024
        mock_deploy.return_value = dll_mock
        mock_deploy_interop.return_value = [dll_mock]

        deploy.main()
        assert mock_deploy.called

    @patch('scripts.deploy.deploy_dll')
    @patch('scripts.deploy.build_project')
    @patch('scripts.deploy.load_config')
    @patch('scripts.deploy.Path.exists')
    @patch('sys.argv', ['deploy.py'])
    def test_main_deployment_failure(self, mock_deploy, mock_build, mock_load_config, mock_exists, tmp_path):
        """Test deployment failure in main."""
        game_path = tmp_path / "game"
        game_path.mkdir()
        mock_load_config.return_value = {"gamePath": str(game_path)}
        mock_exists.return_value = True
        mock_build.return_value = MagicMock(spec=Path)
        mock_deploy.side_effect = OSError("Failed to copy")

        with pytest.raises(SystemExit) as exc:
            deploy.main()
        assert exc.value.code == 1

    @patch('scripts.deploy.build_project')
    @patch('scripts.deploy.load_config')
    @patch('scripts.deploy.Path.exists')
    @patch('sys.argv', ['deploy.py'])
    def test_main_build_failure(self, mock_build, mock_load_config, mock_exists, tmp_path):
        """Test build failure in main."""
        game_path = tmp_path / "game"
        game_path.mkdir()
        mock_load_config.return_value = {"gamePath": str(game_path)}
        mock_exists.return_value = True
        mock_build.side_effect = subprocess.CalledProcessError(1, "cmd")

        with pytest.raises(SystemExit) as exc:
            deploy.main()
        assert exc.value.code == 1

    @patch('scripts.deploy.build_project')
    @patch('scripts.deploy.load_config')
    @patch('scripts.deploy.Path.exists')
    @patch('sys.argv', ['deploy.py'])
    def test_main_build_dll_not_found(self, mock_build, mock_load_config, mock_exists, tmp_path):
        """Test build when DLL is not found after build."""
        game_path = tmp_path / "game"
        game_path.mkdir()
        mock_load_config.return_value = {"gamePath": str(game_path)}
        mock_exists.return_value = True
        mock_build.side_effect = FileNotFoundError("DLL not found")

        with pytest.raises(SystemExit) as exc:
            deploy.main()
        assert exc.value.code == 1


class TestRunCommand:
    """Test run_command function."""

    @patch('scripts.deploy.subprocess.run')
    def test_run_command_success(self, mock_run, tmp_path):
        """Test successful command run."""
        mock_result = MagicMock(spec=subprocess.CompletedProcess)
        mock_result.returncode = 0
        mock_run.return_value = mock_result

        result = deploy.run_command(["test"], tmp_path)
        assert result == mock_result

    @patch('scripts.deploy.subprocess.run')
    def test_run_command_failure(self, mock_run, tmp_path):
        """Test failed command run."""
        def check_returncode():
            raise subprocess.CalledProcessError(1, "cmd")

        mock_result = MagicMock(spec=subprocess.CompletedProcess)
        mock_result.returncode = 1
        mock_result.stdout = "error"
        mock_result.stderr = "error"
        mock_result.check_returncode.side_effect = check_returncode
        mock_run.return_value = mock_result

        with pytest.raises(subprocess.CalledProcessError):
            deploy.run_command(["test"], tmp_path)

    @patch('scripts.deploy.subprocess.run')
    def test_run_command_no_capture(self, mock_run, tmp_path):
        """Test command run without capture."""
        deploy.run_command(["test"], tmp_path, capture=False)
        mock_run.assert_called_with(["test"], cwd=tmp_path, check=True)


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
