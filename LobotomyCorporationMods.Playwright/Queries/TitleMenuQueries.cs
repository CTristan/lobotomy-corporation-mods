// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Hemocode.Playwright.JsonModels;
using UnityEngine.SceneManagement;

#endregion

namespace Hemocode.Playwright.Queries
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

                var hasSaveData = globalGameManager.ExistSaveData();
                var checkpointDay = hasSaveData ? globalGameManager.LoadCheckPointDay() : 0;
                var hasCheckpointData = checkpointDay > 0;

                var availableSaveTypes = new List<string>();
                if (hasSaveData)
                {
                    availableSaveTypes.Add("lastday");
                    if (hasCheckpointData)
                    {
                        availableSaveTypes.Add("checkpoint");
                    }
                }

                return new TitleMenuData
                {
                    currentScene = sceneName,
                    hasSaveData = hasSaveData,
                    hasCheckpointData = hasCheckpointData,
                    hasUnlimitData = globalGameManager.ExistUnlimitData(),
                    lastDay = globalGameManager.PreLoadData(),
                    checkpointDay = checkpointDay,
                    currentLanguage = globalGameManager.GetCurrentLanguage(),
                    buildVersion = globalGameManager.BuildVer,
                    availableSaveTypes = availableSaveTypes
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
