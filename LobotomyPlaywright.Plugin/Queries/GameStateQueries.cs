// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Reflection;

namespace LobotomyPlaywright.Queries
{
    /// <summary>
    /// Queries for game state from GameManager and PlayerModel.
    /// </summary>
    public static class GameStateQueries
    {
        public static GameStateData GetStatus()
        {
            try
            {
                var gameManager = GameManager.currentGameManager;
                var playerModel = PlayerModel.instance;
                var energyModel = EnergyModel.instance;

                if (gameManager == null)
                {
                    throw new InvalidOperationException("GameManager.currentGameManager is null. Game may not be initialized.");
                }

                if (playerModel == null)
                {
                    throw new InvalidOperationException("PlayerModel.instance is null. Game may not be initialized.");
                }

                if (energyModel == null)
                {
                    throw new InvalidOperationException("EnergyModel.instance is null. Game may not be initialized.");
                }

                int day = playerModel.GetDay();
                float currentEnergy = energyModel.GetEnergy();
                float energyQuota = StageTypeInfo.instnace.GetEnergyNeed(day);

                return new GameStateData
                {
                    Day = day,
                    GameState = gameManager.state.ToString(),
                    GameSpeed = gameManager.gameSpeedLevel,
                    Energy = currentEnergy,
                    EnergyQuota = energyQuota,
                    ManagementStarted = gameManager.ManageStarted,
                    IsPaused = gameManager.state == GameState.STOP,
                    EmergencyLevel = PlayerModel.emergencyController != null ? PlayerModel.emergencyController.currentLevel.ToString() : "NORMAL",
                    PlayTime = gameManager.PlayTime,
                    LobPoints = 0 // Access via reflection if needed
                };
            }
            catch (TypeInitializationException ex)
            {
                throw new InvalidOperationException("Game types could not be loaded. Game may not be initialized.", ex);
            }
            catch (FileNotFoundException ex)
            {
                throw new InvalidOperationException("Game assemblies could not be loaded. Game may not be initialized.", ex);
            }
        }

        public static bool IsGameQueryable()
        {
            try
            {
                return GameManager.currentGameManager != null &&
                       PlayerModel.instance != null &&
                       AgentManager.instance != null &&
                       CreatureManager.instance != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
