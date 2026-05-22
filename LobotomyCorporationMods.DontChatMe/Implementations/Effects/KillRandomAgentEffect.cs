// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations.Effects
{
    /// <summary>Kills a random living agent. The agent keeps their equipment for the post-mortem.</summary>
    public sealed class KillRandomAgentEffect : IEffectExecutor
    {
        private readonly IGameAdapter _gameAdapter;

        public KillRandomAgentEffect(IGameAdapter gameAdapter)
        {
            ThrowHelper.ThrowIfNull(gameAdapter, nameof(gameAdapter));
            _gameAdapter = gameAdapter;
        }

        public string Slug => EffectSlugs.KillRandomAgent;
        public float CooldownSeconds => 60f;
        public bool IsDanger => false;

        public string Execute(EffectDispatch dispatch)
        {
            if (!_gameAdapter.IsGameReady)
            {
                return ErrorTags.GameNotReady;
            }

            if (_gameAdapter.LivingAgentCount == 0)
            {
                return ErrorTags.NoAgents;
            }

            return _gameAdapter.KillRandomLivingAgent() ? null : ErrorTags.ExecutionError;
        }

        public bool IsAvailableNow(out string reason)
        {
            if (!_gameAdapter.IsGameReady)
            {
                reason = ErrorTags.GameNotReady;
                return false;
            }

            if (_gameAdapter.LivingAgentCount == 0)
            {
                reason = ErrorTags.NoAgents;
                return false;
            }

            reason = null;
            return true;
        }
    }
}
