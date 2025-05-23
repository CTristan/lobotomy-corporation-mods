// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IFileManager
    {
        void AppendAllText(string filePath, string contents);

        string GetFullPathForFile(string fileName);

        byte[] ReadAllBytes(string filePath);

        string ReadAllText(string filePath,
            bool createIfNotExists);

        void WriteAllText(string filePath,
            string contents);
    }
}
