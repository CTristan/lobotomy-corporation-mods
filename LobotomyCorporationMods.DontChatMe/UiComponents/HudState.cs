// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     Thread-safe state holder shared between the producers (transport thread for connection state,
    ///     main thread for queued-count and last-effect updates, main thread for visibility toggle) and
    ///     the single consumer (the OnGUI renderer on the main thread).
    /// </summary>
    public sealed class HudState
    {
        private readonly object _lock = new object();
        private ConnectionState _state;
        private int _queuedCount;
        private string _lastEffectSlug;
        private bool _visible;

        public HudState(bool initiallyEnabled)
        {
            _state = initiallyEnabled ? ConnectionState.Disconnected : ConnectionState.Disabled;
            _queuedCount = 0;
            _lastEffectSlug = null;
            _visible = true;
        }

        public void SetConnectionState(ConnectionState state)
        {
            lock (_lock)
            {
                _state = state;
            }
        }

        public void SetQueuedCount(int count)
        {
            if (count < 0)
            {
                count = 0;
            }

            lock (_lock)
            {
                _queuedCount = count;
            }
        }

        public void RecordEffect(string slug)
        {
            lock (_lock)
            {
                _lastEffectSlug = slug;
            }
        }

        public void ToggleVisibility()
        {
            lock (_lock)
            {
                _visible = !_visible;
            }
        }

        public HudSnapshot Snapshot
        {
            get
            {
                lock (_lock)
                {
                    return new HudSnapshot(_state, _queuedCount, _lastEffectSlug, _visible);
                }
            }
        }
    }
}
