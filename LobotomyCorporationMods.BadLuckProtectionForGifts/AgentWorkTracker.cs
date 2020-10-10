using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

// ReSharper disable CommentTypo
namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    public sealed class AgentWorkTracker
    {
        public AgentWorkTracker()
        {
            Gifts = new List<Gift>();
        }

        private List<Gift> Gifts { get; }

        /// <summary>
        ///     Loads the tracker data from our custom text file.
        /// </summary>
        /// <param name="trackerData">The contents of our text file.</param>
        /// <returns>Loaded AgentWorkTracker object.</returns>
        [NotNull]
        public static AgentWorkTracker FromString([NotNull] string trackerData)
        {
            var tracker = new AgentWorkTracker();
            var gifts = trackerData.Split('|');
            foreach (var gift in gifts)
            {
                var giftData = gift.Split('^');
                var giftName = giftData[0];
                for (var i = 1; i < giftData.Length; i++)
                {
                    var agentData = giftData[i].Split(';');
                    tracker.IncrementAgentWorkCount(giftName, long.Parse(agentData[0]), float.Parse(agentData[1]));
                }
            }

            return tracker;
        }

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

        /// <summary>
        ///     Converts the AgentWorkTracker object to a custom string format. The format delimits gifts by '|', agents for each
        ///     gift by '^', and agent id and work count are separated by ';'. A gift can have multiple agents and we don't
        ///     duplicate the gift names.
        ///     Example: (gift1)^(agent1);(workcount1)^(agent2);(workcount2)|(gift2)^(agent1);(workcount2)
        ///     I would have preferred to use json, but the Unity json support is very minimal and does not support nested
        ///     objects, so I had to make my own format.
        /// </summary>
        /// <returns></returns>
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
