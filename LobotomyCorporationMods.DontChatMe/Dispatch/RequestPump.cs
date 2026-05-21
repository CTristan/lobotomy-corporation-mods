// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Dispatch
{
    /// <summary>
    ///     Bounded thread-safe queue between the transport thread (producer) and the main-thread
    ///     dispatcher (consumer). net35 has no <c>ConcurrentQueue&lt;T&gt;</c>, so this is a
    ///     <c>Queue&lt;T&gt;</c> guarded by a private lock.
    /// </summary>
    public sealed class RequestPump
    {
        private readonly Queue<EffectDispatch> _queue = new Queue<EffectDispatch>();
        private readonly object _lock = new object();
        private readonly int _capacity;
        private readonly int _maxPerTick;
        private readonly Action<EffectDispatch> _drainCallback;

        public RequestPump(int capacity, int maxPerTick, Action<EffectDispatch> drainCallback)
        {
            ThrowHelper.ThrowIfNull(drainCallback, nameof(drainCallback));
            if (capacity < 1)
            {
                capacity = 1;
            }

            if (maxPerTick < 1)
            {
                maxPerTick = 1;
            }

            _capacity = capacity;
            _maxPerTick = maxPerTick;
            _drainCallback = drainCallback;
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        /// <summary>
        ///     Producer-side. Returns <c>false</c> when the queue is at <c>capacity</c>; the transport
        ///     replies <c>effect_failed { error: "overloaded" }</c> in that case.
        /// </summary>
        public bool TryEnqueue(EffectDispatch dispatch)
        {
            ThrowHelper.ThrowIfNull(dispatch, nameof(dispatch));
            lock (_lock)
            {
                if (_queue.Count >= _capacity)
                {
                    return false;
                }

                _queue.Enqueue(dispatch);
                return true;
            }
        }

        /// <summary>Consumer-side. Drains up to <c>maxPerTick</c> items and returns the count drained.</summary>
        public int Tick()
        {
            var drained = 0;
            while (drained < _maxPerTick)
            {
                EffectDispatch next;
                lock (_lock)
                {
                    if (_queue.Count == 0)
                    {
                        break;
                    }

                    next = _queue.Dequeue();
                }

                _drainCallback(next);
                drained++;
            }

            return drained;
        }
    }
}
