// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.FreeCustomization.Extensions
{
    public static class AgentInfoWindowExtensions
    {
        internal static void OpenAppearanceWindow(this AgentInfoWindow agentInfoWindow)
        {
            OpenAppearanceWindow(agentInfoWindow, new AgentInfoWindowAdapter());
        }

        public static void OpenAppearanceWindow(this AgentInfoWindow agentInfoWindow, [NotNull] IAgentInfoWindowAdapter agentInfoWindowAdapter)
        {
            Guard.Against.Null(agentInfoWindowAdapter, nameof(agentInfoWindowAdapter));

            agentInfoWindowAdapter.OpenAppearanceWindow(agentInfoWindow);
        }
    }
}
