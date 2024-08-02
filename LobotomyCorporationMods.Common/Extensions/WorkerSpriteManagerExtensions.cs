// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using WorkerSprite;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class WorkerSpriteManagerExtensions
    {
        /// <summary>
        ///     Loads all custom sprites in a "CustomData" folder inside the mod folder itself rather than the root folder. We need this because the standalone version of Basemod doesn't
        ///     have a way to load sprites from the mod folder.
        /// </summary>
        /// <param name="workerSpriteManager"></param>
        /// <param name="modFolder"></param>
        public static void LoadCustomSpritesFromModFolder([NotNull] this WorkerSpriteManager workerSpriteManager,
            [NotNull] string modFolder)
        {
            Guard.Against.Null(workerSpriteManager, nameof(workerSpriteManager));

            var modFolderSrc = Path.Combine(modFolder, "CustomData");
            if (!Directory.Exists(modFolderSrc))
            {
                return;
            }


            var directories = new Dictionary<string, BasicSpriteRegion>
            {
                {
                    Path.Combine(modFolderSrc, "Face/Eye_Default"), BasicSpriteRegion.EYE_DEFAULT
                },
                {
                    Path.Combine(modFolderSrc, "Face/Eye_Panic"), BasicSpriteRegion.EYE_PANIC
                },
                {
                    Path.Combine(modFolderSrc, "Face/Eye_Dead"), BasicSpriteRegion.EYE_DEAD
                },
                {
                    Path.Combine(modFolderSrc, "Face/Eyebrow_Default"), BasicSpriteRegion.EYEBROW
                },
                {
                    Path.Combine(modFolderSrc, "Face/Eyebrow_Battle"), BasicSpriteRegion.EYEBROW_BATTLE
                },
                {
                    Path.Combine(modFolderSrc, "Face/Eyebrow_Panic"), BasicSpriteRegion.EYEBROW_PANIC
                },
                {
                    Path.Combine(modFolderSrc, "Face/Mouth_Default"), BasicSpriteRegion.MOUTH
                },
                {
                    Path.Combine(modFolderSrc, "Face/Mouth_Battle"), BasicSpriteRegion.MOUTH_BATTLE
                },
                {
                    Path.Combine(modFolderSrc, "Hair/Front"), BasicSpriteRegion.HAIR_FRONT
                },
                {
                    Path.Combine(modFolderSrc, "Hair/Rear"), BasicSpriteRegion.HAIR_REAR
                },
            };

            foreach (var entry in directories)
            {
                LoadSpriteIfExists(workerSpriteManager, entry.Key, entry.Value);
            }
        }

        private static void LoadSpriteIfExists(WorkerSpriteManager workerSpriteManager,
            [NotNull] string path,
            BasicSpriteRegion region)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                return;
            }

            var sizeRef = GetSizeRef(region);
            workerSpriteManager.LoadCustomSprite(directoryInfo, region, sizeRef);
        }

        private static WorkerSpriteManager.SizeRef GetSizeRef(BasicSpriteRegion region)
        {
            var sizeRefMap = new Dictionary<BasicSpriteRegion, Func<WorkerSpriteManager.SizeRef>>
            {
                {
                    BasicSpriteRegion.EYE_DEFAULT, WorkerSpriteManager.SizeRef.Eye
                },
                {
                    BasicSpriteRegion.EYE_PANIC, WorkerSpriteManager.SizeRef.Eye
                },
                {
                    BasicSpriteRegion.EYE_DEAD, WorkerSpriteManager.SizeRef.Eye
                },
                {
                    BasicSpriteRegion.EYEBROW, WorkerSpriteManager.SizeRef.Eyebrow
                },
                {
                    BasicSpriteRegion.EYEBROW_BATTLE, WorkerSpriteManager.SizeRef.Eyebrow
                },
                {
                    BasicSpriteRegion.EYEBROW_PANIC, WorkerSpriteManager.SizeRef.Eyebrow
                },
                {
                    BasicSpriteRegion.MOUTH, WorkerSpriteManager.SizeRef.Mouth
                },
                {
                    BasicSpriteRegion.MOUTH_BATTLE, WorkerSpriteManager.SizeRef.Mouth
                },
                {
                    BasicSpriteRegion.HAIR_FRONT, WorkerSpriteManager.SizeRef.FrontHair
                },
                {
                    BasicSpriteRegion.HAIR_REAR, WorkerSpriteManager.SizeRef.RearHair
                },
            };

            if (sizeRefMap.TryGetValue(region, out var sizeRefFunc))
            {
                return sizeRefFunc();
            }

            throw new ArgumentOutOfRangeException(nameof(region), region, null);
        }
    }
}
