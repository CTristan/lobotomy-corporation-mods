// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.JsonModels;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class InputHandler : IInputHandler
    {
        private const int AutoRefreshFrameInterval = 60;
        private const float AutoRefreshDurationSeconds = 60f;

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
            return elapsedTime < AutoRefreshDurationSeconds && frameCount % AutoRefreshFrameInterval == 0;
        }
    }
}
