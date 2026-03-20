// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IFileSystemScanner
    {
        bool DirectoryExists(string path);

        bool FileExists(string path);

        IList<string> GetFiles(string directoryPath, string searchPattern);

        IList<string> GetDirectories(string path);

        string ReadAllText(string path);

        long GetFileSize(string path);

        string GetBaseModsPath();

        string GetSaveDataPath();

        string GetGameRootPath();

        string GetExternalDataPath();

        string GetUserProfilePath();

        string ReadLockedFile(string path);

        DateTime GetLastWriteTimeUtc(string path);
    }
}
