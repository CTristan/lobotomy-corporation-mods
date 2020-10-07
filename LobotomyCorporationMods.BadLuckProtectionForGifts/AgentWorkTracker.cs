using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    public sealed class AgentWorkTracker
    {
        public AgentWorkTracker()
        {
            Gifts = new List<Gift>();
        }

        private List<Gift> Gifts { get; }

        [CanBeNull]
        private Agent GetAgent(string giftName, long agentId)
        {
            var gift = Gifts.FirstOrDefault(g => g.Name.Equals(giftName));
            if (gift != null)
            {
                return gift.Agents.FirstOrDefault(a => a.Id.Equals(agentId));
            }

            // Gift not found, start tracking the gift
            Gifts.Add(new Gift(giftName));
            gift = Gifts.Find(g => g.Name.Equals(giftName));
            gift.Agents.Add(new Agent(agentId));
            return gift.Agents.Find(a => a.Id == agentId);
        }

        public float GetAgentWorkCount(string giftName, long agentId)
        {
            var agent = GetAgent(giftName, agentId);
            if (agent == null)
            {
                return 0;
            }

            return agent.WorkCount;
        }

        public void IncrementAgentWorkCount(string giftName, long agentId, float numberOfTimes = 1f)
        {
            var agent = GetAgent(giftName, agentId);
            if (agent != null)
            {
                agent.WorkCount += numberOfTimes;
            }
        }
    }
}
