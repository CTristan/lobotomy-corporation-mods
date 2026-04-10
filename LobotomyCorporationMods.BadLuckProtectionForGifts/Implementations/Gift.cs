// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common.Extensions;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    internal sealed class Gift : IGift
    {
        private readonly Dictionary<long, IAgent> _agents = new Dictionary<long, IAgent>();
        private readonly string _name;

        internal Gift(string giftName)
        {
            _name = giftName;
        }

        public IEnumerable<IAgent> GetAgents()
        {
            return _agents.Values;
        }

        public string GetName()
        {
            return _name;
        }

        /// <summary>If an agent exists then returns that agent, otherwise creates a new agent and adds it to the dictionary.</summary>
        /// <param name="agentId">Agent Id</param>
        /// <returns>A new or existing agent.</returns>
        [NotNull]
        public IAgent GetOrAddAgent(long agentId)
        {
            if (_agents.TryGetValue(agentId, out var existingAgent))
            {
                return existingAgent;
            }

            var agent = new Agent(agentId);
            _agents[agentId] = agent;

            return agent;
        }
    }
}
