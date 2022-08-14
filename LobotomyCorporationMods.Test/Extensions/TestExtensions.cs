// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;
using NSubstitute;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace LobotomyCorporationMods.Test
{
    internal static class TestExtensions
    {
        [NotNull]
        public static IFileManager CreateFileManager()
        {
            var fileManager = Substitute.For<IFileManager>(null);
            fileManager.GetFile(default).ReturnsForAnyArgs(x => x.Arg<string>().InCurrentDirectory());
            fileManager.ReadAllText(default, default).ReturnsForAnyArgs(x => File.ReadAllText(x.Arg<string>().InCurrentDirectory()));

            return fileManager;
        }

        [NotNull]
        internal static string InCurrentDirectory([NotNull] this string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        private static void CreateUninitializedObject<TObject>(out TObject obj)
        {
            obj = (TObject)FormatterServices.GetSafeUninitializedObject(typeof(TObject));
        }

        public static GameObject CreateGameObject()
        {
            CreateUninitializedObject(out GameObject obj);

            return obj;
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

        #region Unity Objects

        [NotNull]
        public static CreatureEquipmentMakeInfo CreateCreatureEquipmentMakeInfo(string giftName)
        {
            var info = Substitute.For<CreatureEquipmentMakeInfo>();
            info.equipTypeInfo = new EquipmentTypeInfo { localizeData = new Dictionary<string, string> { { "name", giftName } }, type = EquipmentTypeInfo.EquipmentType.SPECIAL };

            LocalizeTextDataModel.instance?.Init(new Dictionary<string, string> { { giftName, giftName } });

            return info;
        }

        [NotNull]
        public static UseSkill CreateUseSkill(string giftName, long agentId, int numberOfSuccesses)
        {
            var useSkill = Substitute.For<UseSkill>();
            CreateUninitializedObject(out useSkill.agent);
            useSkill.agent.instanceId = agentId;
            CreateUninitializedObject(out useSkill.targetCreature);
            useSkill.targetCreature.metaInfo = new CreatureTypeInfo { equipMakeInfos = new List<CreatureEquipmentMakeInfo> { CreateCreatureEquipmentMakeInfo(giftName) } };
            useSkill.successCount = numberOfSuccesses;

            return useSkill;
        }

        #endregion
    }
}
