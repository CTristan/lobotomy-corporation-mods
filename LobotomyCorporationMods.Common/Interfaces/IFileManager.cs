namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IFileManager
    {
        string ReadAllText(string path);
        string ReadAllText(string path, bool createIfNotExists);
        void WriteAllText(string path, string contents);
    }
}
