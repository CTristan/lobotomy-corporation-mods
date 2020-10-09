using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.Test.Fakes
{
    internal sealed class FakeFile : IFile
    {
        private readonly AgentWorkTracker _tracker = new AgentWorkTracker();

        public AgentWorkTracker ReadFromJson(string path)
        {
            return _tracker;
        }

        public void WriteAllText(string path, string contents)
        {
        }

        public void WriteToJson(string path, object obj)
        {
        }
    }
}
