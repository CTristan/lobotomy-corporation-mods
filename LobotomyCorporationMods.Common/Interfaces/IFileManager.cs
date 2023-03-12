// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IFileManager
    {
        string GetFile(string fileName);
        string GetOrCreateFile(string fileName);
        byte[] ReadAllBytes(string path);
        string ReadAllText(string fileWithPath, bool createIfNotExists);
        void WriteAllText(string fileWithPath, string contents);
    }
}
