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
using LobotomyCorporationMods.Common.Implementations.Facades;
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
        private static readonly Dictionary<CreatureIds, Func<CreatureEvaluatorParameters, ICreatureEvaluator>> s_evaluatorFactoryDictionary = InitEvaluatorFactoryDictionary();

        public static void PatchAfterSetFilter([NotNull] this AgentSlot instance,
            AgentState state,
            [CanBeNull] GameManager currentGameManager,
            [CanBeNull] IBeautyBeastAnimTestAdapter beautyBeastAnimTestAdapter = null,
            [CanBeNull] IImageTestAdapter imageTestAdapter = null,
            [CanBeNull] ITextTestAdapter textTestAdapter = null,
            [CanBeNull] IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter = null)
        {
            Guard.Against.Null(instance, nameof(instance));

            if (!currentGameManager.IsValidGameStage(state))
            {
                return;
            }

            var agentWillDie = instance.CheckIfWorkWillKillAgent(CommandWindow.CommandWindow.CurrentWindow, s_evaluatorFactoryDictionary, beautyBeastAnimTestAdapter, yggdrasilAnimTestAdapter);

            if (!agentWillDie)
            {
                return;
            }

            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
            var slotColor = commandWindow.DeadColor;
            var slotText = LocalizeTextDataModel.instance.GetText("AgentState_Dead");
            instance.UpdateAgentSlot(slotColor, slotText, imageTestAdapter, textTestAdapter);
        }

        /// <summary>Stores our evaluators in a dictionary of factories so that we only need to create the dictionary once but can make evaluators from the factories as often as we need to.</summary>
        /// <returns></returns>
        [NotNull]
        private static Dictionary<CreatureIds, Func<CreatureEvaluatorParameters, ICreatureEvaluator>> InitEvaluatorFactoryDictionary()
        {
            return new Dictionary<CreatureIds, Func<CreatureEvaluatorParameters, ICreatureEvaluator>>
            {
                {
                    CreatureIds.BeautyAndTheBeast, parameters => new BeautyAndTheBeastEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType, parameters.BeautyBeastAnimTestAdapter)
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
                    CreatureIds.ParasiteTree, parameters => new ParasiteTreeEvaluator(parameters.Agent, parameters.Creature, parameters.SkillType, parameters.YggdrasilAnimTestAdapter)
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

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable InconsistentNaming
        public static void Postfix([NotNull] AgentSlot __instance,
            AgentState state)
        {
            try
            {
                var currentGameManager = GameManager.currentGameManager;
                __instance.PatchAfterSetFilter(state, currentGameManager);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
        // ReSharper enable InconsistentNaming
    }
}
