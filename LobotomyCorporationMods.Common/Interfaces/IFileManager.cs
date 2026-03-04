// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IFileManager
    {
        string GetFile(string fileName);

        byte[] ReadAllBytes(string filePath);

        string ReadAllText(string fileWithPath,
            bool createIfNotExists);

        void WriteAllText(string fileWithPath,
            string contents);
    }
}