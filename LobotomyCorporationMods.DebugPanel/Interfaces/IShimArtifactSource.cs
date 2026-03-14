// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IShimArtifactSource
    {
        bool BackupDirectoryExists { get; }

        string BackupDirectoryPath { get; }

        bool InteropCacheExists { get; }

        string InteropCachePath { get; }

        IList<string> GetBackupFileNames();

        byte[] ReadBackupFileBytes(string fileName);

        int GetInteropCacheEntryCount();
    }
}
