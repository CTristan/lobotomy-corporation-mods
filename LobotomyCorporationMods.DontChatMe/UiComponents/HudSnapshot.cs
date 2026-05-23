// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     Immutable point-in-time view of the HUD's data. Returned by
    ///     <see cref="HudState.Snapshot" /> so the OnGUI renderer reads a consistent set of values
    ///     even while producers update them on other threads.
    /// </summary>
    public sealed class HudSnapshot
    {
        public HudSnapshot(
            ConnectionState state,
            int queuedCount,
            string lastEffectSlug,
            bool visible
        )
        {
            State = state;
            QueuedCount = queuedCount;
            LastEffectSlug = lastEffectSlug;
            Visible = visible;
        }

        public ConnectionState State { get; }
        public int QueuedCount { get; }
        public string LastEffectSlug { get; }
        public bool Visible { get; }
    }
}
