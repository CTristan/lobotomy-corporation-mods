// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.DebugPanel.JsonModels;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IOverlayRenderer
    {
        void Draw(DiagnosticReport report, DebugPanelConfig config, Action refreshAction, Action generateLogAction);
    }
}
