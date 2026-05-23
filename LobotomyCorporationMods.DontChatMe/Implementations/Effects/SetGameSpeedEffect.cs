// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations.Effects
{
    /// <summary>
    ///     Doubles the game speed. v1 ships one fixed variant; future slugs can expand the catalog
    ///     (pause, slow, etc.) once timed effects land.
    /// </summary>
    public sealed class SetGameSpeedEffect : IEffectExecutor
    {
        public const float FastSpeed = 2f;

        private readonly IGameAdapter _gameAdapter;

        public SetGameSpeedEffect(IGameAdapter gameAdapter)
        {
            ThrowHelper.ThrowIfNull(gameAdapter, nameof(gameAdapter));
            _gameAdapter = gameAdapter;
        }

        public string Slug => EffectSlugs.SetGameSpeed;
        public float CooldownSeconds => 30f;
        public bool IsDanger => false;

        public string Execute(EffectDispatch dispatch)
        {
            if (!_gameAdapter.IsGameReady)
            {
                return StandardErrors.GameStateBlocked;
            }

            _gameAdapter.SetGameSpeed(FastSpeed);
            return null;
        }

        public bool IsAvailableNow(out string reason)
        {
            if (!_gameAdapter.IsGameReady)
            {
                reason = StandardErrors.GameStateBlocked;
                return false;
            }

            reason = null;
            return true;
        }
    }
}
