using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.Test.Fakes
{
    internal sealed class FakeFile : IFile
    {
        public string ReadAllText(string path)
        {
            return string.Empty;
        }

        public void WriteAllText(string path, string contents)
        {
        }
    }
}
