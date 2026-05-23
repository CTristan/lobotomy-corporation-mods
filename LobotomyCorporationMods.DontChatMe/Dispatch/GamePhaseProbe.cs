// SPDX-License-Identifier: MIT

#region

using System;
using System.Threading;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Dispatch
{
    /// <summary>
    ///     Polls <see cref="IGameAdapter.ReadPhase" /> and pushes a <c>game_state</c> frame to the
    ///     chat-side server whenever the phase changes, plus a slow heartbeat to keep the chat-side
    ///     resynchronized after a hiccup. Mirrors the <c>AvailabilityProbe</c> shape: ticked from
    ///     <c>GameManagerPatchUpdate</c>'s Postfix on the Unity main thread, and exposes
    ///     <see cref="RequestSnapshot" /> for the transport's transition into Connected.
    /// </summary>
    public sealed class GamePhaseProbe
    {
        /// <summary>Maximum gap between heartbeats when the phase has not changed.</summary>
        public const float HeartbeatIntervalSeconds = 5f;

        private readonly IGameAdapter _adapter;
        private readonly Action<GameStateReply> _send;
        private readonly Func<float> _now;

        private string _lastSentPhase;
        private float _nextHeartbeatAt;
        private int _snapshotRequested;

        public GamePhaseProbe(IGameAdapter adapter, Action<GameStateReply> send, Func<float> now)
        {
            ThrowHelper.ThrowIfNull(adapter, nameof(adapter));
            ThrowHelper.ThrowIfNull(send, nameof(send));
            ThrowHelper.ThrowIfNull(now, nameof(now));
            _adapter = adapter;
            _send = send;
            _now = now;
        }

        /// <summary>
        ///     Forces the next <see cref="Tick" /> to emit the current phase regardless of whether
        ///     it has changed. Wired to the transport's transition into Connected so the chat-side
        ///     starts in sync.
        /// </summary>
        public void RequestSnapshot()
        {
            Interlocked.Exchange(ref _snapshotRequested, 1);
        }

        /// <summary>
        ///     Emits a <c>game_state</c> frame when the phase has changed since last emit, when a
        ///     snapshot has been requested, or when the heartbeat interval has elapsed.
        /// </summary>
        public void Tick()
        {
            var snapshot = Interlocked.Exchange(ref _snapshotRequested, 0) == 1;
            var phase = _adapter.ReadPhase();
            if (phase == null)
            {
                return;
            }

            var now = _now();
            var changed = phase != _lastSentPhase;
            var heartbeatDue = _lastSentPhase != null && now >= _nextHeartbeatAt;

            if (!snapshot && !changed && !heartbeatDue)
            {
                return;
            }

            _send(new GameStateReply(phase));
            _lastSentPhase = phase;
            _nextHeartbeatAt = now + HeartbeatIntervalSeconds;
        }
    }
}
