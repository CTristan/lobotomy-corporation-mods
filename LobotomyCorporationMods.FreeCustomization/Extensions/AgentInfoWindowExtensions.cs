// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

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
            agentInfoWindowAdapter.NotNull(nameof(agentInfoWindowAdapter));

            agentInfoWindowAdapter.OpenAppearanceWindow(agentInfoWindow);
        }
    }
}
