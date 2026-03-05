#!/usr/bin/env python3
"""Tests for query CLI."""

import sys
import json
import pytest
from io import StringIO
from unittest.mock import Mock, patch, MagicMock
from scripts import query


class TestQueryFormatting:
    """Test query output formatting functions."""

    def test_format_agent_without_json(self):
        """Test agent formatting without JSON output."""
        agent = {
            "instanceId": 1,
            "name": "Sarah",
            "hp": 100,
            "maxHp": 100,
            "mental": 50,
            "maxMental": 100,
            "fortitude": 50,
            "prudence": 60,
            "temperance": 55,
            "justice": 45,
            "state": "IDLE",
            "currentSefira": "KETHER",
            "giftIds": ["gift1", "gift2"],
            "weaponId": "weapon1",
            "armorId": "armor1",
            "isDead": False,
            "isPanicking": False,
        }

        result = query.format_agent(agent, json_output=False)

        assert "Sarah" in result
        assert "ID: 1" in result
        assert "HP: 100/100" in result
        assert "Mental: 50/100" in result
        assert "Fortitude 50" in result
        assert "State: IDLE" in result
        assert "Department: KETHER" in result
        assert "2 equipped" in result

    def test_format_agent_with_json(self):
        """Test agent formatting with JSON output."""
        agent = {"instanceId": 1, "name": "Sarah"}

        result = query.format_agent(agent, json_output=True)

        parsed = json.loads(result)
        assert parsed["instanceId"] == 1
        assert parsed["name"] == "Sarah"

    def test_format_agent_missing_fields(self):
        """Test agent formatting with missing fields."""
        agent = {"instanceId": 1, "name": "Test"}

        result = query.format_agent(agent, json_output=False)

        assert "Test" in result
        assert "HP: 0/0" in result  # Default values

    def test_format_agent_dead(self):
        """Test agent formatting for dead agent."""
        agent = {
            "instanceId": 1,
            "name": "DeadAgent",
            "hp": 0,
            "maxHp": 100,
            "mental": 0,
            "maxMental": 100,
            "fortitude": 50,
            "prudence": 60,
            "temperance": 55,
            "justice": 45,
            "state": "DEAD",
            "currentSefira": "KETHER",
            "giftIds": [],
            "weaponId": "",
            "armorId": "",
            "isDead": True,
            "isPanicking": False,
        }

        result = query.format_agent(agent, json_output=False)

        assert "DEAD" in result
        assert "Status: DEAD" in result

    def test_format_agent_panicking(self):
        """Test agent formatting for panicking agent."""
        agent = {
            "instanceId": 1,
            "name": "PanicAgent",
            "hp": 100,
            "maxHp": 100,
            "mental": 0,
            "maxMental": 100,
            "fortitude": 50,
            "prudence": 60,
            "temperance": 55,
            "justice": 45,
            "state": "PANIC",
            "currentSefira": "KETHER",
            "giftIds": [],
            "weaponId": "",
            "armorId": "",
            "isDead": False,
            "isPanicking": True,
        }

        result = query.format_agent(agent, json_output=False)

        assert "PANIC" in result
        assert "Status: PANIC" in result

    def test_format_creature_without_json(self):
        """Test creature formatting without JSON output."""
        creature = {
            "instanceId": 100001,
            "name": "Scorched Girl",
            "riskLevel": "WAW",
            "state": "IDLE",
            "qliphothCounter": 2,
            "maxQliphothCounter": 3,
            "feelingState": "GOOD",
            "currentSefira": "KETHER",
            "workCount": 15,
            "isEscaping": False,
            "isSuppressed": False,
        }

        result = query.format_creature(creature, json_output=False)

        assert "Scorched Girl" in result
        assert "ID: 100001" in result
        assert "Risk Level: WAW" in result
        assert "State: IDLE" in result
        assert "Qliphoth: 2/3" in result
        assert "Feeling: GOOD" in result
        assert "Work Count: 15" in result

    def test_format_creature_with_json(self):
        """Test creature formatting with JSON output."""
        creature = {"instanceId": 100001, "name": "Test"}

        result = query.format_creature(creature, json_output=True)

        parsed = json.loads(result)
        assert parsed["instanceId"] == 100001
        assert parsed["name"] == "Test"

    def test_format_creature_escaping(self):
        """Test creature formatting for escaping creature."""
        creature = {
            "instanceId": 100001,
            "name": "EscapingCreature",
            "riskLevel": "ALEPH",
            "state": "ESCAPE",
            "qliphothCounter": 0,
            "maxQliphothCounter": 3,
            "feelingState": "BAD",
            "currentSefira": "KETHER",
            "workCount": 10,
            "isEscaping": True,
            "isSuppressed": False,
        }

        result = query.format_creature(creature, json_output=False)

        assert "ESCAPING" in result
        assert "Status: ESCAPING" in result

    def test_format_creature_suppressed(self):
        """Test creature formatting for suppressed creature."""
        creature = {
            "instanceId": 100001,
            "name": "SuppressedCreature",
            "riskLevel": "WAW",
            "state": "IDLE",
            "qliphothCounter": 2,
            "maxQliphothCounter": 3,
            "feelingState": "GOOD",
            "currentSefira": "KETHER",
            "workCount": 10,
            "isEscaping": False,
            "isSuppressed": True,
        }

        result = query.format_creature(creature, json_output=False)

        assert "SUPPRESSED" in result
        assert "Status: SUPPRESSED" in result

    def test_format_game_state_without_json(self):
        """Test game state formatting without JSON output."""
        state = {
            "day": 15,
            "gameState": "PLAYING",
            "gameSpeed": 2,
            "energy": 450.0,
            "energyQuota": 500.0,
            "managementStarted": True,
            "isPaused": False,
            "emergencyLevel": "NONE",
            "playTime": 120.5,
            "lobPoints": 5000,
        }

        result = query.format_game_state(state, json_output=False)

        assert "Day: 15" in result
        assert "Phase: PLAYING" in result
        assert "Speed: 2x" in result
        assert "Energy: 450.0/500.0" in result
        assert "90.0%" in result
        assert "Emergency: NONE" in result
        assert "Management Started: True" in result
        assert "Paused: False" in result
        assert "Play Time: 120.5s" in result

    def test_format_game_state_with_json(self):
        """Test game state formatting with JSON output."""
        state = {"day": 1, "gameState": "STOP"}

        result = query.format_game_state(state, json_output=True)

        parsed = json.loads(result)
        assert parsed["day"] == 1
        assert parsed["gameState"] == "STOP"

    def test_format_sefira_without_json(self):
        """Test sefira formatting without JSON output."""
        sefira = {
            "name": "KETHER",
            "sefiraEnum": "KETHER",
            "isOpen": True,
            "openLevel": 3,
            "agentIds": [1, 2, 3],
            "creatureIds": [100001, 100002],
            "officerCount": 2,
        }

        result = query.format_sefira(sefira, json_output=False)

        assert "Department: KETHER" in result
        assert "Status: Open" in result
        assert "Level 3" in result
        assert "Agents: 3" in result
        assert "Creatures: 2" in result
        assert "Officers: 2" in result

    def test_format_sefira_closed(self):
        """Test sefira formatting for closed department."""
        sefira = {
            "name": "CHOKHMAH",
            "sefiraEnum": "CHOKHMAH",
            "isOpen": False,
            "openLevel": 0,
            "agentIds": [],
            "creatureIds": [],
            "officerCount": 0,
        }

        result = query.format_sefira(sefira, json_output=False)

        assert "Department: CHOKHMAH" in result
        assert "Status: Closed" in result

    def test_format_sefira_with_json(self):
        """Test sefira formatting with JSON output."""
        sefira = {"name": "KETHER", "sefiraEnum": "KETHER"}

        result = query.format_sefira(sefira, json_output=True)

        parsed = json.loads(result)
        assert parsed["name"] == "KETHER"
        assert parsed["sefiraEnum"] == "KETHER"


