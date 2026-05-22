// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations.Effects
{
    /// <summary>Forces a random controllable agent into panic and drains their sanity.</summary>
    public sealed class RandomAgentPanicEffect : IEffectExecutor
    {
        private readonly IGameAdapter _gameAdapter;

        public RandomAgentPanicEffect(IGameAdapter gameAdapter)
        {
            ThrowHelper.ThrowIfNull(gameAdapter, nameof(gameAdapter));
            _gameAdapter = gameAdapter;
        }

        public string Slug => EffectSlugs.RandomAgentPanic;
        public float CooldownSeconds => 30f;
        public bool IsDanger => false;

        public string Execute(EffectDispatch dispatch)
        {
            if (!_gameAdapter.IsGameReady)
            {
                return ErrorTags.GameNotReady;
            }

            if (_gameAdapter.ControllableAgentCount == 0)
            {
                return ErrorTags.NoAgents;
            }

            return _gameAdapter.ForcePanicOnRandomControllableAgent()
                ? null
                : ErrorTags.ExecutionError;
        }

        public bool IsAvailableNow(out string reason)
        {
            if (!_gameAdapter.IsGameReady)
            {
                reason = ErrorTags.GameNotReady;
                return false;
            }

            if (_gameAdapter.ControllableAgentCount == 0)
            {
                reason = ErrorTags.NoAgents;
                return false;
            }

            reason = null;
            return true;
        }
    }
}
