// SPDX-License-Identifier: MIT

#region

using System;
using System.Globalization;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public sealed class LogFileWriter : ILogFileWriter
    {
        private readonly IFileManager _fileManager;
        private readonly IReportFormatter _reportFormatter;
        private readonly IExternalLogSource _externalLogSource;

        public LogFileWriter(IFileManager fileManager, IReportFormatter reportFormatter, IExternalLogSource externalLogSource)
        {
            ThrowHelper.ThrowIfNull(fileManager);
            _fileManager = fileManager;
            ThrowHelper.ThrowIfNull(reportFormatter);
            _reportFormatter = reportFormatter;
            ThrowHelper.ThrowIfNull(externalLogSource);
            _externalLogSource = externalLogSource;
        }

        public string WriteReport(DiagnosticReport report)
        {
            ThrowHelper.ThrowIfNull(report);
            _ = report;

            var externalLogs = _externalLogSource.GetExternalLogs();
            var lines = _reportFormatter.FormatForLogFile(report, externalLogs);
            var content = string.Join(Environment.NewLine, ToArray(lines));
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture);
            var fileName = "Logs/DebugPanel_" + timestamp + ".log";
            var filePath = _fileManager.GetFile(fileName);
            _fileManager.EnsureDirectoryExists(filePath);
            _fileManager.WriteAllText(filePath, content);

            return filePath;
        }

        private static string[] ToArray(System.Collections.Generic.IList<string> list)
        {
            var array = new string[list.Count];
            list.CopyTo(array, 0);

            return array;
        }
    }
}
