// SPDX-License-Identifier: MIT

#region

using System;
using System.Globalization;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;

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
            _fileManager = Guard.Against.Null(fileManager, nameof(fileManager));
            _reportFormatter = Guard.Against.Null(reportFormatter, nameof(reportFormatter));
            _externalLogSource = Guard.Against.Null(externalLogSource, nameof(externalLogSource));
        }

        public string WriteReport(DiagnosticReport report)
        {
            _ = Guard.Against.Null(report, nameof(report));

            var externalLogs = _externalLogSource.GetExternalLogs();
            var lines = _reportFormatter.FormatForLogFile(report, externalLogs);
            var content = string.Join(Environment.NewLine, ToArray(lines));
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture);
            var fileName = "DebugPanel_" + timestamp + ".log";
            var filePath = _fileManager.GetFile(fileName);
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
