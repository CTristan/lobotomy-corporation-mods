using System.Runtime.Serialization;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;
using NSubstitute;

namespace LobotomyCorporationMods.Test
{
    internal static class TestExtensions
    {
        /// <summary>
        ///     Create an uninitialized object without calling a constructor. Needed because some of the classes we need
        ///     to mock either don't have a public constructor or cause a Unity exception.
        /// </summary>
        public static TObject CreateUninitializedObject<TObject>()
        {
            return (TObject)FormatterServices.GetSafeUninitializedObject(typeof(TObject));
        }

        [NotNull]
        public static IFileManager GetFileManager()
        {
            return Substitute.For<IFileManager>(null);
        }
    }
}
