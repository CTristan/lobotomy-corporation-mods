using System;

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IFileManager
    {
        string GetFile(string fileName);
        string ReadAllText(string path);
        string ReadAllText(string path, bool createIfNotExists);
        void WriteAllText(string path, string contents);
        void WriteToLog(string message, string logFileName = "log.txt");
        void WriteToLog(Exception ex, string logFileName = "log.txt");
    }
}
