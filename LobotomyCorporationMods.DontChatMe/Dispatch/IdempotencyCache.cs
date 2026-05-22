// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Dispatch
{
    /// <summary>
    ///     LRU cache that tracks per-redemption lifecycle so the dispatcher can (a) refuse a duplicate
    ///     dispatch that already produced a terminal reply, and (b) allow the same redemption_id to be
    ///     resubmitted after an <c>effect_retry</c> reply. A redemption is "terminal" once it has
    ///     produced an <c>effect_executed</c> or non-retryable <c>effect_failed</c>; until then,
    ///     repeated <see cref="TryEnter" /> calls return <c>true</c> so retries can be processed.
    ///     Not thread-safe; only the main-thread dispatcher touches it.
    /// </summary>
    public sealed class IdempotencyCache
    {
        private sealed class Entry
        {
            public LinkedListNode<string> OrderNode;
            public int RetryCount;
            public bool Terminal;
        }

        private readonly int _capacity;
        private readonly LinkedList<string> _order = new LinkedList<string>();
        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(
            StringComparer.Ordinal
        );

        public IdempotencyCache(int capacity)
        {
            if (capacity < 1)
            {
                capacity = 1;
            }

            _capacity = capacity;
        }

        public int Count => _entries.Count;

        /// <summary>
        ///     Returns <c>true</c> if the dispatcher is allowed to process this redemption (either
        ///     never seen, or seen but only produced retries). Returns <c>false</c> if the redemption
        ///     already produced a terminal reply — in that case the caller should emit
        ///     <c>duplicate_redemption</c>. Refreshes LRU position either way.
        /// </summary>
        public bool TryEnter(string redemptionId)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));

            Entry existing;
            if (_entries.TryGetValue(redemptionId, out existing))
            {
                Touch(existing);
                return !existing.Terminal;
            }

            AddNew(redemptionId);
            return true;
        }

        /// <summary>
        ///     Increments and returns the retry counter for this redemption. The caller compares
        ///     the result against its retry cap to decide whether to issue another <c>effect_retry</c>
        ///     or downgrade to a permanent <c>effect_failed</c>.
        /// </summary>
        public int RecordRetry(string redemptionId)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));

            Entry entry;
            if (!_entries.TryGetValue(redemptionId, out entry))
            {
                entry = AddNew(redemptionId);
            }
            else
            {
                Touch(entry);
            }

            entry.RetryCount++;
            return entry.RetryCount;
        }

        /// <summary>
        ///     Marks the redemption as terminal — future <see cref="TryEnter" /> calls will return
        ///     <c>false</c> for this id until LRU eviction.
        /// </summary>
        public void MarkTerminal(string redemptionId)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));

            Entry entry;
            if (!_entries.TryGetValue(redemptionId, out entry))
            {
                entry = AddNew(redemptionId);
            }
            else
            {
                Touch(entry);
            }

            entry.Terminal = true;
        }

        /// <summary>Diagnostic: has this id ever been seen by the cache?</summary>
        public bool Contains(string redemptionId) => _entries.ContainsKey(redemptionId);

        /// <summary>Diagnostic: is this id in a terminal state?</summary>
        public bool IsTerminal(string redemptionId)
        {
            Entry entry;
            return _entries.TryGetValue(redemptionId, out entry) && entry.Terminal;
        }

        private Entry AddNew(string redemptionId)
        {
            var node = _order.AddFirst(redemptionId);
            var entry = new Entry { OrderNode = node };
            _entries[redemptionId] = entry;

            if (_entries.Count > _capacity)
            {
                var oldest = _order.Last;
                _order.RemoveLast();
                _entries.Remove(oldest.Value);
            }

            return entry;
        }

        private void Touch(Entry entry)
        {
            _order.Remove(entry.OrderNode);
            _order.AddFirst(entry.OrderNode);
        }
    }
}
