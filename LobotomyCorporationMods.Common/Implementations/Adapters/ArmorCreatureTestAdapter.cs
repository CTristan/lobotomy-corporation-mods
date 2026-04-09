// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class ArmorCreatureTestAdapter
        : CreatureBaseTestAdapter<ArmorCreature>,
            IArmorCreatureTestAdapter
    {
        internal ArmorCreatureTestAdapter([NotNull] ArmorCreature gameObject)
            : base(gameObject) { }

        [CanBeNull]
        public IList SpecialAgentList
        {
            get
            {
                var fieldInfo = typeof(ArmorCreature).GetField(
                    "_specialAgentList",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                return fieldInfo?.GetValue(GameObject) as IList;
            }
        }

        /// <summary>Reloads the special agent list of the ArmorCreatureTestAdapter.</summary>
        /// <remarks>The internal list for CrumblingArmor uses a nested private class, so we need to use a lot of Reflection to make everything work.</remarks>
        public void ReloadSpecialAgentList()
        {
            if (SpecialAgentList == null)
            {
                return;
            }

            SpecialAgentList.Clear();
            foreach (
                var stackAgent in AgentManager
                    .instance.GetAgentList()
                    .Where(agent => agent.HasCrumblingArmor())
                    .Select(CreateStackAgent)
            )
            {
                SpecialAgentList.Add(stackAgent);
            }
        }

        private static object CreateStackAgent(object agent)
        {
            var stackAgentType = typeof(ArmorCreature).GetNestedType(
                "StackAgent",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            var stackAgent = Activator.CreateInstance(stackAgentType, true);
            var agentProperty = stackAgentType.GetProperty("agent");
            agentProperty?.SetValue(stackAgent, agent, null);

            return stackAgent;
        }
    }
}
