// SPDX-License-Identifier: MIT

#region

using DebugPanel.JsonModels;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IConfigProvider
    {
        DebugPanelConfig LoadConfig();

        void SaveConfig(DebugPanelConfig config);
    }
}
