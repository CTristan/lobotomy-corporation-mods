// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentData), nameof(AgentData.AppearCopy))]
    public static class AgentDataPatchAppearCopy
    {
        public static void PatchAfterAppearCopy([NotNull] this AgentData instance,
            [NotNull] AgentData copied)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(copied, nameof(copied));

            instance.appearance.Mouth_Panic = copied.appearance.Mouth_Panic;
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] AgentData __instance,
            [NotNull] AgentData copied)
        {
            try
            {
                __instance.PatchAfterAppearCopy(copied);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
