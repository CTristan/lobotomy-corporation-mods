// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches
{
    [HarmonyPatch(typeof(AgentSlot), nameof(AgentSlot.SetFilter))]
    public static class AgentSlotPatchSetFilter
    {
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix([NotNull] AgentSlot __instance, AgentState state)
        {
            try
            {
                var currentGameManager = GameManager.currentGameManager;
                __instance.PatchAfterSetFilter(state, currentGameManager, new BeautyBeastAnimAdapter(),
                    new ImageAdapter(), new TextAdapter(), new YggdrasilAnimAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
        // ReSharper enable InconsistentNaming

        public static void PatchAfterSetFilter([NotNull] this AgentSlot instance, AgentState state,
            [CanBeNull] GameManager currentGameManager, IBeautyBeastAnimAdapter beautyBeastAnimAdapter,
            [NotNull] IImageAdapter imageAdapter,
            [NotNull] ITextAdapter textAdapter, IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(imageAdapter, nameof(imageAdapter));
            Guard.Against.Null(textAdapter, nameof(textAdapter));

            // First load won't have a game manager yet, so just gracefully exit
            if (currentGameManager is null)
            {
                return;
            }

            // If we're not in Management phase then we don't need to check anything
            if (!currentGameManager.ManageStarted)
            {
                return;
            }

            // Some initial Command Window checks to make sure we're in the right state
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
            if (commandWindow is null || !commandWindow.IsAbnormalityWorkWindow() || state.IsUncontrollable())
            {
                return;
            }

            var agentWillDie =
                instance.CheckIfWorkWillKillAgent(commandWindow, beautyBeastAnimAdapter, yggdrasilAnimAdapter);

            if (!agentWillDie)
            {
                return;
            }

            imageAdapter.GameObject = instance.WorkFilterFill;
            imageAdapter.Color = commandWindow.DeadColor;

            textAdapter.GameObject = instance.WorkFilterText;
            textAdapter.Text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");

            instance.SetColor(commandWindow.DeadColor);
        }
    }
}
