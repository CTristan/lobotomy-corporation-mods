// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Extensions;
using UnityEngine;

namespace LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Patches
{
    // ReSharper disable once StringLiteralTypo
    [HarmonyPatch(typeof(AgentModel), "ManageCreature")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    // ReSharper disable once IdentifierTypo
    public static class AgentModelPatchManageCreature
    {
        /// <summary>
        ///     Runs before ManageCreature to check if we should disallow working with this abnormality. This is required due to
        ///     the potential for other mods to unintentionally bypass the work assignment check in the command window by calling
        ///     the agent work method directly.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static bool Prefix([NotNull] AgentModel __instance, CreatureModel target, SkillTypeInfo skill, Sprite skillSprite)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));
                Guard.Against.Null(target, nameof(target));

                var creatureOverloadManager = CreatureOverloadManager.instance;
                var playerModel = PlayerModel.instance;
                var stageTypeInfo = StageTypeInfo.instnace;
                var energyModel = EnergyModel.instance;

                return !__instance.CheckIfMaxMeltdown(creatureOverloadManager, target, playerModel, stageTypeInfo, energyModel);
            }
            catch (Exception ex)
            {
                // Null argument exception only comes up during testing due to Unity operator overloading.
                // https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
                if (ex is ArgumentNullException)
                {
                    return true;
                }

                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
