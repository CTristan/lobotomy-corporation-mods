// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IFileManager
    {
        string ModFolder { get; }
        void CreateDirectoryIfNotExists(string path);
        string GetFile(string fileName);

        IEnumerable<string> GetFilesFromDirectory(string path,
            string searchPattern = "*.*");

        byte[] ReadAllBytes(string filePath);

        string ReadAllText(string fileWithPath,
            bool createIfNotExists);

        void WriteAllText(string fileWithPath,
            string contents,
            bool append = false);
    }
}
