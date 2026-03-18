// SPDX-License-Identifier: MIT

#region

using System;
using System.IO;
using Hemocode.Common.Implementations;
using Hemocode.Common.Models.Diagnostics;
using Hemocode.DebugPanel.Interfaces;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    public sealed class GameplayLogErrorCollector : IInfoCollector<GameplayLogErrorReport>
    {
        private readonly IFileSystemScanner _scanner;

        public GameplayLogErrorCollector(IFileSystemScanner scanner)
        {
            ThrowHelper.ThrowIfNull(scanner);
            _scanner = scanner;
        }

        public GameplayLogErrorReport Collect()
        {
            var logContent = ReadGameplayLog();
            var parser = new GameplayLogErrorParser();
            var entries = parser.Parse(logContent);

            return new GameplayLogErrorReport(entries);
        }

        private string ReadGameplayLog()
        {
            try
            {
                var userProfile = _scanner.GetUserProfilePath();
                if (string.IsNullOrEmpty(userProfile))
                {
                    return string.Empty;
                }

                var logPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(userProfile, "AppData"), "LocalLow"), "Project_Moon"), "Lobotomy"), Path.Combine("LobotomyBaseMod", "Log.txt"));

                return _scanner.FileExists(logPath) ? _scanner.ReadLockedFile(logPath) : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
