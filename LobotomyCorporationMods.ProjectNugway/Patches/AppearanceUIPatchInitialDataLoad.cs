using System;
using System.Collections.Generic;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using UnityEngine;
using WorkerSprite;

namespace LobotomyCorporationMods.ProjectNugway.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), nameof(AppearanceUI.InitialDataLoad))]
    public static class AppearanceUiPatchInitialDataLoad
    {
        public static void PatchAfterInitialDataLoad([NotNull] this AppearanceUI instance)
        {
            Guard.Against.Null(instance, nameof(instance));

            var mouthBattleSprites = new List<Sprite>();
            mouthBattleSprites.AddRange(instance.mouth_Battle.SpriteList);
            var basicData = WorkerSpriteManager.instance.basicData;
            if (basicData.GetData(BasicSpriteRegion.MOUTH_PANIC, out var mouthPanicSprites))
            {
                var allMouthPanicSprites = mouthPanicSprites.GetAllSprites();
                mouthBattleSprites.AddRange(allMouthPanicSprites);
            }

            instance.mouth_Battle.Init(mouthBattleSprites);
        }

        [EntryPoint]
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
