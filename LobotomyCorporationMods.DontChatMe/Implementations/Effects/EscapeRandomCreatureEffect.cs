// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations.Effects
{
    /// <summary>Releases a random contained creature. Danger-gated behind the config flag.</summary>
    public sealed class EscapeRandomCreatureEffect : IEffectExecutor
    {
        private readonly IGameAdapter _gameAdapter;

        public EscapeRandomCreatureEffect(IGameAdapter gameAdapter)
        {
            ThrowHelper.ThrowIfNull(gameAdapter, nameof(gameAdapter));
            _gameAdapter = gameAdapter;
        }

        public string Slug => EffectSlugs.EscapeRandomCreature;
        public float CooldownSeconds => 180f;
        public bool IsDanger => true;

        public string Execute(EffectDispatch dispatch)
        {
            if (!_gameAdapter.IsGameReady)
            {
                return StandardErrors.GameStateBlocked;
            }

            if (_gameAdapter.CreatureCount == 0)
            {
                return StandardErrors.EffectUnavailableNow;
            }

            return _gameAdapter.EscapeRandomCreature() ? null : StandardErrors.ModInternalError;
        }

        public bool IsAvailableNow(out string reason)
        {
            if (!_gameAdapter.IsGameReady)
            {
                reason = StandardErrors.GameStateBlocked;
                return false;
            }

            if (_gameAdapter.CreatureCount == 0)
            {
                reason = StandardErrors.EffectUnavailableNow;
                return false;
            }

            reason = null;
            return true;
        }
    }
}
