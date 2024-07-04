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
