// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.DebugPanel.JsonModels;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IConfigProvider
    {
        DebugPanelConfig LoadConfig();

        void SaveConfig(DebugPanelConfig config);
    }
}
