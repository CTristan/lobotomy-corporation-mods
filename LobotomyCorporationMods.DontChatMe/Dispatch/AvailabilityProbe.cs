// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Threading;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Dispatch
{
    /// <summary>
    ///     Polls each effect executor's <see cref="IEffectExecutor.IsAvailableNow" /> at ~1 Hz and
    ///     pushes <c>effect_state</c> deltas to the chat-side server so its UI can grey out effects
    ///     that would currently fail. Also applies the <c>DangerEffectsEnabled</c> gate centrally
    ///     so each effect doesn't need to know about config policy.
    ///     <para>
    ///         Designed to be ticked from <c>GameManagerPatchUpdate</c>'s Postfix on the Unity main
    ///         thread. <see cref="RequestSnapshot" /> is safe to call from any thread — the transport
    ///         calls it from a socket worker thread when the connection reaches Connected.
    ///     </para>
    /// </summary>
    public sealed class AvailabilityProbe
    {
        private const float TickIntervalSeconds = 1f;

        private readonly IEffectExecutor[] _executors;
        private readonly IDontChatMeConfig _config;
        private readonly Action<EffectStateReply> _send;
        private readonly Func<float> _now;
        private readonly Dictionary<string, EffectStateReply> _lastSent = new Dictionary<
            string,
            EffectStateReply
        >(StringComparer.Ordinal);

        private float _nextTickAt;
        private int _snapshotRequested;

        public AvailabilityProbe(
            IEnumerable<IEffectExecutor> executors,
            IDontChatMeConfig config,
            Action<EffectStateReply> send,
            Func<float> now
        )
        {
            ThrowHelper.ThrowIfNull(executors, nameof(executors));
            ThrowHelper.ThrowIfNull(config, nameof(config));
            ThrowHelper.ThrowIfNull(send, nameof(send));
            ThrowHelper.ThrowIfNull(now, nameof(now));

            var list = new List<IEffectExecutor>();
            foreach (var executor in executors)
            {
                if (executor == null || string.IsNullOrEmpty(executor.Slug))
                {
                    continue;
                }

                list.Add(executor);
            }

            _executors = list.ToArray();
            _config = config;
            _send = send;
            _now = now;
        }

        /// <summary>
        ///     Forces the next <see cref="Tick" /> to emit every slug's current state regardless of
        ///     change. Wire this to the transport's transition into the Connected state so the
        ///     chat-side starts in sync.
        /// </summary>
        public void RequestSnapshot()
        {
            Interlocked.Exchange(ref _snapshotRequested, 1);
        }

        /// <summary>
        ///     Evaluates each executor and emits an <c>effect_state</c> frame when the state has
        ///     changed (or always, on a requested snapshot). Throttles to ~1 Hz so per-frame ticks
        ///     don't spam the wire.
        /// </summary>
        public void Tick()
        {
            var snapshot = Interlocked.Exchange(ref _snapshotRequested, 0) == 1;
            var now = _now();
            if (!snapshot && now < _nextTickAt)
            {
                return;
            }

            _nextTickAt = now + TickIntervalSeconds;

            foreach (var executor in _executors)
            {
                EffectStateReply current = Evaluate(executor);

                EffectStateReply previous;
                var known = _lastSent.TryGetValue(executor.Slug, out previous);
                if (!snapshot && known && previous.Equals(current))
                {
                    continue;
                }

                _send(current);
                _lastSent[executor.Slug] = current;
            }
        }

        private EffectStateReply Evaluate(IEffectExecutor executor)
        {
            if (executor.IsDanger && !_config.DangerEffectsEnabled)
            {
                return new EffectStateReply(
                    executor.Slug,
                    selectable: false,
                    reason: ErrorTags.DangerEffectsDisabled
                );
            }

            string reason;
            var available = executor.IsAvailableNow(out reason);
            return new EffectStateReply(executor.Slug, available, available ? null : reason);
        }
    }
}
