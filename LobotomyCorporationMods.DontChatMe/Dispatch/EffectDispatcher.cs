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
    ///     Decides what to do with one inbound <see cref="EffectDispatch" /> and emits the
    ///     single <see cref="EffectResponse" /> per dispatch. Runs on the main thread (driven
    ///     by <see cref="RequestPump" />), never on the transport thread.
    ///     Transient executor errors (<c>game_state_blocked</c>, <c>effect_unavailable_now</c>)
    ///     map to <see cref="ResponseStatuses.Retry" />; the server's per-game queue holds the
    ///     head and resends after <see cref="RetryHoldMs" />, governed by the 3-attempt budget.
    /// </summary>
    public sealed class EffectDispatcher
    {
        /// <summary>Default <c>retry_after_ms</c> we ask the server's queue to wait before resending.</summary>
        public const int RetryHoldMs = 5000;

        private readonly IDontChatMeConfig _config;
        private readonly Dictionary<string, IEffectExecutor> _executors;
        private readonly CooldownGate _cooldownGate;
        private readonly IdempotencyCache _idempotencyCache;
        private readonly Action<EffectResponse> _sendResponse;
        private readonly ILogger _logger;
        private readonly Action<string> _onExecuted;

        public EffectDispatcher(
            IDontChatMeConfig config,
            IEnumerable<IEffectExecutor> executors,
            CooldownGate cooldownGate,
            IdempotencyCache idempotencyCache,
            Action<EffectResponse> sendResponse,
            ILogger logger,
            Action<string> onExecuted = null
        )
        {
            ThrowHelper.ThrowIfNull(config, nameof(config));
            ThrowHelper.ThrowIfNull(executors, nameof(executors));
            ThrowHelper.ThrowIfNull(cooldownGate, nameof(cooldownGate));
            ThrowHelper.ThrowIfNull(idempotencyCache, nameof(idempotencyCache));
            ThrowHelper.ThrowIfNull(sendResponse, nameof(sendResponse));
            ThrowHelper.ThrowIfNull(logger, nameof(logger));

            _config = config;
            _cooldownGate = cooldownGate;
            _idempotencyCache = idempotencyCache;
            _sendResponse = sendResponse;
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

            var cached = _idempotencyCache.LastTerminal(dispatch.RedemptionId);
            if (cached != null)
            {
                // A replay of a redemption we already terminated: re-send the cached outcome
                // so the server can advance idempotently.
                _sendResponse(cached);
                return;
            }

            IEffectExecutor executor;
            if (!_executors.TryGetValue(dispatch.EffectSlug, out executor))
            {
                SendTerminal(
                    dispatch.RedemptionId,
                    EffectResponse.Failure(dispatch.RedemptionId, StandardErrors.EffectUnknown)
                );
                return;
            }

            if (executor.IsDanger && !_config.DangerEffectsEnabled)
            {
                SendTerminal(
                    dispatch.RedemptionId,
                    EffectResponse.Failure(dispatch.RedemptionId, StandardErrors.EffectDisabled)
                );
                return;
            }

            if (_cooldownGate.IsOnCooldown(executor.Slug, executor.CooldownSeconds))
            {
                SendTerminal(
                    dispatch.RedemptionId,
                    EffectResponse.Failure(dispatch.RedemptionId, StandardErrors.Cooldown)
                );
                return;
            }

            string error;
            try
            {
                error = executor.Execute(dispatch);
            }
#pragma warning disable CA1031 // The dispatcher is a per-frame stage on the main thread; one effect throwing must not propagate up and kill the loop.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _logger.WriteException(ex);
                SendTerminal(
                    dispatch.RedemptionId,
                    EffectResponse.Failure(dispatch.RedemptionId, StandardErrors.ModInternalError)
                );
                return;
            }

            if (error == null)
            {
                _cooldownGate.Mark(executor.Slug);
                SendTerminal(dispatch.RedemptionId, EffectResponse.Success(dispatch.RedemptionId));
                if (_onExecuted != null)
                {
                    _onExecuted(executor.Slug);
                }

                return;
            }

            if (IsRetryable(error))
            {
                // Don't cache: the server's queue will resend the same redemption_id and we want
                // the next attempt to re-evaluate the executor's preconditions.
                _sendResponse(EffectResponse.Retry(dispatch.RedemptionId, error, RetryHoldMs));
                return;
            }

            SendTerminal(
                dispatch.RedemptionId,
                EffectResponse.Failure(dispatch.RedemptionId, error)
            );
        }

        private void SendTerminal(string redemptionId, EffectResponse response)
        {
            _idempotencyCache.MarkTerminal(redemptionId, response);
            _sendResponse(response);
        }

        private static bool IsRetryable(string error)
        {
            // Transient blocks: the server should resend and re-evaluate preconditions. Game-wide
            // phase gating (game_state_blocked) and per-effect availability blocks
            // (effect_unavailable_now) are both transient by definition; the server's queue and
            // 3-attempt budget bound how long we hold a head.
            return error == StandardErrors.GameStateBlocked
                || error == StandardErrors.EffectUnavailableNow;
        }
    }
}
