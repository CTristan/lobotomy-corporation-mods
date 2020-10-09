using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
                var agent = gift.Agents.FirstOrDefault(a => a.Id.Equals(agentId));
                if (agent != null)
                {
                    return agent;
                }

                gift.Agents.Add(new Agent(agentId));
                return gift.Agents.Find(a => a.Id == agentId);
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

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < Gifts.Count; i++)
            {
                var gift = Gifts[i];
                if (i > 0)
                {
                    builder.Append('|');
                }

                builder.Append(gift.Name);
                foreach (var agent in gift.Agents)
                {
                    builder.Append("^" + agent.Id + ";" + agent.WorkCount.ToString(CultureInfo.InvariantCulture));
                }
            }

            return builder.ToString();
        }
    }
}
