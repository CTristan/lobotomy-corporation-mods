// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.DontChatMe.Configuration;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes
{
    public sealed class FakeConfig : IDontChatMeConfig
    {
        public Uri ServerUrl { get; set; }
        public string AuthToken { get; set; } = string.Empty;
        public int GameId { get; set; } = 1;
        public bool Enabled { get; set; } = true;
        public bool DangerEffectsEnabled { get; set; }
        public float GlobalCooldownSeconds { get; set; }
        public float EnergyAmount { get; set; } = 10f;
        public int MoneyAmount { get; set; } = 100;
        public int SaveCount { get; private set; }

        public void Save()
        {
            SaveCount++;
        }
    }
}
