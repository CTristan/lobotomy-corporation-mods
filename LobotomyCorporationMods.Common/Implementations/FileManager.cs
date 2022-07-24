using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class FileManager : IFileManager
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
            if (!File.Exists(path))
            {
                if (!createIfNotExists) { return string.Empty; }

                WriteAllText(path, string.Empty);
            }

            lock (_fileLock)
            {
                return File.ReadAllText(path);
            }
        }

        public void WriteAllText([NotNull] string path, [NotNull] string contents)
        {
            lock (_fileLock)
            {
                File.WriteAllText(path, contents);
            }
        }
    }
}
