// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Test.CommonTests
{
    internal sealed class FakeHarmonyPatch : HarmonyPatchBase
    {
        internal FakeHarmonyPatch(bool isNotDuplicating) : base(isNotDuplicating)
        {
        }

        internal void TestInitializePatchData(List<DirectoryInfo> directoryList)
        {
            InitializePatchData(typeof(FakeHarmonyPatch), "LobotomyCorporationMods.Test.dll", directoryList);
        }

        internal void ApplyHarmonyPatch([NotNull] Type harmonyPatchType, string modFileName, [NotNull] ILogger logger)
        {
            LoadData(logger);
            Instance.LoadData(logger);

            base.ApplyHarmonyPatch(harmonyPatchType, modFileName);
        }
    }
}
