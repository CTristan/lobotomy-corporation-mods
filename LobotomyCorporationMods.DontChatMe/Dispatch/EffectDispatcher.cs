// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Dispatch
{
    /// <summary>
    ///     Decides what to do with one inbound <see cref="EffectDispatch" /> and emits the reply.
    ///     Called from the main-thread <see cref="RequestPump" />, never from the transport thread.
    ///     Transient game-state errors (e.g. <c>game_not_ready</c>) are returned as
    ///     <see cref="EffectRetryReply" /> with a delay rather than an immediate refund; the chat-side
    ///     resubmits the same redemption id, and the cap of <see cref="MaxRetries" /> protects against
    ///     an indefinite loop if the state never resolves.
    /// </summary>
    public sealed class EffectDispatcher
    {
        /// <summary>
        ///     Default cap on how many <c>effect_retry</c> replies the dispatcher will issue for one
        ///     redemption before downgrading to a permanent <c>effect_failed</c>. Three is enough to
        ///     ride out a day-boundary animation or short meltdown without blocking the wire on a
        ///     permanently-stuck redemption.
        /// </summary>
        public const int MaxRetries = 3;

        /// <summary>How long the chat-side should wait before resubmitting a <c>game_not_ready</c>.</summary>
        public const int GameNotReadyRetryDelayMs = 5000;

        private readonly IDontChatMeConfig _config;
        private readonly Dictionary<string, IEffectExecutor> _executors;
        private readonly CooldownGate _cooldownGate;
        private readonly IdempotencyCache _idempotencyCache;
        private readonly Action<EffectReply> _sendReply;
        private readonly Action<EffectRetryReply> _sendRetry;
        private readonly ILogger _logger;
        private readonly Action<string> _onExecuted;

        public EffectDispatcher(
            IDontChatMeConfig config,
            IEnumerable<IEffectExecutor> executors,
            CooldownGate cooldownGate,
            IdempotencyCache idempotencyCache,
            Action<EffectReply> sendReply,
            Action<EffectRetryReply> sendRetry,
            ILogger logger,
            Action<string> onExecuted = null
        )
        {
            ThrowHelper.ThrowIfNull(config, nameof(config));
            ThrowHelper.ThrowIfNull(executors, nameof(executors));
            ThrowHelper.ThrowIfNull(cooldownGate, nameof(cooldownGate));
            ThrowHelper.ThrowIfNull(idempotencyCache, nameof(idempotencyCache));
            ThrowHelper.ThrowIfNull(sendReply, nameof(sendReply));
            ThrowHelper.ThrowIfNull(sendRetry, nameof(sendRetry));
            ThrowHelper.ThrowIfNull(logger, nameof(logger));

            _config = config;
            _cooldownGate = cooldownGate;
            _idempotencyCache = idempotencyCache;
            _sendReply = sendReply;
            _sendRetry = sendRetry;
            _logger = logger;
            _onExecuted = onExecuted;
            _executors = new Dictionary<string, IEffectExecutor>(StringComparer.Ordinal);
            foreach (var executor in executors)
            {
                if (executor == null || string.IsNullOrEmpty(executor.Slug))
                {
                    continue;
                }

                _executors[executor.Slug] = executor;
            }
        }

        public void Dispatch(EffectDispatch dispatch)
        {
            ThrowHelper.ThrowIfNull(dispatch, nameof(dispatch));

            if (!_idempotencyCache.TryEnter(dispatch.RedemptionId))
            {
                _sendReply(
                    EffectReply.Failed(dispatch.RedemptionId, ErrorTags.DuplicateRedemption)
                );
                return;
            }

            IEffectExecutor executor;
            if (!_executors.TryGetValue(dispatch.EffectSlug, out executor))
            {
                FailTerminal(dispatch.RedemptionId, ErrorTags.UnknownSlug);
                return;
            }

            if (executor.IsDanger && !_config.DangerEffectsEnabled)
            {
                FailTerminal(dispatch.RedemptionId, ErrorTags.DangerEffectsDisabled);
                return;
            }

            if (_cooldownGate.IsOnCooldown(executor.Slug, executor.CooldownSeconds))
            {
                FailTerminal(dispatch.RedemptionId, ErrorTags.Cooldown);
                return;
            }

            string error;
            try
            {
                error = executor.Execute(dispatch);
            }
#pragma warning disable CA1031 // The dispatcher is a per-frame stage on the main thread; one effect throwing an unexpected exception must not propagate up and kill the loop.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _logger.WriteException(ex);
                FailTerminal(dispatch.RedemptionId, ErrorTags.ExecutionError);
                return;
            }

            if (error == null)
            {
                _idempotencyCache.MarkTerminal(dispatch.RedemptionId);
                _cooldownGate.Mark(executor.Slug);
                _sendReply(EffectReply.Executed(dispatch.RedemptionId));
                if (_onExecuted != null)
                {
                    _onExecuted(executor.Slug);
                }

                return;
            }

            if (IsRetryable(error))
            {
                var retries = _idempotencyCache.RecordRetry(dispatch.RedemptionId);
                if (retries > MaxRetries)
                {
                    FailTerminal(dispatch.RedemptionId, error);
                    return;
                }

                _sendRetry(
                    new EffectRetryReply(dispatch.RedemptionId, GameNotReadyRetryDelayMs, error)
                );
                return;
            }

            FailTerminal(dispatch.RedemptionId, error);
        }

        private void FailTerminal(string redemptionId, string error)
        {
            _idempotencyCache.MarkTerminal(redemptionId);
            _sendReply(EffectReply.Failed(redemptionId, error));
        }

        private static bool IsRetryable(string error)
        {
            // Only game_not_ready is returned by an executor today; overloaded never reaches the
            // dispatcher (the pump itself rejects it before enqueue). The list lives here so future
            // retryable tags can join without touching call sites.
            return error == ErrorTags.GameNotReady;
        }
    }
}
