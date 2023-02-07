// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IFileManager
    {
        string GetFile(string fileName);
        string ReadAllText(string path, bool createIfNotExists);
        void WriteAllText(string path, string contents);
    }
}
