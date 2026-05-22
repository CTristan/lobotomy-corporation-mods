// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations.Effects
{
    /// <summary>Triggers a meltdown on one random abnormality.</summary>
    public sealed class RandomMeltdownEffect : IEffectExecutor
    {
        private readonly IGameAdapter _gameAdapter;

        public RandomMeltdownEffect(IGameAdapter gameAdapter)
        {
            ThrowHelper.ThrowIfNull(gameAdapter, nameof(gameAdapter));
            _gameAdapter = gameAdapter;
        }

        public string Slug => EffectSlugs.RandomMeltdown;
        public float CooldownSeconds => 120f;
        public bool IsDanger => false;

        public string Execute(EffectDispatch dispatch)
        {
            if (!_gameAdapter.IsGameReady)
            {
                return ErrorTags.GameNotReady;
            }

            return _gameAdapter.ActivateRandomMeltdown() ? null : ErrorTags.ExecutionError;
        }

        public bool IsAvailableNow(out string reason)
        {
            if (!_gameAdapter.IsGameReady)
            {
                reason = ErrorTags.GameNotReady;
                return false;
            }

            reason = null;
            return true;
        }
    }
}
