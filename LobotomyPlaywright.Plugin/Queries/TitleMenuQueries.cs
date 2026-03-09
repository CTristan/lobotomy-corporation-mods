// SPDX-License-Identifier: MIT

using System;
using LobotomyPlaywright.JsonModels;
using UnityEngine.SceneManagement;

namespace LobotomyPlaywright.Queries
{
    /// <summary>
    /// Queries for title menu state and operations.
    /// </summary>
    public static class TitleMenuQueries
    {
        public static TitleMenuData GetTitleMenuStatus()
        {
            try
            {
                var globalGameManager = GlobalGameManager.instance;

                if (globalGameManager == null)
                {
                    throw new InvalidOperationException("GlobalGameManager.instance is null. Game may not be initialized.");
                }

                var sceneName = SceneManager.GetActiveScene().name;

                return new TitleMenuData
                {
                    currentScene = sceneName,
                    hasSaveData = globalGameManager.ExistSaveData(),
                    hasUnlimitData = globalGameManager.ExistUnlimitData(),
                    lastDay = globalGameManager.PreLoadData(),
                    currentLanguage = globalGameManager.GetCurrentLanguage(),
                    buildVersion = globalGameManager.BuildVer
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get title menu status: {ex.Message}", ex);
            }
        }

        public static bool IsOnTitleScreen()
        {
            return GameStateQueries.IsOnTitleScreen();
        }
    }
}
