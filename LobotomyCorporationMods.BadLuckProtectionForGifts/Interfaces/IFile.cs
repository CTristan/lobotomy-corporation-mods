namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    public interface IFile
    {
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
    }
}
