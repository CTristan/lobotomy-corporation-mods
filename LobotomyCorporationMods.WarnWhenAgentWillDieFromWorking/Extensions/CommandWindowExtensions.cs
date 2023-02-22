// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class CommandWindowExtensions
    {
        internal static ICreatureEvaluator GetCreatureEvaluator(this CommandWindow.CommandWindow commandWindow, AgentModel agent, IBeautyBeastAnimAdapter beautyBeastAnimAdapter,
            IGameObjectAdapter gameObjectAdapter)
        {
            ICreatureEvaluator evaluator;

            // Make sure we actually have an abnormality in our work window
            if (commandWindow.TryGetCreature(out var creature) && creature is not null)
            {
                // Need to use the command window's skill type since the agent isn't using a skill yet
                var skillType = commandWindow.CurrentSkill.rwbpType;

                var evaluatorDictionary = new Dictionary<CreatureIds, CreatureEvaluator>
                {
                    { CreatureIds.BeautyAndTheBeast, new BeautyAndTheBeastEvaluator(agent, creature, skillType, beautyBeastAnimAdapter) },
                    { CreatureIds.Bloodbath, new BloodbathEvaluator(agent, creature, skillType) },
                    { CreatureIds.BlueStar, new BlueStarEvaluator(agent, creature, skillType) },
                    { CreatureIds.CrumblingArmor, new CrumblingArmorEvaluator(agent, creature, skillType) },
                    { CreatureIds.HappyTeddyBear, new HappyTeddyBearEvaluator(agent, creature, skillType) },
                    { CreatureIds.NothingThere, new NothingThereEvaluator(agent, creature, skillType) },
                    { CreatureIds.ParasiteTree, new ParasiteTreeEvaluator(agent, creature, skillType, gameObjectAdapter) },
                    { CreatureIds.RedShoes, new RedShoesEvaluator(agent, creature, skillType) },
                    { CreatureIds.SingingMachine, new SingingMachineEvaluator(agent, creature, skillType) },
                    { CreatureIds.SpiderBud, new SpiderBudEvaluator(agent, creature, skillType) },
                    { CreatureIds.VoidDream, new VoidDreamEvaluator(agent, creature, skillType) },
                    { CreatureIds.WarmHeartedWoodsman, new WarmHeartedWoodsmanEvaluator(agent, creature, skillType) }
                };

                evaluator = evaluatorDictionary.TryGetValue((CreatureIds)creature.metadataId, out var concreteEvaluator) ? concreteEvaluator : new DefaultEvaluator(agent, creature, skillType);
            }
            else
            {
                evaluator = new NoneEvaluator();
            }

            return evaluator;
        }

        internal static bool IsAbnormalityWorkWindow(this CommandWindow.CommandWindow commandWindow)
        {
            var isAbnormalityWorkWindow = commandWindow.CurrentSkill?.rwbpType is not null && commandWindow.CurrentWindowType == CommandType.Management;

            // Validation checks to confirm we have everything we need

            return isAbnormalityWorkWindow;
        }

        private static bool TryGetCreature(this CommandWindow.CommandWindow commandWindow, out CreatureModel? creature)
        {
            creature = null;

            var unitModel = commandWindow.CurrentTarget;
            if (unitModel is CreatureModel creatureModel)
            {
                creature = creatureModel;
            }

            return creature is not null;
        }
    }
}
