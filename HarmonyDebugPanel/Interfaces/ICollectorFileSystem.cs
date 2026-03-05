// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace HarmonyDebugPanel.Interfaces
{
    public interface ICollectorFileSystem
    {
        bool DirectoryExists(string path);

        IEnumerable<string> EnumerateDirectories(string path);
    }
}
