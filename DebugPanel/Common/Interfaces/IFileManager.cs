// SPDX-License-Identifier: MIT

namespace DebugPanel.Common.Interfaces
{
    public interface IFileManager
    {
        void EnsureDirectoryExists(string filePath);

        string GetFile(string fileName);

        byte[] ReadAllBytes(string filePath);

        string ReadAllText(string fileWithPath,
            bool createIfNotExists);

        void WriteAllText(string fileWithPath,
            string contents);
    }
}
