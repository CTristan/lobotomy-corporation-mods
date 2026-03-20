// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using DebugPanel.Common.Implementations;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    public sealed class ErrorLogCollector : IInfoCollector<ErrorLogReport>
    {
        private static readonly string[] s_errorLogFileNames =
        {
            "Herror.txt",
            "LMMerror.txt",
            "GlError.txt",
            "DllError.txt",
            "LTDerror.txt",
            "DPerror.txt",
            "Glerror.txt",
        };

        private readonly IFileSystemScanner _scanner;

        public ErrorLogCollector(IFileSystemScanner scanner)
        {
            ThrowHelper.ThrowIfNull(scanner);
            _scanner = scanner;
        }

        public ErrorLogReport Collect()
        {
            var entries = new List<ErrorLogEntry>();
            var baseModsPath = _scanner.GetBaseModsPath();

            foreach (var fileName in s_errorLogFileNames)
            {
                var filePath = baseModsPath + "/" + fileName;
                if (!_scanner.FileExists(filePath))
                {
                    continue;
                }

                try
                {
                    var content = _scanner.ReadAllText(filePath);
                    entries.Add(new ErrorLogEntry(fileName, content, filePath));
                }
                catch (Exception ex)
                {
                    entries.Add(new ErrorLogEntry(fileName, "Error reading file: " + ex.Message, filePath));
                }
            }

            return new ErrorLogReport(entries);
        }
    }
}
