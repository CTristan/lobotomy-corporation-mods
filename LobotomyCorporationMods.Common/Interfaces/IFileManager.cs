// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IFileManager
    {
        string GetOrCreateFile(string fileName);
        string ReadAllText(string fileWithPath, bool createIfNotExists);
        void WriteAllText(string fileWithPath, string contents);
    }
}
