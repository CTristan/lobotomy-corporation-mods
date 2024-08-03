using System;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.ProjectNugway.Patches
{
    [HarmonyPatch(typeof(WorkerSpriteManager), nameof(WorkerSpriteManager.LoadCustomSprites))]
    public static class WorkerSpriteManagerPatchLoadCustomSprites
    {
        /// <summary>We need this because the standalone version of Basemod does not have a way to load sprites from the mod folder.</summary>
        /// <param name="instance"></param>
        public static void PatchAfterLoadCustomSprites([NotNull] this WorkerSpriteManager instance)
        {
            Guard.Against.Null(instance, nameof(instance));

            var modFolder = Harmony_Patch.Instance.FileManager.ModFolder;
            instance.LoadCustomSpritesFromModFolder(modFolder);
        }

        [EntryPoint]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] WorkerSpriteManager __instance)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));
                __instance.PatchAfterLoadCustomSprites();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogError(ex);

                throw;
            }
        }
    }
}
