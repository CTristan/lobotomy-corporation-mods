using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    internal sealed class Gift : IGift
    {
        internal Gift([NotNull] string giftName)
        {
            _name = giftName;
        }

        [NotNull] private readonly List<IAgent> _agents = new List<IAgent>();
        [NotNull] private readonly string _name;

        [NotNull]
        public string GetName()
        {
            return _name;
        }

        [NotNull]
        public List<IAgent> GetAgents()
        {
            return _agents;
        }

        /// <summary>
        ///     If an agent exists then returns that agent, otherwise creates a new agent and adds it to the list.
        /// </summary>
        /// <param name="agentId">Agent Id</param>
        /// <returns>A new or existing agent.</returns>
        [NotNull]
        public IAgent GetOrAddAgent(long agentId)
        {
            var agent = _agents.Find(a => a?.GetId() == agentId);
            if (agent != null)
            {
                return agent;
            }

            agent = new Agent(agentId);
            _agents.Add(agent);
            return agent;
        }
    }
}
