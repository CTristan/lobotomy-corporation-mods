// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SetupExternal
{
    /// <summary>
    /// Parses Valve's VDF (Valve Data Format) files to extract library folder paths.
    /// </summary>
    public static class VdfParser
    {
        private static readonly string[] LineSeparators = ["\r", "\n"];
        /// <summary>
        /// Extracts all "path" values from a libraryfolders.vdf file.
        /// </summary>
        /// <param name="vdfContent">The content of the VDF file.</param>
        /// <returns>A list of Steam library folder paths.</returns>
        public static IReadOnlyList<string> ExtractLibraryPaths(string vdfContent)
        {
            List<string> paths = [];

            if (string.IsNullOrWhiteSpace(vdfContent))
            {
                return paths;
            }

            // VDF format example:
            // "libraryfolders"
            // {
            //   "0"
            //   {
            //     "path"		"C:\\Program Files (x86)\\Steam"
            //     "label"		""
            //     "contentid"		"123456789"
            //   }
            //   "1"
            //   {
            //     "path"		"D:\\Games\\Steam"
            //     ...
            //   }
            // }
            //
            // We look for lines that contain "path" followed by a quoted string value.

            var lines = vdfContent.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                // Look for "path" followed by a quoted value on the same line or next line
                if (trimmed.StartsWith("\"path\"", StringComparison.Ordinal))
                {
                    // Same line: "path"\t\t"C:\\Program Files (x86)\\Steam"
                    var match = Regex.Match(trimmed, "\"path\"\\s*\"([^\"]+)\"");
                    if (match.Success)
                    {
                        var path = match.Groups[1].Value;
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            paths.Add(path);
                        }
                    }
                }
            }

            return paths;
        }
    }
}
