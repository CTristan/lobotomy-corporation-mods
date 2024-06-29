// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class CommandWindowExtensions
    {
        /// <summary>Retrieves the creature evaluator for the given command window, agent, beautyBeastAnimAdapter, and yggdrasilAnimAdapter.</summary>
        /// <param name="commandWindow">The command window from which to retrieve the creature evaluator.</param>
        /// <param name="agent">The agent model.</param>
        /// <param name="evaluators">The evaluators dictionary.</param>
        /// <param name="beautyBeastAnimAdapter">The beautyBeastAnimAdapter.</param>
        /// <param name="yggdrasilAnimAdapter">The yggdrasilAnimAdapter.</param>
        /// <returns>The creature evaluator for the given parameters.</returns>
        /// <remarks>
        ///     GetCreatureEvaluator retrieves the creature evaluator based on the command window, agent, evaluators, beautyBeastAnimAdapter, and yggdrasilAnimAdapter parameters. If the
        ///     command window contains a creature, the evaluator will be retrieved from the evaluators dictionary using the creature's ID. If the command window does not contain a creature
        ///     or it.IsNull(), the evaluator will be set to null.
        /// </remarks>
        internal static ICreatureEvaluator GetCreatureEvaluator([NotNull] this CommandWindow.CommandWindow commandWindow,
            AgentModel agent,
            Dictionary<CreatureIds, Func<CreatureEvaluatorParameters, ICreatureEvaluator>> evaluators,
            IBeautyBeastAnimAdapter beautyBeastAnimAdapter,
            IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            ICreatureEvaluator evaluator;

            if (commandWindow.TryGetCreature(out var creature) && !creature.IsNull())
            {
                var skillType = commandWindow.CurrentSkill.rwbpType;
                var evaluatorParameters = new CreatureEvaluatorParameters(agent, creature, skillType, beautyBeastAnimAdapter, yggdrasilAnimAdapter);

                evaluator = evaluators.TryGetValue((CreatureIds)creature.metadataId, out var factoryMethod) ? factoryMethod(evaluatorParameters) : new DefaultEvaluator(agent, creature, skillType);
            }
            else
            {
                evaluator = new NoneEvaluator();
            }

            return evaluator;
        }

        /// <summary>
        ///     Determines whether the current CommandWindow is an abnormality work window. An abnormality work window is a CommandWindow in the Management phase with a non-null rwbpType
        ///     in the CurrentSkill property.
        /// </summary>
        /// <param name="commandWindow">The current CommandWindow to check.</param>
        /// <returns><c>true</c> if the current CommandWindow is an abnormality work window, otherwise <c>false</c>.</returns>
        internal static bool IsAbnormalityWorkWindow([NotNull] this CommandWindow.CommandWindow commandWindow)
        {
            // Validation checks to confirm we have everything we need
            var isAbnormalityWorkWindow = commandWindow.CurrentSkill.IsNotNull() && commandWindow.CurrentWindowType == CommandType.Management;

            return isAbnormalityWorkWindow;
        }

        private static bool TryGetCreature([NotNull] this CommandWindow.CommandWindow commandWindow,
            [CanBeNull] out CreatureModel creature)
        {
            creature = null;

            var unitModel = commandWindow.CurrentTarget;
            if (unitModel is CreatureModel creatureModel)
            {
                creature = creatureModel;
            }

            return !creature.IsNull();
        }
    }
}
