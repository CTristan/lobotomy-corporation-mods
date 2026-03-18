// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace Hemocode.Playwright.JsonModels
{
    [Serializable]
    public sealed class GameStateData
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