class TestQueryMain:
    """Test main query CLI function."""

    @patch("scripts.query.LobotomyPlaywrightClient")
    def test_main_agents_list(self, mock_client_class):
        """Test querying all agents."""
        mock_client = Mock()
        mock_client.query.return_value = [{"name": "Agent1"}, {"name": "Agent2"}]
        mock_client_class.return_value.__enter__ = Mock(return_value=mock_client)
        mock_client_class.return_value.__exit__ = Mock(return_value=False)

        with patch("sys.argv", ["query.py", "agents"]):
            with patch("sys.stdout", new_callable=StringIO):
                query.main()

        mock_client.query.assert_called_once_with("agents")

    @patch("scripts.query.LobotomyPlaywrightClient")
    def test_main_agents_specific_id(self, mock_client_class):
        """Test querying specific agent by ID."""
        mock_client = Mock()
        mock_client.query.return_value = {"name": "Agent1", "instanceId": 1}
        mock_client_class.return_value.__enter__ = Mock(return_value=mock_client)
        mock_client_class.return_value.__exit__ = Mock(return_value=False)

        with patch("sys.argv", ["query.py", "agents", "1"]):
            with patch("sys.stdout", new_callable=StringIO):
                query.main()

        mock_client.query.assert_called_once_with("agents", {"id": 1})

    @patch("scripts.query.LobotomyPlaywrightClient")
    def test_main_creatures_list(self, mock_client_class):
        """Test querying all creatures."""
        mock_client = Mock()
        mock_client.query.return_value = [{"name": "Creature1"}]
        mock_client_class.return_value.__enter__ = Mock(return_value=mock_client)
        mock_client_class.return_value.__exit__ = Mock(return_value=False)

        with patch("sys.argv", ["query.py", "creatures"]):
            with patch("sys.stdout", new_callable=StringIO):
                query.main()

        mock_client.query.assert_called_once_with("creatures")

    @patch("scripts.query.LobotomyPlaywrightClient")
    def test_main_game_status(self, mock_client_class):
        """Test querying game status."""
        mock_client = Mock()
        mock_client.query.return_value = {"day": 10, "gameState": "PLAYING"}
        mock_client_class.return_value.__enter__ = Mock(return_value=mock_client)
        mock_client_class.return_value.__exit__ = Mock(return_value=False)

        with patch("sys.argv", ["query.py", "game"]):
            with patch("sys.stdout", new_callable=StringIO):
                query.main()

        mock_client.query.assert_called_once_with("game")

    @patch("scripts.query.LobotomyPlaywrightClient")
    def test_main_departments_list(self, mock_client_class):
        """Test querying all departments."""
        mock_client = Mock()
        mock_client.query.return_value = [{"name": "KETHER"}]
        mock_client_class.return_value.__enter__ = Mock(return_value=mock_client)
        mock_client_class.return_value.__exit__ = Mock(return_value=False)

        with patch("sys.argv", ["query.py", "departments"]):
            with patch("sys.stdout", new_callable=StringIO):
                query.main()

        mock_client.query.assert_called_once_with("sefira")

    @patch("scripts.query.LobotomyPlaywrightClient")
    def test_main_json_flag(self, mock_client_class):
        """Test --json flag outputs raw JSON."""
        mock_client = Mock()
        mock_client.query.return_value = [{"name": "Agent1"}]
        mock_client_class.return_value.__enter__ = Mock(return_value=mock_client)
        mock_client_class.return_value.__exit__ = Mock(return_value=False)

        with patch("sys.argv", ["query.py", "agents", "--json"]):
            with patch("sys.stdout", new_callable=StringIO) as mock_stdout:
                query.main()
                output = mock_stdout.getvalue()

            # Should contain raw JSON
            assert '"name": "Agent1"' in output or '{"name": "Agent1"}' in output

    @patch("scripts.query.LobotomyPlaywrightClient")
    @patch("sys.stderr", new_callable=StringIO)
    def test_main_connection_error(self, mock_stderr, mock_client_class):
        """Test handling connection errors."""
        mock_client_class.side_effect = ConnectionError("Connection refused")

        with patch("sys.argv", ["query.py", "agents"]):
            with pytest.raises(SystemExit) as exc_info:
                query.main()

            assert exc_info.value.code == 1

        stderr_output = mock_stderr.getvalue()
        assert "Connection error" in stderr_output

    @patch("scripts.query.LobotomyPlaywrightClient")
    @patch("sys.stderr", new_callable=StringIO)
    def test_main_query_error(self, mock_stderr, mock_client_class):
        """Test handling query errors."""
        mock_client = Mock()
        mock_client.query.side_effect = RuntimeError("Game not ready")
        mock_client_class.return_value.__enter__ = Mock(return_value=mock_client)
        mock_client_class.return_value.__exit__ = Mock(return_value=False)

        with patch("sys.argv", ["query.py", "agents"]):
            with pytest.raises(SystemExit) as exc_info:
                query.main()

            assert exc_info.value.code == 1

        stderr_output = mock_stderr.getvalue()
        assert "Query error" in stderr_output

    @patch("scripts.query.LobotomyPlaywrightClient")
    def test_main_unknown_target(self, mock_client_class):
        """Test handling unknown target."""
        with patch("sys.argv", ["query.py", "unknown"]):
            with pytest.raises(SystemExit) as exc_info:
                query.main()

            assert exc_info.value.code == 1

    @patch("scripts.query.LobotomyPlaywrightClient")
    def test_main_custom_host_port(self, mock_client_class):
        """Test custom host and port arguments."""
        mock_client = Mock()
        mock_client.query.return_value = []
        mock_client_class.return_value.__enter__ = Mock(return_value=mock_client)
        mock_client_class.return_value.__exit__ = Mock(return_value=False)

        with patch("sys.argv", ["query.py", "agents", "--host", "192.168.1.1", "--port", "9000"]):
            with patch("sys.stdout", new_callable=StringIO):
                query.main()

        mock_client_class.assert_called_once_with(host="192.168.1.1", port=9000)

    @patch("scripts.query.LobotomyPlaywrightClient")
    def test_main_keyboard_interrupt(self, mock_client_class):
        """Test handling keyboard interrupt."""
        mock_client_class.side_effect = KeyboardInterrupt()

        with patch("sys.argv", ["query.py", "agents"]):
            with pytest.raises(SystemExit) as exc_info:
                query.main()

            assert exc_info.value.code == 0


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
