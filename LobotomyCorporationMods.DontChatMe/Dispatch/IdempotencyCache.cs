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
    ///     LRU of terminal redemption outcomes. The server sequences dispatches strictly
    ///     (one in-flight per game), so the cache only needs to dedupe replays — the
    ///     server resending the in-flight head after a reconnect. On a hit, the dispatcher
    ///     re-emits the cached response so the server can advance idempotently; on a miss,
    ///     normal execution proceeds.
    ///     Not thread-safe; only the main-thread dispatcher touches it.
    /// </summary>
    public sealed class IdempotencyCache
    {
        private sealed class Entry
        {
            public LinkedListNode<string> OrderNode;
            public EffectResponse TerminalResponse;
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
        ///     Returns the cached terminal <see cref="EffectResponse" /> for the redemption,
        ///     or <c>null</c> when no terminal entry exists. Refreshes LRU position on hit.
        /// </summary>
        public EffectResponse LastTerminal(string redemptionId)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));

            Entry entry;
            if (!_entries.TryGetValue(redemptionId, out entry))
            {
                return null;
            }

            Touch(entry);
            return entry.TerminalResponse;
        }

        /// <summary>
        ///     Stores <paramref name="response" /> as the terminal outcome for this redemption.
        ///     Future <see cref="LastTerminal" /> calls will return the same object until LRU eviction.
        /// </summary>
        public void MarkTerminal(string redemptionId, EffectResponse response)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            ThrowHelper.ThrowIfNull(response, nameof(response));

            Entry entry;
            if (_entries.TryGetValue(redemptionId, out entry))
            {
                entry.TerminalResponse = response;
                Touch(entry);
                return;
            }

            AddNew(redemptionId, response);
        }

        /// <summary>
        ///     The redemption_id at the head of the LRU — i.e. the most-recently terminal-acked
        ///     dispatch. Sent in the <c>hello.last_seen_redemption_id</c> field so the server
        ///     can skip a head that has already been delivered in a prior session. Returns
        ///     <c>null</c> when the cache is empty.
        /// </summary>
        public string MostRecentTerminalRedemptionId =>
            _order.First == null ? null : _order.First.Value;

        private void AddNew(string redemptionId, EffectResponse response)
        {
            var node = _order.AddFirst(redemptionId);
            var entry = new Entry { OrderNode = node, TerminalResponse = response };
            _entries[redemptionId] = entry;

            if (_entries.Count > _capacity)
            {
                var oldest = _order.Last;
                _order.RemoveLast();
                _entries.Remove(oldest.Value);
            }
        }

        private void Touch(Entry entry)
        {
            _order.Remove(entry.OrderNode);
            _order.AddFirst(entry.OrderNode);
        }
    }
}
