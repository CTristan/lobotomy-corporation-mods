// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using WorkerSprite;

namespace LobotomyCorporationMods.CustomizationOverhaul.Patches
{
    [HarmonyPatch(typeof(WorkerSpriteManager), nameof(WorkerSpriteManager.LoadCustomSprites))]
    public static class WorkerSpriteManagerPatchLoadCustomSprites
    {
        /// <summary>We need this because the standalone version of Basemod does not have a way to load sprites from the mod folder.</summary>
        public static void PatchAfterLoadCustomSprites(
            [NotNull] this WorkerSpriteManager instance,
            [CanBeNull] IFileManager fileManager = null
        )
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            fileManager = fileManager ?? Harmony_Patch.Instance.FileManager;
            var customDataFolder = fileManager.GetFile("CustomData");
            if (!Directory.Exists(customDataFolder))
            {
                return;
            }

            foreach (var entry in GetRegionDirectoryMap(customDataFolder))
            {
                LoadSpriteIfExists(instance, entry.Key, entry.Value);
            }
        }

        private static Dictionary<string, BasicSpriteRegion> GetRegionDirectoryMap(
            string customDataFolder
        )
        {
            return new Dictionary<string, BasicSpriteRegion>
            {
                {
                    Path.Combine(customDataFolder, "Face/Eye_Default"),
                    BasicSpriteRegion.EYE_DEFAULT
                },
                { Path.Combine(customDataFolder, "Face/Eye_Panic"), BasicSpriteRegion.EYE_PANIC },
                { Path.Combine(customDataFolder, "Face/Eye_Dead"), BasicSpriteRegion.EYE_DEAD },
                {
                    Path.Combine(customDataFolder, "Face/Eyebrow_Default"),
                    BasicSpriteRegion.EYEBROW
                },
                {
                    Path.Combine(customDataFolder, "Face/Eyebrow_Battle"),
                    BasicSpriteRegion.EYEBROW_BATTLE
                },
                {
                    Path.Combine(customDataFolder, "Face/Eyebrow_Panic"),
                    BasicSpriteRegion.EYEBROW_PANIC
                },
                { Path.Combine(customDataFolder, "Face/Mouth_Default"), BasicSpriteRegion.MOUTH },
                {
                    Path.Combine(customDataFolder, "Face/Mouth_Battle"),
                    BasicSpriteRegion.MOUTH_BATTLE
                },
                { Path.Combine(customDataFolder, "Hair/Front"), BasicSpriteRegion.HAIR_FRONT },
                { Path.Combine(customDataFolder, "Hair/Rear"), BasicSpriteRegion.HAIR_REAR },
            };
        }

        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        private static void LoadSpriteIfExists(
            WorkerSpriteManager workerSpriteManager,
            [NotNull] string path,
            BasicSpriteRegion region
        )
        {
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                return;
            }

            workerSpriteManager.LoadCustomSprite(directoryInfo, region, GetSizeRef(region));
        }

        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        private static WorkerSpriteManager.SizeRef GetSizeRef(BasicSpriteRegion region)
        {
            switch (region)
            {
                case BasicSpriteRegion.EYE_DEFAULT:
                case BasicSpriteRegion.EYE_PANIC:
                case BasicSpriteRegion.EYE_DEAD:
                    return WorkerSpriteManager.SizeRef.Eye();
                case BasicSpriteRegion.EYEBROW:
                case BasicSpriteRegion.EYEBROW_BATTLE:
                case BasicSpriteRegion.EYEBROW_PANIC:
                    return WorkerSpriteManager.SizeRef.Eyebrow();
                case BasicSpriteRegion.MOUTH:
                case BasicSpriteRegion.MOUTH_BATTLE:
                    return WorkerSpriteManager.SizeRef.Mouth();
                case BasicSpriteRegion.HAIR_FRONT:
                    return WorkerSpriteManager.SizeRef.FrontHair();
                case BasicSpriteRegion.HAIR_REAR:
                    return WorkerSpriteManager.SizeRef.RearHair();
                default:
                    throw new ArgumentOutOfRangeException(nameof(region), region, null);
            }
        }

        [EntryPoint]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] WorkerSpriteManager __instance)
        {
            try
            {
                ThrowHelper.ThrowIfNull(__instance, nameof(__instance));
                __instance.PatchAfterLoadCustomSprites();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
