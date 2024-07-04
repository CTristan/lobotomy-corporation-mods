// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Extensions;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    internal sealed class Gift : IGift
    {
        private readonly List<IAgent> _agents = new List<IAgent>();
        private readonly string _name;

        internal Gift(string giftName)
        {
            _name = giftName;
        }

        public List<IAgent> GetAgents()
        {
            return _agents;
        }

        public string GetName()
        {
            return _name;
        }

        /// <summary>If an agent exists then returns that agent, otherwise creates a new agent and adds it to the list.</summary>
        /// <param name="agentId">Agent Id</param>
        /// <returns>A new or existing agent.</returns>
        [NotNull]
        public IAgent GetOrAddAgent(long agentId)
        {
            var agent = _agents.Find(a => a.GetId() == agentId);

            if (agent.IsNotNull())
            {
                return agent;
            }

            agent = new Agent(agentId);
            _agents.Add(agent);

            return agent;
        }
    }
}
