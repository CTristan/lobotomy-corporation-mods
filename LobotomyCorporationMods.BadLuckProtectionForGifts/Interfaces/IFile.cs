namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    internal interface IFile
    {
        string ReadAllText(string path);
        string ReadAllText(string path, bool createIfNotExists);
        void WriteAllText(string path, string contents);
    }
}
