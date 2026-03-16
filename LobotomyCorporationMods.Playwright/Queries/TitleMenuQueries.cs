// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Playwright.JsonModels;
using UnityEngine.SceneManagement;

#endregion

namespace LobotomyCorporationMods.Playwright.Queries
{
    /// <summary>
    /// Queries for title menu state and operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class TitleMenuQueries
    {
        public static TitleMenuData GetTitleMenuStatus()
        {
            try
            {
                var globalGameManager = GlobalGameManager.instance ?? throw new InvalidOperationException("GlobalGameManager.instance is null. Game may not be initialized.");
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
