namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IFile
    {
        AgentWorkTracker ReadFromJson(string path);
        void WriteAllText(string path, string contents);
        void WriteToJson(string path, object obj);
    }
}
