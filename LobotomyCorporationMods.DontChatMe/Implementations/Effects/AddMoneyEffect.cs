// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations.Effects
{
    /// <summary>Adds the configured amount of LOB points to the player's stash.</summary>
    public sealed class AddMoneyEffect : IEffectExecutor
    {
        private readonly IGameAdapter _gameAdapter;
        private readonly IDontChatMeConfig _config;

        public AddMoneyEffect(IGameAdapter gameAdapter, IDontChatMeConfig config)
        {
            ThrowHelper.ThrowIfNull(gameAdapter, nameof(gameAdapter));
            ThrowHelper.ThrowIfNull(config, nameof(config));
            _gameAdapter = gameAdapter;
            _config = config;
        }

        public string Slug => EffectSlugs.AddMoney;
        public float CooldownSeconds => 20f;
        public bool IsDanger => false;

        public string Execute(EffectDispatch dispatch)
        {
            if (!_gameAdapter.IsGameReady)
            {
                return ErrorTags.GameNotReady;
            }

            _gameAdapter.AddMoney(_config.MoneyAmount);
            return null;
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
