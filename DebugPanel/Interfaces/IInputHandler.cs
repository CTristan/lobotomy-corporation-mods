// SPDX-License-Identifier: MIT

namespace DebugPanel.Interfaces
{
    public interface IInputHandler
    {
        bool ShouldToggleOverlay();

        bool ShouldRefresh();

        bool ShouldAutoRefresh(float elapsedTime, int frameCount);
    }
}
