using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    internal sealed class File : IFile
    {
        private readonly object _fileLock = new object();

        public void WriteAllText([NotNull] string path, string contents)
        {
            lock (_fileLock)
            {
                System.IO.File.WriteAllText(path, contents);
            }
        }
    }
}
