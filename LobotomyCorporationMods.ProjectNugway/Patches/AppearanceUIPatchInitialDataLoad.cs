using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.ProjectNugway.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), nameof(AppearanceUI.InitialDataLoad))]
    public static class AppearanceUiPatchInitialDataLoad
    {
        public static void PatchAfterInitialDataLoad([NotNull] this AppearanceUI instance)
        {
            Guard.Against.Null(instance, nameof(instance));

            instance.LoadPanicMouthSpritesIntoBattleMouths();
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] AppearanceUI __instance)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));
                __instance.PatchAfterInitialDataLoad();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogError(ex);

                throw;
            }
        }
    }
}
