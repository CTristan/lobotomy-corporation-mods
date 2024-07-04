// SPDX-License-Identifier: MIT

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class ManagementUiFacade
    {
        /// <summary>
        ///     Determines whether the current CommandWindow is an abnormality work window. An abnormality work window is a CommandWindow in the Management phase with a non-null rwbpType
        ///     in the CurrentSkill property.
        /// </summary>
        /// <param name="commandWindow">The current CommandWindow to check.</param>
        /// <returns><c>true</c> if the current CommandWindow is an abnormality work window, otherwise <c>false</c>.</returns>
        public static bool IsAbnormalityWorkWindow([NotNull] this CommandWindow.CommandWindow commandWindow)
        {
            Guard.Against.Null(commandWindow, nameof(commandWindow));

            // Validation checks to confirm we have everything we need
            var isAbnormalityWorkWindow = commandWindow.CurrentSkill.IsNotNull() && commandWindow.CurrentWindowType == CommandType.Management;

            return isAbnormalityWorkWindow;
        }

        public static void UpdateAgentSlot([NotNull] this AgentSlot agentSlot,
            Color slotColor,
            string slotText,
            [CanBeNull] IImageTestAdapter imageTestAdapter = null,
            [CanBeNull] ITextTestAdapter textTestAdapter = null)
        {
            Guard.Against.Null(agentSlot, nameof(agentSlot));

            imageTestAdapter = imageTestAdapter.EnsureNotNullWithMethod(() => new ImageTestAdapter(agentSlot.WorkFilterFill));
            textTestAdapter = textTestAdapter.EnsureNotNullWithMethod(() => new TextTestAdapter(agentSlot.WorkFilterText));

            imageTestAdapter.Color = slotColor;
            textTestAdapter.Text = slotText;
            agentSlot.SetColor(slotColor);
        }
    }
}
