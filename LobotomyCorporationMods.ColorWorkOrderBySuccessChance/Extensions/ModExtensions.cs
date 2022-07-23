using System.IO;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.ColorWorkOrderBySuccessChance.Extensions
{
    internal static class ModExtensions
    {
        [NotNull] private static readonly object s_fileLock = new object();

        [NotNull]
        public static string GetDataPath(string modFileName)
        {
            foreach (var directoryInfo in Add_On.instance.DirList)
            {
                if (File.Exists(Path.Combine(directoryInfo.FullName, modFileName)))
                {
                    return directoryInfo.FullName;
                }
            }

            return string.Empty;
        }

        [NotNull]
        public static string ReadAllText([NotNull] string path)
        {
            return ReadAllText(path, false);
        }

        [NotNull]
        public static string ReadAllText([NotNull] string path, bool createIfNotExists)
        {
            if (!File.Exists(path))
            {
                if (!createIfNotExists) { return string.Empty; }

                WriteAllText(path, string.Empty);
            }

            lock (s_fileLock)
            {
                return File.ReadAllText(path);
            }
        }

        public static void WriteAllText([NotNull] string path, [NotNull] string contents)
        {
            lock (s_fileLock)
            {
                File.WriteAllText(path, contents);
            }
        }
    }
}
