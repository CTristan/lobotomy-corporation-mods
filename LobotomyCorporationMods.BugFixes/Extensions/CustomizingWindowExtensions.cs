// SPDX-License-Identifier: MIT

using System;
using System.Security;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.BugFixes.Extensions
{
    public static class CustomizingWindowExtensions
    {
        public static void UpgradeStat([NotNull] this CustomizingWindow customizingWindow, int originalStatLevel, int currentStatLevel, int statLevelIncrease, out int upgradedStatValue)
        {
            try
            {
                Guard.Against.Null(customizingWindow, nameof(customizingWindow));

                upgradedStatValue = customizingWindow.SetRandomStatValue(originalStatLevel, currentStatLevel, statLevelIncrease);
            }
            catch (SecurityException)
            {
                // Only happens during testing, so we'll just copy the original method and return the system's random method
                HandleUnityError(originalStatLevel, currentStatLevel, statLevelIncrease, out upgradedStatValue);
            }
            catch (MissingMemberException)
            {
                // Only happens during testing, so we'll just copy the original method and return the system's random method
                HandleUnityError(originalStatLevel, currentStatLevel, statLevelIncrease, out upgradedStatValue);
            }
        }

        /// <summary>
        ///     Copy of the original game code that sets a random stat value. Used for testing because the original game code calls
        ///     Unity methods that we can't access.
        /// </summary>
        private static void HandleUnityError(int originalStatLevel, int currentStatLevel, int statLevelIncrease, out int upgradedStatValue)
        {
            // Multiplier is the level 6 minimum value divided by the level number
            const float StatValueMultiplier = 110f / 6f;
            const int InitialStatValue = 15;
            var newLevel = currentStatLevel + statLevelIncrease;
            const int LevelOne = 1;
            const int LevelSix = 6;
            if (statLevelIncrease == 0 || newLevel > LevelSix)
            {
                upgradedStatValue = originalStatLevel;

                return;
            }

            if (newLevel == LevelOne)
            {
                upgradedStatValue = InitialStatValue;

                return;
            }

            var min = (int)Math.Floor(newLevel * StatValueMultiplier);
            var random = new Random();
            upgradedStatValue = random.Next(min, min);
        }
    }
}
