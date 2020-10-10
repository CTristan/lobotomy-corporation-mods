namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IFile
    {
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
    }
}
