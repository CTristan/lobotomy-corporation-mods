// SPDX-License-Identifier: MIT

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IInputHandler
    {
        bool ShouldToggleOverlay();

        bool ShouldRefresh();

        bool ShouldAutoRefresh(float elapsedTime, int frameCount);
    }
}
