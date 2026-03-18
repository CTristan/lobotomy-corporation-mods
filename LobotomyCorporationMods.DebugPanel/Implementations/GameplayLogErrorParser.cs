// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    public sealed class GameplayLogErrorParser
    {
        private const string HerrorPrefix = "Herror - ";
        private const string ModDllSeparator = " / ";
        private const string DllExtension = ".dll";
        private const string StackTracePrefix = "  at ";

        public IList<GameplayLogErrorEntry> Parse(string logContent)
        {
            var entries = new List<GameplayLogErrorEntry>();

            if (string.IsNullOrEmpty(logContent))
            {
                return entries;
            }

            var lines = logContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!line.StartsWith(HerrorPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var remainder = line.Substring(HerrorPrefix.Length);
                var modName = string.Empty;
                var dllName = string.Empty;
                string errorMessage;

                var separatorIndex = remainder.IndexOf(ModDllSeparator, StringComparison.Ordinal);
                if (separatorIndex >= 0)
                {
                    modName = remainder.Substring(0, separatorIndex);
                    var afterSeparator = remainder.Substring(separatorIndex + ModDllSeparator.Length);

                    var dllIndex = afterSeparator.IndexOf(DllExtension, StringComparison.OrdinalIgnoreCase);
                    if (dllIndex >= 0)
                    {
                        var dllEnd = dllIndex + DllExtension.Length;
                        dllName = afterSeparator.Substring(0, dllEnd);
                        errorMessage = afterSeparator.Substring(dllEnd);
                    }
                    else
                    {
                        errorMessage = afterSeparator;
                    }
                }
                else
                {
                    errorMessage = remainder;
                }

                var stackTrace = CollectStackTrace(lines, i + 1);

                entries.Add(new GameplayLogErrorEntry(modName, dllName, errorMessage, stackTrace, line));
            }

            return entries;
        }

        private static string CollectStackTrace(string[] lines, int startIndex)
        {
            var stackLines = new List<string>();

            for (var i = startIndex; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(StackTracePrefix, StringComparison.Ordinal))
                {
                    stackLines.Add(lines[i]);
                }
                else
                {
                    break;
                }
            }

            if (stackLines.Count == 0)
            {
                return string.Empty;
            }

            var result = stackLines[0];
            for (var i = 1; i < stackLines.Count; i++)
            {
                result += Environment.NewLine + stackLines[i];
            }

            return result;
        }
    }
}
