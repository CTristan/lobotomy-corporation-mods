// SPDX-License-Identifier: MIT

#region

using System;
using Hemocode.DebugPanel.JsonModels;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IOverlayRenderer
    {
        void Draw(DiagnosticReport report, DebugPanelConfig config, Action refreshAction, Action generateLogAction);
    }
}
