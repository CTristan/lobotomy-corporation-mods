// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Dispatch
{
    /// <summary>
    ///     LRU set of recently-seen redemption ids. The dispatcher consults it so a duplicate
    ///     dispatch (e.g. a chat-side retry after a dropped reply) does not execute the effect twice.
    ///     Not thread-safe; only the main-thread dispatcher touches it.
    /// </summary>
    public sealed class IdempotencyCache
    {
        private readonly int _capacity;
        private readonly LinkedList<string> _order = new LinkedList<string>();
        private readonly Dictionary<string, LinkedListNode<string>> _index =
            new Dictionary<string, LinkedListNode<string>>();

        public IdempotencyCache(int capacity)
        {
            if (capacity < 1)
            {
                capacity = 1;
            }

            _capacity = capacity;
        }

        public int Count => _index.Count;

        /// <summary>
        ///     Records the id as seen and returns <c>true</c>; if the id was already in the cache,
        ///     refreshes its LRU position and returns <c>false</c>.
        /// </summary>
        public bool Add(string redemptionId)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));

            LinkedListNode<string> existing;
            if (_index.TryGetValue(redemptionId, out existing))
            {
                _order.Remove(existing);
                _order.AddFirst(existing);
                return false;
            }

            var node = _order.AddFirst(redemptionId);
            _index[redemptionId] = node;

            if (_index.Count > _capacity)
            {
                var oldest = _order.Last;
                _order.RemoveLast();
                _index.Remove(oldest.Value);
            }

            return true;
        }

        public bool Contains(string redemptionId) => _index.ContainsKey(redemptionId);
    }
}
