// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IFileManager
    {
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
