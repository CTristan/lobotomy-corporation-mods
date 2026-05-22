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
    /// <summary>Adds the configured amount of energy to the facility.</summary>
    public sealed class AddEnergyEffect : IEffectExecutor
    {
        private readonly IGameAdapter _gameAdapter;
        private readonly IDontChatMeConfig _config;

        public AddEnergyEffect(IGameAdapter gameAdapter, IDontChatMeConfig config)
        {
            ThrowHelper.ThrowIfNull(gameAdapter, nameof(gameAdapter));
            ThrowHelper.ThrowIfNull(config, nameof(config));
            _gameAdapter = gameAdapter;
            _config = config;
        }

        public string Slug => EffectSlugs.AddEnergy;
        public float CooldownSeconds => 15f;
        public bool IsDanger => false;

        public string Execute(EffectDispatch dispatch)
        {
            if (!_gameAdapter.IsGameReady)
            {
                return ErrorTags.GameNotReady;
            }

            _gameAdapter.AddEnergy(_config.EnergyAmount);
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
