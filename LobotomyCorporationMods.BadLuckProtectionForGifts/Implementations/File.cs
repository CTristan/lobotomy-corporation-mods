using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    internal sealed class File : IFile
    {
        [NotNull] private readonly object _fileLock = new object();

        [NotNull]
        public string ReadAllText([NotNull] string path)
        {
            return ReadAllText(path, false);
        }

        [NotNull]
        public string ReadAllText([NotNull] string path, bool createIfNotExists)
        {
            if (!System.IO.File.Exists(path))
            {
                if (!createIfNotExists) { return string.Empty; }

                WriteAllText(path, string.Empty);
            }

            lock (_fileLock)
            {
                return System.IO.File.ReadAllText(path);
            }
        }

        public void WriteAllText([NotNull] string path, [NotNull] string contents)
        {
            lock (_fileLock)
            {
                System.IO.File.WriteAllText(path, contents);
            }
        }
    }
}
