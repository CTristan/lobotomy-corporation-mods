using System.Collections.Generic;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    internal sealed class Gift
    {
        internal Gift([NotNull] string giftName)
        {
            Agents = new List<Agent>();
            Name = giftName;
        }

        [NotNull] internal List<Agent> Agents { get; }
        [NotNull] internal string Name { get; }

        /// <summary>
        ///     If an agent exists then returns that agent, otherwise creates a new agent and adds it to the list.
        /// </summary>
        /// <param name="agentId">Agent Id</param>
        /// <returns>A new or existing agent.</returns>
        [NotNull]
        public Agent GetOrAddAgent(long agentId)
        {
            var agent = Agents.Find(a => a?.Id == agentId);
            if (agent != null)
            {
                return agent;
            }

            agent = new Agent(agentId);
            Agents.Add(agent);
            return agent;
        }
    }
}
