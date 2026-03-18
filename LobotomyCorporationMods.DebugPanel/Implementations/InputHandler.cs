// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.DebugPanel.JsonModels;
using UnityEngine;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class InputHandler : IInputHandler
    {
        private const int AutoRefreshFrameInterval = 60;

        private readonly KeyCode _toggleKey;
        private readonly KeyCode _refreshKey;

        public InputHandler(DebugPanelConfig config)
        {
            _toggleKey = KeyCodeParser.Parse(config?.OverlayToggleKey);
            _refreshKey = KeyCodeParser.Parse(config?.RefreshKey);
        }

        public bool ShouldToggleOverlay()
        {
            return Input.GetKeyDown(_toggleKey);
        }

        public bool ShouldRefresh()
        {
            return Input.GetKeyDown(_refreshKey);
        }

        public bool ShouldAutoRefresh(float elapsedTime, int frameCount)
        {
            return frameCount % AutoRefreshFrameInterval == 0;
        }
    }
}
