// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.IO;
using HarmonyDebugPanel.Interfaces;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class CollectorFileSystem : ICollectorFileSystem
    {
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }
    }
}
