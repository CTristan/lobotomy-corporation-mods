// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Dispatch
{
    /// <summary>
    ///     One-slot mailbox between the transport thread (producer) and the main-thread
    ///     dispatcher (consumer). The server sequences dispatches strictly (one in-flight
    ///     per game), so a single-element slot is all we need. A producer arriving while
    ///     the slot is already full is a protocol violation by the server; we log a
    ///     warning and overwrite (newest dispatch wins — the server has clearly already
    ///     advanced past the prior one).
    /// </summary>
    public sealed class RequestPump
    {
        private readonly object _lock = new object();
        private readonly Action<EffectDispatch> _drainCallback;
        private readonly Action<EffectDispatch> _onOverwrite;
        private EffectDispatch _pending;

        public RequestPump(
            Action<EffectDispatch> drainCallback,
            Action<EffectDispatch> onOverwrite = null
        )
        {
            ThrowHelper.ThrowIfNull(drainCallback, nameof(drainCallback));
            _drainCallback = drainCallback;
            _onOverwrite = onOverwrite;
        }

        /// <summary>
        ///     <c>true</c> when a dispatch is sitting in the slot waiting for the next main-thread
        ///     <see cref="Tick" />.
        /// </summary>
        public bool HasPending
        {
            get
            {
                lock (_lock)
                {
                    return _pending != null;
                }
            }
        }

        /// <summary>
        ///     Producer-side. Stages a dispatch for the next main-thread tick. If a prior
        ///     dispatch is still sitting in the slot, the on-overwrite callback fires with
        ///     the displaced one (so it can be logged) and the new one wins.
        /// </summary>
        public void Enqueue(EffectDispatch dispatch)
        {
            ThrowHelper.ThrowIfNull(dispatch, nameof(dispatch));

            EffectDispatch displaced;
            lock (_lock)
            {
                displaced = _pending;
                _pending = dispatch;
            }

            if (displaced != null && _onOverwrite != null)
            {
                _onOverwrite(displaced);
            }
        }

        /// <summary>Consumer-side. Drains the slot if non-empty and runs the callback.</summary>
        public bool Tick()
        {
            EffectDispatch next;
            lock (_lock)
            {
                next = _pending;
                _pending = null;
            }

            if (next == null)
            {
                return false;
            }

            _drainCallback(next);
            return true;
        }
    }
}
