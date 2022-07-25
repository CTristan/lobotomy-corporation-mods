using System;
using System.Runtime.Serialization;
using System.Security;
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

        /// <summary>
        ///     Depending on the environment, the same test may return a different exception. Both exception types appear for the
        ///     same reason (trying to use a Unity-specific method), so we'll check if the exception we get is either of those
        ///     types to verify our test isn't getting an exception for some other reason.
        /// </summary>
        public static bool AssertIsUnityException(Exception exception)
        {
            return exception is SecurityException || exception is MissingMethodException;
        }
    }
}
