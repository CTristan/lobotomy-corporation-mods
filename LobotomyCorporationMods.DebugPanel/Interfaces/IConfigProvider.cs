// SPDX-License-Identifier: MIT

#region

using Hemocode.DebugPanel.JsonModels;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IConfigProvider
    {
        DebugPanelConfig LoadConfig();

        void SaveConfig(DebugPanelConfig config);
    }
}
