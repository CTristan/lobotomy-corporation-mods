// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright.Queries
{
    public class GameStateData
    {
        public int Day { get; set; }
        public string GameState { get; set; }
        public int GameSpeed { get; set; }
        public float Energy { get; set; }
        public float EnergyQuota { get; set; }
        public bool ManagementStarted { get; set; }
        public bool IsPaused { get; set; }
        public string EmergencyLevel { get; set; }
        public float PlayTime { get; set; }
        public int LobPoints { get; set; }
    }
}
