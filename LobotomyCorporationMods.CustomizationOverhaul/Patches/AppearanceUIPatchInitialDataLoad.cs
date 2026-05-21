// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using UnityEngine;
using WorkerSprite;

namespace LobotomyCorporationMods.CustomizationOverhaul.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), nameof(AppearanceUI.InitialDataLoad))]
    public static class AppearanceUiPatchInitialDataLoad
    {
        public static void PatchAfterInitialDataLoad([NotNull] this AppearanceUI instance)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            var mouthBattleSprites = new List<Sprite>();
            mouthBattleSprites.AddRange(instance.mouth_Battle.SpriteList);

            var basicData = WorkerSpriteManager.instance.basicData;
            if (basicData.GetData(BasicSpriteRegion.MOUTH_PANIC, out var mouthPanicSprites))
            {
                mouthBattleSprites.AddRange(mouthPanicSprites.GetAllSprites());
            }

            instance.mouth_Battle.Init(mouthBattleSprites);
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] AppearanceUI __instance)
        {
            try
            {
                ThrowHelper.ThrowIfNull(__instance, nameof(__instance));
                __instance.PatchAfterInitialDataLoad();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
