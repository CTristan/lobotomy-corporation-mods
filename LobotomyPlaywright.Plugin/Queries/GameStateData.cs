// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;

namespace LobotomyPlaywright.Queries
{
    [Serializable]
    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
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
