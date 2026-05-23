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
                return StandardErrors.GameStateBlocked;
            }

            if (_gameAdapter.ControllableAgentCount == 0)
            {
                return StandardErrors.EffectUnavailableNow;
            }

            return _gameAdapter.ForcePanicOnRandomControllableAgent()
                ? null
                : StandardErrors.ModInternalError;
        }

        public bool IsAvailableNow(out string reason)
        {
            if (!_gameAdapter.IsGameReady)
            {
                reason = StandardErrors.GameStateBlocked;
                return false;
            }

            if (_gameAdapter.ControllableAgentCount == 0)
            {
                reason = StandardErrors.EffectUnavailableNow;
                return false;
            }

            reason = null;
            return true;
        }
    }
}
