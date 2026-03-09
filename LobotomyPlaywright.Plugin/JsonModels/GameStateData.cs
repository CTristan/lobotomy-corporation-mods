// SPDX-License-Identifier: MIT

using System;

namespace LobotomyPlaywright.JsonModels
{
    [Serializable]
    public class GameStateData
    {
        public int day;
        public string gameState;
        public int gameSpeed;
        public float energy;
        public float energyQuota;
        public bool managementStarted;
        public bool isPaused;
        public string emergencyLevel;
        public float playTime;
        public int lobPoints;
    }
}
