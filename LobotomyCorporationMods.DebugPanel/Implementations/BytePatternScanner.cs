// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Text;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public sealed class BytePatternScanner
    {
        // Ordered longest-first to prevent substring false positives
        private static readonly string[] s_knownPatterns =
        {
            "0Harmony109",
            "0Harmony12",
            "12Harmony",
            "0Harmony",
        };

        public IList<string> FindHarmonyReferences(byte[] dllBytes)
        {
            ThrowHelper.ThrowIfNull(dllBytes);
            _ = dllBytes;

            var results = new List<string>();

            if (dllBytes.Length == 0)
            {
                return results;
            }

            var consumed = new bool[dllBytes.Length];

            foreach (var pattern in s_knownPatterns)
            {
                var patternBytes = Encoding.UTF8.GetBytes(pattern);

                if (FindPattern(dllBytes, patternBytes, consumed))
                {
                    results.Add(pattern);
                }
            }

            return results;
        }

        private static bool FindPattern(byte[] source, byte[] pattern, bool[] consumed)
        {
            if (pattern.Length == 0 || pattern.Length > source.Length)
            {
                return false;
            }

            var limit = source.Length - pattern.Length;

            for (var i = 0; i <= limit; i++)
            {
                if (consumed[i])
                {
                    continue;
                }

                var match = true;

                for (var j = 0; j < pattern.Length; j++)
                {
                    if (consumed[i + j] || source[i + j] != pattern[j])
                    {
                        match = false;

                        break;
                    }
                }

                if (match)
                {
                    for (var j = 0; j < pattern.Length; j++)
                    {
                        consumed[i + j] = true;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
