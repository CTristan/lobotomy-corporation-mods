// SPDX-License-Identifier: MIT

#region

using System;
using System.Globalization;
using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.Common.Models.Diagnostics
{
    public sealed class ExternalLogData
    {
        public ExternalLogData(
            string retargetHarmonyLog,
            string bepInExLog,
            string unityLog,
            string gameplayLog,
            string saveFolderLog,
            string lmmDirectoryLog,
            string lmmSystemLog,
            string baseModsLog)
        {
            ThrowHelper.ThrowIfNull(retargetHarmonyLog);
            RetargetHarmonyLog = retargetHarmonyLog;
            ThrowHelper.ThrowIfNull(bepInExLog);
            BepInExLog = bepInExLog;
            ThrowHelper.ThrowIfNull(unityLog);
            UnityLog = unityLog;
            ThrowHelper.ThrowIfNull(gameplayLog);
            GameplayLog = gameplayLog;
            ThrowHelper.ThrowIfNull(saveFolderLog);
            SaveFolderLog = saveFolderLog;
            ThrowHelper.ThrowIfNull(lmmDirectoryLog);
            LmmDirectoryLog = lmmDirectoryLog;
            ThrowHelper.ThrowIfNull(lmmSystemLog);
            LmmSystemLog = lmmSystemLog;
            ThrowHelper.ThrowIfNull(baseModsLog);
            BaseModsLog = baseModsLog;
            BepInExLogTimestamp = ParseBepInExTimestamp(bepInExLog);
        }

        public string RetargetHarmonyLog { get; private set; }

        public string BepInExLog { get; private set; }

        public string UnityLog { get; private set; }

        public string GameplayLog { get; private set; }

        public string SaveFolderLog { get; private set; }

        public string LmmDirectoryLog { get; private set; }

        public string LmmSystemLog { get; private set; }

        public string BaseModsLog { get; private set; }

        public DateTime? BepInExLogTimestamp { get; private set; }

        public static DateTime? ParseBepInExTimestamp(string bepInExLog)
        {
            if (string.IsNullOrEmpty(bepInExLog))
            {
                return null;
            }

            // BepInEx LogOutput.log first line format: "BepInEx 5.x.x.x - ... (M/d/yyyy h:mm:ss tt)"
            // or similar with the date in parentheses at the end of the first line
            var firstLine = bepInExLog;
            var newlineIndex = bepInExLog.IndexOf('\n');
            if (newlineIndex >= 0)
            {
                firstLine = bepInExLog.Substring(0, newlineIndex);
            }

            var openParen = firstLine.LastIndexOf('(');
            var closeParen = firstLine.LastIndexOf(')');
            if (openParen < 0 || closeParen <= openParen)
            {
                return null;
            }

            var dateString = firstLine.Substring(openParen + 1, closeParen - openParen - 1);

            if (DateTime.TryParseExact(dateString, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }

            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return null;
        }
    }
}
