// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(UseSkill), PrivateMethods.UseSkill.FinishWorkSuccessfully)]
    public static class UseSkillPatchFinishWorkSuccessfully
    {
        public static void PatchAfterFinishWorkSuccessfully([NotNull] this UseSkill instance,
            [NotNull] IAgentWorkTracker agentWorkTracker)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agentWorkTracker, nameof(agentWorkTracker));

            var giftName = instance.GetAbnormalityGiftName();

            // If the abnormality has no gift then there's nothing to track
            if (string.IsNullOrEmpty(giftName))
            {
                return;
            }

            var agentId = instance.GetAgentId();
            var numberOfSuccesses = instance.successCount;

            agentWorkTracker.IncrementAgentWorkCount(giftName, agentId, numberOfSuccesses);
        }

        /// <summary>Runs after an agent finishes working with an abnormality to increment their work count.</summary>
        /// <param name="__instance"></param>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] UseSkill __instance)
        {
            try
            {
                __instance.PatchAfterFinishWorkSuccessfully(Harmony_Patch.Instance.AgentWorkTracker);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
        // ReSharper enable InconsistentNaming
    }
}
