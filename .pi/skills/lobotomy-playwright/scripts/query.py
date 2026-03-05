#!/usr/bin/env python3
"""CLI for querying Lobotomy Corporation game state."""

import argparse
import json
import sys
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent))

from client import LobotomyPlaywrightClient


def format_agent(agent: dict, json_output: bool) -> str:
    """Format agent data for display."""
    if json_output:
        return json.dumps(agent, indent=2)

    return f"""
Agent: {agent.get('name')} (ID: {agent.get('instanceId')})
  HP: {agent.get('hp', 0)}/{agent.get('maxHp', 0)}
  Mental: {agent.get('mental', 0)}/{agent.get('maxMental', 0)}
  Stats: Fortitude {agent.get('fortitude', 0)}, Prudence {agent.get('prudence', 0)},
          Temperance {agent.get('temperance', 0)}, Justice {agent.get('justice', 0)}
  State: {agent.get('state', 'UNKNOWN')}
  Department: {agent.get('currentSefira', 'None')}
  Gifts: {len(agent.get('giftIds', []))} equipped
  Weapon: {agent.get('weaponId', 'None')}
  Armor: {agent.get('armorId', 'None')}
  Status: {'DEAD' if agent.get('isDead') else 'PANIC' if agent.get('isPanicking') else 'Normal'}
""".strip()


def format_creature(creature: dict, json_output: bool) -> str:
    """Format creature data for display."""
    if json_output:
        return json.dumps(creature, indent=2)

    qliphoth = f"{creature.get('qliphothCounter', 0)}/{creature.get('maxQliphothCounter', 0)}"

    return f"""
Abnormality: {creature.get('name')} (ID: {creature.get('instanceId')})
  Risk Level: {creature.get('riskLevel', 'UNKNOWN')}
  State: {creature.get('state', 'UNKNOWN')}
  Qliphoth: {qliphoth}
  Feeling: {creature.get('feelingState', 'UNKNOWN')}
  Department: {creature.get('currentSefira', 'None')}
  Work Count: {creature.get('workCount', 0)}
  Status: {'ESCAPING' if creature.get('isEscaping') else 'SUPPRESSED' if creature.get('isSuppressed') else 'Normal'}
""".strip()


def format_game_state(state: dict, json_output: bool) -> str:
    """Format game state for display."""
    if json_output:
        return json.dumps(state, indent=2)

    energy_pct = (state.get('energy', 0) / max(state.get('energyQuota', 1), 1)) * 100

    return f"""
Game State:
  Day: {state.get('day', 0)}
  Phase: {state.get('gameState', 'UNKNOWN')}
  Speed: {state.get('gameSpeed', 1)}x
  Energy: {state.get('energy', 0):.1f}/{state.get('energyQuota', 0):.1f} ({energy_pct:.1f}%)
  Emergency: {state.get('emergencyLevel', 'NORMAL')}
  Management Started: {state.get('managementStarted', False)}
  Paused: {state.get('isPaused', False)}
  Play Time: {state.get('playTime', 0):.1f}s
  LOB Points: {state.get('lobPoints', 0)}
""".strip()


def format_sefira(sefira: dict, json_output: bool) -> str:
    """Format department data for display."""
    if json_output:
        return json.dumps(sefira, indent=2)

    status = "Closed"
    if sefira.get('isOpen'):
        status = f"Open (Level {sefira.get('openLevel', 0)})"

    return f"""
Department: {sefira.get('name')} ({sefira.get('sefiraEnum')})
  Status: {status}
  Agents: {len(sefira.get('agentIds', []))}
  Creatures: {len(sefira.get('creatureIds', []))}
  Officers: {sefira.get('officerCount', 0)}
""".strip()


def main():
    parser = argparse.ArgumentParser(description="Query Lobotomy Corporation game state")
    parser.add_argument("target", help="Query target: agents, creatures, game, or departments")
    parser.add_argument("id", nargs="?", help="Specific ID to query (for agents/creatures)")
    parser.add_argument("--host", default="localhost", help="TCP host (default: localhost)")
    parser.add_argument("--port", type=int, default=8484, help="TCP port (default: 8484)")
    parser.add_argument("--json", action="store_true", help="Output raw JSON")

    args = parser.parse_args()

    try:
        with LobotomyPlaywrightClient(host=args.host, port=args.port) as client:
            target = args.target.lower()

            if target in ("agents", "agent"):
                if args.id:
                    params = {"id": int(args.id)}
                    data = client.query("agents", params)
                    print(format_agent(data, args.json))
                else:
                    data = client.query("agents")
                    if args.json:
                        print(json.dumps(data, indent=2))
                    else:
                        for agent in data:
                            print(format_agent(agent, args.json))
                            print("---")

            elif target in ("creatures", "creature", "abnormalities", "abnormality"):
                if args.id:
                    params = {"id": int(args.id)}
                    data = client.query("creatures", params)
                    print(format_creature(data, args.json))
                else:
                    data = client.query("creatures")
                    if args.json:
                        print(json.dumps(data, indent=2))
                    else:
                        for creature in data:
                            print(format_creature(creature, args.json))
                            print("---")

            elif target in ("game", "status"):
                data = client.query("game")
                print(format_game_state(data, args.json))

            elif target in ("departments", "department", "sefira", "sefiras"):
                data = client.query("sefira")
                if args.json:
                    print(json.dumps(data, indent=2))
                else:
                    for sefira in data:
                        print(format_sefira(sefira, args.json))
                        print("---")

            else:
                print(f"Unknown target: {args.target}", file=sys.stderr)
                print("Valid targets: agents, creatures, game, departments", file=sys.stderr)
                sys.exit(1)

    except ConnectionError as e:
        print(f"Connection error: {e}", file=sys.stderr)
        print("Ensure Lobotomy Corporation is running with LobotomyPlaywright plugin.", file=sys.stderr)
        sys.exit(1)
    except RuntimeError as e:
        print(f"Query error: {e}", file=sys.stderr)
        sys.exit(1)
    except KeyboardInterrupt:
        print("\nInterrupted")
        sys.exit(0)


if __name__ == "__main__":
    main()
