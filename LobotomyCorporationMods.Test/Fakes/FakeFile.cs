using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts;

namespace LobotomyCorporationMods.Test.Fakes
{
    internal sealed class FakeFile : IFile
    {
        [NotNull]
        public string ReadAllText([NotNull] string path)
        {
            return string.Empty;
        }

        public void WriteAllText([NotNull] string path, [NotNull] string contents)
        {
        }
    }
}
