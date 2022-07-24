// ReSharper disable CheckNamespace

using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public static class FileManagerExtensions
    {
        [NotNull]
        public static string GetDataPath(this IFileManager fileManager, string modFileName)
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
    }
}
