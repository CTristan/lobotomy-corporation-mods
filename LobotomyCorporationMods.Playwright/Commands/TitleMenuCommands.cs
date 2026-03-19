// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Hemocode.Playwright.JsonModels;
using Hemocode.Playwright.Queries;

// Use alias to avoid conflicts with Unity's Debug
using TcpServer = Hemocode.Playwright.Server.TcpServer;

#endregion

namespace Hemocode.Playwright.Commands
{
    /// <summary>
    /// Title menu command handlers — navigate from title screen into gameplay.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class TitleMenuCommands
    {
        /// <summary>
        /// Continue from last save. Loads global data then last-day save and transitions to gameplay.
        /// </summary>
        public static Response HandleContinue(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!TitleMenuQueries.IsOnTitleScreen())
            {
                return Response.CreateError(request.Id, "Not on title screen", "NOT_ON_TITLE_SCREEN");
            }

            try
            {
                var titleScript = NewTitleScript.instance;
                if (titleScript == null)
                {
                    return Response.CreateError(request.Id, "NewTitleScript not available", "NOT_AVAILABLE");
                }

                if (!GlobalGameManager.instance.ExistSaveData())
                {
                    return Response.CreateError(request.Id, "No save data exists", "NO_SAVE_DATA");
                }

                // Call the same method the UI button triggers, which handles
                // animation setup and delegates to ClickAfterContinue via OnMoveEnd
                titleScript.OnClickContinue();

                return Response.CreateSuccess(request.Id, new { result = "continue_started" });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleContinue");
                return Response.CreateError(request.Id, $"Failed to continue: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Start a new game (story mode). Warning: this overwrites existing progress.
        /// </summary>
        public static Response HandleNewGame(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!TitleMenuQueries.IsOnTitleScreen())
            {
                return Response.CreateError(request.Id, "Not on title screen", "NOT_ON_TITLE_SCREEN");
            }

            try
            {
                var globalGameManager = GlobalGameManager.instance;
                if (globalGameManager == null)
                {
                    return Response.CreateError(request.Id, "GlobalGameManager not available", "NOT_AVAILABLE");
                }

                globalGameManager.isPlayingTutorial = true;
                globalGameManager.LoadGlobalData();
                CreatureGenerate.CreatureGenerateInfoManager.Instance.Init();
                globalGameManager.InitStoryMode();
                PlayerModel.instance.InitAddingCreatures();
                NewTitleScript.instance.LoadStoryMode();

                TcpServer.LogDebug("[LobotomyPlaywright] Title menu: new game started");
                return Response.CreateSuccess(request.Id, new { result = "new_game_started" });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleNewGame");
                return Response.CreateError(request.Id, $"Failed to start new game: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Load a specific save type (lastday or checkpoint).
        /// </summary>
        public static Response HandleLoadSave(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!TitleMenuQueries.IsOnTitleScreen())
            {
                return Response.CreateError(request.Id, "Not on title screen", "NOT_ON_TITLE_SCREEN");
            }

            var paramsObj = GetParams(request);
            if (paramsObj == null || string.IsNullOrEmpty(paramsObj.saveType))
            {
                return Response.CreateError(request.Id, "Missing saveType parameter (lastday or checkpoint)", "INVALID_PARAMS");
            }

            var saveTypeLower = paramsObj.saveType.ToLowerInvariant();
            SaveType saveType;
            switch (saveTypeLower)
            {
                case "lastday":
                    saveType = SaveType.LASTDAY;
                    break;
                case "checkpoint":
                    saveType = SaveType.CHECK_POINT;
                    break;
                default:
                    return Response.CreateError(request.Id, $"Invalid saveType: {paramsObj.saveType}. Must be 'lastday' or 'checkpoint'", "INVALID_SAVE_TYPE");
            }

            try
            {
                var globalGameManager = GlobalGameManager.instance;
                if (globalGameManager == null)
                {
                    return Response.CreateError(request.Id, "GlobalGameManager not available", "NOT_AVAILABLE");
                }

                if (!globalGameManager.ExistSaveData())
                {
                    return Response.CreateError(request.Id, "No save data exists", "NO_SAVE_DATA");
                }

                globalGameManager.LoadGlobalData();
                globalGameManager.LoadData(saveType);

                if (globalGameManager.saveState == "story")
                {
                    NewTitleScript.instance.LoadStoryMode();
                }
                else
                {
                    globalGameManager.lastLoaded = true;
                    NewTitleScript.instance.LoadMainGame();
                }

                TcpServer.LogDebug($"[LobotomyPlaywright] Title menu: loaded save type {saveTypeLower}");
                return Response.CreateSuccess(request.Id, new { result = "save_loaded", saveType = saveTypeLower });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleLoadSave");
                return Response.CreateError(request.Id, $"Failed to load save: {ex.Message}", "COMMAND_ERROR");
            }
        }

        /// <summary>
        /// Start the day from the deployment screen (clicks "Begin Management").
        /// Uses SendMessage to invoke DeployUI.OnClickStartGame(), following the same
        /// code path as a real UI button click. The deployment screen must be fully
        /// initialized before calling this — allow a few seconds after scene load.
        /// </summary>
        public static Response HandleStartDay(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                var deployUI = DeployUI.instance;
                if (deployUI == null)
                {
                    return Response.CreateError(request.Id, "DeployUI not available — game may not be on the deployment screen", "NOT_AVAILABLE");
                }

                if (deployUI.IsGameStarted)
                {
                    return Response.CreateError(request.Id, "Day has already started", "ALREADY_STARTED");
                }

                if (GameManager.currentGameManager == null)
                {
                    return Response.CreateError(request.Id, "GameManager not available", "NOT_AVAILABLE");
                }

                // Simulate the "Begin Management" button click via SendMessage,
                // which follows the same code path as a real UI click
                deployUI.SendMessage("OnClickStartGame");

                return Response.CreateSuccess(request.Id, new { result = "day_started" });
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "HandleStartDay");
                return Response.CreateError(request.Id, $"Failed to start day: {ex.Message}", "COMMAND_ERROR");
            }
        }

        private static LoadSaveParams GetParams(Request request)
        {
            if (request.Params == null)
            {
                return null;
            }

            try
            {
                var paramsObj = new LoadSaveParams();
                if (request.Params.ContainsKey("saveType"))
                {
                    var value = request.Params["saveType"];
                    if (value != null)
                    {
                        paramsObj.saveType = value.ToString();
                    }
                }

                return paramsObj;
            }
            catch
            {
                return null;
            }
        }
    }
}
