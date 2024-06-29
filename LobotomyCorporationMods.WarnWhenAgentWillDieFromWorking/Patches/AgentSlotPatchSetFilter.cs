// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches
{
    [HarmonyPatch(typeof(AgentSlot), nameof(AgentSlot.SetFilter))]
    public static class AgentSlotPatchSetFilter
    {
        /// <summary>Dictionary that contains factory methods for creating ICreatureEvaluator objects based on CreatureIds.</summary>
        private static readonly Dictionary<CreatureIds, Func<CreatureEvaluatorParameters, ICreatureEvaluator>>
            s_evaluatorFactoryDictionary = InitEvaluatorFactoryDictionary(); // ReSharper disable InconsistentNaming

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] AgentSlot __instance,
            AgentState state)
        {
            try
            {
                var currentGameManager = GameManager.currentGameManager;
                __instance.PatchAfterSetFilter(state, currentGameManager, new BeautyBeastAnimAdapter(), new ImageAdapter(), new TextAdapter(), new YggdrasilAnimAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
        // ReSharper enable InconsistentNaming

        public static void PatchAfterSetFilter([NotNull] this AgentSlot instance,
            AgentState state,
            [CanBeNull] GameManager currentGameManager,
            IBeautyBeastAnimAdapter beautyBeastAnimAdapter,
            [NotNull] IImageAdapter imageAdapter,
            [NotNull] ITextAdapter textAdapter,
            IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(imageAdapter, nameof(imageAdapter));
            Guard.Against.Null(textAdapter, nameof(textAdapter));

            if (!currentGameManager.IsValidGameStage(state))
            {
                return;
            }

            var agentWillDie = instance.CheckIfWorkWillKillAgent(CommandWindow.CommandWindow.CurrentWindow, s_evaluatorFactoryDictionary, beautyBeastAnimAdapter, yggdrasilAnimAdapter);

            if (!agentWillDie)
            {
                return;
            }

            instance.IndicateThatAgentWillDie(imageAdapter, textAdapter);
        }

        /// <summary>Stores our evaluators in a dictionary of factories so that we only need to create the dictionary once but can make evaluators from the factories as often as we need to.</summary>
        /// <returns></returns>
        [NotNull]
        private static Dictionary<CreatureIds, Func<CreatureEvaluatorParameters, ICreatureEvaluator>> InitEvaluatorFactoryDictionary()
        {
            return new Dictionary<CreatureIds, Func<CreatureEvaluatorParameters, ICreatureEvaluator>>
            {
                {
                    CreatureIds.BeautyAndTheBeast, parameters => new BeautyAndTheBeastEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType, parameters.BeautyBeastAnimAdapter)
                },
                {
                    CreatureIds.Bloodbath, parameters => new BloodbathEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
                {
                    CreatureIds.BlueStar, parameters => new BlueStarEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
                {
                    CreatureIds.CrumblingArmor, parameters => new CrumblingArmorEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
                {
                    CreatureIds.HappyTeddyBear, parameters => new HappyTeddyBearEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
                {
                    CreatureIds.NothingThere, parameters => new NothingThereEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
                {
                    CreatureIds.ParasiteTree, parameters => new ParasiteTreeEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType, parameters.YggdrasilAnimAdapter)
                },
                {
                    CreatureIds.RedShoes, parameters => new RedShoesEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
                {
                    CreatureIds.SingingMachine, parameters => new SingingMachineEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
                {
                    CreatureIds.SpiderBud, parameters => new SpiderBudEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
                {
                    CreatureIds.VoidDream, parameters => new VoidDreamEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
                {
                    CreatureIds.WarmHeartedWoodsman, parameters => new WarmHeartedWoodsmanEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType)
                },
            };
        }
    }
}
