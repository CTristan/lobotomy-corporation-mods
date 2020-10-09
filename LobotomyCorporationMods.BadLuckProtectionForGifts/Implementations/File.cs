using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using UnityEngine;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    internal sealed class File : IFile
    {
        private readonly object _jsonLock = new object();

        public AgentWorkTracker ReadFromJson([NotNull] string path)
        {
            lock (_jsonLock)
            {
                var json = System.IO.File.ReadAllText(path);
                return JsonUtility.FromJson<AgentWorkTracker>(json);
            }
        }

        public void WriteAllText([NotNull] string path, string contents)
        {
            System.IO.File.WriteAllText(path, contents);
        }

        public void WriteToJson([NotNull] string path, object obj)
        {
            lock (_jsonLock)
            {
                var json = JsonUtility.ToJson(obj);
                WriteAllText(path, json);
            }
        }
    }
}
