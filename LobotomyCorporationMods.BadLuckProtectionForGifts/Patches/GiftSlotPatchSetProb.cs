// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CreatureInfo;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Constants;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(GiftSlot), nameof(GiftSlot.SetProb))]
    public static class GiftSlotPatchSetProb
    {
        /// <summary>Formats the gift chance display text with the agent name, showing both base and boosted probabilities.</summary>
        /// <param name="baseProb">The base probability without any mod bonus.</param>
        /// <param name="boostedProb">The probability with the agent's accumulated bonus applied.</param>
        /// <param name="agentName">The name of the most recent agent, or null if unavailable.</param>
        /// <param name="giftTitle">The localized gift title text.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <param name="showBaseChance">Whether to include the base chance in the display text.</param>
        /// <param name="chanceFormat">Localized format string for gift chance without base (expects {0}=title, {1}=agent, {2}=percent).</param>
        /// <param name="chanceWithBaseFormat">Localized format string for gift chance with base (expects {0}=title, {1}=agent, {2}=percent, {3}=base).</param>
        /// <returns>The formatted display text, or null if the text should not be modified.</returns>
        [CanBeNull]
        public static string FormatGiftChanceText(
            float baseProb,
            float boostedProb,
            [CanBeNull] string agentName,
            [NotNull] string giftTitle,
            int decimalPlaces,
            bool showBaseChance,
            [NotNull] string chanceFormat,
            [NotNull] string chanceWithBaseFormat
        )
        {
            if (string.IsNullOrEmpty(agentName))
            {
                return null;
            }

            var format = "F" + decimalPlaces;
            var boostedPercent = (boostedProb * 100f).ToString(
                format,
                CultureInfo.InvariantCulture
            );

            if (!showBaseChance)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    chanceFormat,
                    giftTitle,
                    agentName,
                    boostedPercent
                );
            }

            var basePercent = (baseProb * 100f).ToString(format, CultureInfo.InvariantCulture);

            return string.Format(
                CultureInfo.InvariantCulture,
                chanceWithBaseFormat,
                giftTitle,
                agentName,
                boostedPercent,
                basePercent
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

                var agentWorkTracker = Harmony_Patch.Instance.AgentWorkTracker;
                var config = Harmony_Patch.Instance.Config;
                var agentName = GetAgentName(__instance, agentWorkTracker);

                var boostedProb = GetBoostedProbability(__instance, prob, agentWorkTracker, config);

                var giftTitle = LocalizeTextDataModel.instance.GetText("Inventory_GiftTitle");
                var decimalPlaces = config.GiftChanceDecimalPlaces;
                var showBaseChance = config.ShowBaseChance;
                var chanceFormat = LocalizationIds.GiftChanceFormat.GetLocalized();
                var chanceWithBaseFormat = LocalizationIds.GiftChanceWithBaseFormat.GetLocalized();
                var formattedText = FormatGiftChanceText(
                    prob,
                    boostedProb,
                    agentName,
                    giftTitle,
                    decimalPlaces,
                    showBaseChance,
                    chanceFormat,
                    chanceWithBaseFormat
                );

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

        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        private static float GetBoostedProbability(
            [NotNull] GiftSlot giftSlot,
            float baseProb,
            [NotNull] IAgentWorkTracker agentWorkTracker,
            [NotNull] IBadLuckProtectionConfig config
        )
        {
            var info = giftSlot.Info;
            if (info.IsNull())
            {
                return baseProb;
            }

            var giftName = info.Name;
            if (string.IsNullOrEmpty(giftName))
            {
                return baseProb;
            }

            var agentId = agentWorkTracker.GetMostRecentAgentIdByGift(giftName);
            if (!agentId.HasValue)
            {
                return baseProb;
            }

            var workCount = agentWorkTracker.GetAgentWorkCountByGift(giftName, agentId.Value);
            var riskLevel = agentWorkTracker.GetRiskLevelByGift(giftName);
            var bonusPercentage = riskLevel.HasValue
                ? config.GetBonusPercentageForRiskLevel(riskLevel.Value)
                : 1.0f;

            var boostedProb = baseProb + workCount * bonusPercentage / 100f;

            return boostedProb > 1f ? 1f : boostedProb;
        }

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
