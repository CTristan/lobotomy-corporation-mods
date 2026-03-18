// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Hemocode.Playwright.JsonModels;
using UnityEngine.SceneManagement;

#endregion

namespace Hemocode.Playwright.Queries
{
    /// <summary>
    /// Queries for game state from GameManager and PlayerModel.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class GameStateQueries
    {
        private static string CurrentScene
        {
            get
            {
                try
                {
                    return SceneManager.GetActiveScene().name;
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

        public static bool IsOnTitleScreen()
        {
            var sceneName = CurrentScene;
            return sceneName == "NewTitleScene" || sceneName == "AlterTitleScene" || sceneName == "Intro";
        }

        public static bool IsOnGameScreen()
        {
            return CurrentScene == "Main";
        }

#if DEBUG
        // Internal override for unit testing
        internal static bool? IsGameQueryableOverride { get; set; }
#endif

        public static GameStateData GetStatus()
        {
            try
            {
                var sceneName = CurrentScene;
                Server.TcpServer.LogDebug($"[LobotomyPlaywright] GetStatus called, scene={sceneName}");

                var gameManager = GameManager.currentGameManager;
                var playerModel = PlayerModel.instance;

                // Title/intro screen state
                if (IsOnTitleScreen())
                {
                    var globalGameManager = GlobalGameManager.instance;

                    // For Intro scene, return limited info
                    if (sceneName == "Intro")
                    {
                        return new GameStateData
                        {
                            day = playerModel != null ? playerModel.GetDay() : 0,
                            gameState = "INTRO",
                            gameSpeed = 1,
                            energy = 0,
                            energyQuota = 0,
                            managementStarted = false,
                            isPaused = false,
                            emergencyLevel = "NORMAL",
                            playTime = 0,
                            lobPoints = 0
                        };
                    }

                    // For actual title screen
                    return new GameStateData
                    {
                        day = playerModel != null ? playerModel.GetDay() : 0,
                        gameState = "TITLE_SCREEN",
                        gameSpeed = 1,
                        energy = 0,
                        energyQuota = 0,
                        managementStarted = false,
                        isPaused = false,
                        emergencyLevel = "NORMAL",
                        playTime = 0,
                        lobPoints = 0
                    };
                }

                // Main game state
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

                var day = playerModel.GetDay();
                var currentEnergy = energyModel.GetEnergy();
                var energyQuota = StageTypeInfo.instnace.GetEnergyNeed(day);

                return new GameStateData
                {
                    day = day,
                    gameState = gameManager.state.ToString(),
                    gameSpeed = gameManager.gameSpeedLevel,
                    energy = currentEnergy,
                    energyQuota = energyQuota,
                    managementStarted = gameManager.ManageStarted,
                    isPaused = gameManager.state == GameState.STOP,
                    emergencyLevel = PlayerModel.emergencyController != null ? PlayerModel.emergencyController.currentLevel.ToString() : "NORMAL",
                    playTime = gameManager.PlayTime,
                    lobPoints = 0 // Access via reflection if needed
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
#if DEBUG
            if (IsGameQueryableOverride.HasValue)
            {
                return IsGameQueryableOverride.Value;
            }
#endif
            try
            {
                // Queryable on intro, title screen, or main game
                var sceneName = CurrentScene;
                if (sceneName == "NewTitleScene" || sceneName == "AlterTitleScene" ||
                    sceneName == "Intro" || sceneName == "Main")
                {
                    return true;
                }

                // Also queryable if GameManager is available (for other game scenes)
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
