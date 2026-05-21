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
    /// </summary>
    public sealed class EffectDispatcher
    {
        private readonly IDontChatMeConfig _config;
        private readonly Dictionary<string, IEffectExecutor> _executors;
        private readonly CooldownGate _cooldownGate;
        private readonly IdempotencyCache _idempotencyCache;
        private readonly Action<EffectReply> _sendReply;
        private readonly ILogger _logger;

        public EffectDispatcher(
            IDontChatMeConfig config,
            IEnumerable<IEffectExecutor> executors,
            CooldownGate cooldownGate,
            IdempotencyCache idempotencyCache,
            Action<EffectReply> sendReply,
            ILogger logger
        )
        {
            ThrowHelper.ThrowIfNull(config, nameof(config));
            ThrowHelper.ThrowIfNull(executors, nameof(executors));
            ThrowHelper.ThrowIfNull(cooldownGate, nameof(cooldownGate));
            ThrowHelper.ThrowIfNull(idempotencyCache, nameof(idempotencyCache));
            ThrowHelper.ThrowIfNull(sendReply, nameof(sendReply));
            ThrowHelper.ThrowIfNull(logger, nameof(logger));

            _config = config;
            _cooldownGate = cooldownGate;
            _idempotencyCache = idempotencyCache;
            _sendReply = sendReply;
            _logger = logger;
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

            if (!_idempotencyCache.Add(dispatch.RedemptionId))
            {
                _sendReply(
                    EffectReply.Failed(dispatch.RedemptionId, ErrorTags.DuplicateRedemption)
                );
                return;
            }

            IEffectExecutor executor;
            if (!_executors.TryGetValue(dispatch.EffectSlug, out executor))
            {
                _sendReply(EffectReply.Failed(dispatch.RedemptionId, ErrorTags.UnknownSlug));
                return;
            }

            if (executor.IsDanger && !_config.DangerEffectsEnabled)
            {
                _sendReply(
                    EffectReply.Failed(dispatch.RedemptionId, ErrorTags.DangerEffectsDisabled)
                );
                return;
            }

            if (_cooldownGate.IsOnCooldown(executor.Slug, executor.CooldownSeconds))
            {
                _sendReply(EffectReply.Failed(dispatch.RedemptionId, ErrorTags.Cooldown));
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
                _sendReply(EffectReply.Failed(dispatch.RedemptionId, ErrorTags.ExecutionError));
                return;
            }

            if (error == null)
            {
                _cooldownGate.Mark(executor.Slug);
                _sendReply(EffectReply.Executed(dispatch.RedemptionId));
            }
            else
            {
                _sendReply(EffectReply.Failed(dispatch.RedemptionId, error));
            }
        }
    }
}
