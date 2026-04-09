// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CreatureInfo;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common.Attributes;
using LobotomyCorporation.Mods.Common.Constants;
using LobotomyCorporation.Mods.Common.Extensions;
using LobotomyCorporation.Mods.Common.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(GiftSlot), nameof(GiftSlot.SetProb))]
    public static class GiftSlotPatchSetProb
    {
        /// <summary>Formats the gift chance display text with the agent name.</summary>
        /// <param name="prob">The probability value passed to SetProb.</param>
        /// <param name="agentName">The name of the most recent agent, or null if unavailable.</param>
        /// <param name="giftTitle">The localized gift title text.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <returns>The formatted display text, or null if the text should not be modified.</returns>
        [CanBeNull]
        public static string FormatGiftChanceText(
            float prob,
            [CanBeNull] string agentName,
            [NotNull] string giftTitle,
            int decimalPlaces
        )
        {
            if (string.IsNullOrEmpty(agentName))
            {
                return null;
            }

            var percentValue = (prob * 100f).ToString(
                "F" + decimalPlaces,
                CultureInfo.InvariantCulture
            );

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} ({1} Next Chance:{2}%)",
                giftTitle,
                agentName,
                percentValue
            );
        }

        /// <summary>Runs after SetProb to update the gift chance display with the agent name.</summary>
        /// <param name="__instance">The GiftSlot instance.</param>
        /// <param name="prob">The probability value.</param>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] GiftSlot __instance, float prob)
        {
            try
            {
                ThrowHelper.ThrowIfNull(__instance, nameof(__instance));

                var agentName = GetAgentName(__instance, Harmony_Patch.Instance.AgentWorkTracker);

                var giftTitle = LocalizeTextDataModel.instance.GetText("Inventory_GiftTitle");
                var decimalPlaces = Harmony_Patch.Instance.Config.GiftChanceDecimalPlaces;
                var formattedText = FormatGiftChanceText(prob, agentName, giftTitle, decimalPlaces);

                if (formattedText.IsNotNull())
                {
                    __instance.Title.text = formattedText;
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        // ReSharper enable InconsistentNaming

        [CanBeNull]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        private static string GetAgentName(
            [NotNull] GiftSlot giftSlot,
            [NotNull] IAgentWorkTracker agentWorkTracker
        )
        {
            var info = giftSlot.Info;
            if (info.IsNull())
            {
                return null;
            }

            var giftName = info.Name;
            if (string.IsNullOrEmpty(giftName))
            {
                return null;
            }

            var agentId = agentWorkTracker.GetMostRecentAgentIdByGift(giftName);
            if (!agentId.HasValue)
            {
                return null;
            }

            var agentManager = AgentManager.instance;
            if (agentManager.IsNull())
            {
                return null;
            }

            var agent = agentManager.GetAgent(agentId.Value);

            return agent.IsNull() ? null : agent.GetUnitName();
        }
    }
}
