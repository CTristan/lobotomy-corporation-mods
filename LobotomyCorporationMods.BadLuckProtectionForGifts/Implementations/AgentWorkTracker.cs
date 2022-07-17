using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

// ReSharper disable CommentTypo
namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    internal sealed class AgentWorkTracker : IAgentWorkTracker
    {
        [NotNull] private readonly List<IGift> _gifts = new List<IGift>();
        private readonly Dictionary<string, long> _mostRecentAgentIdByGift = new Dictionary<string, long>();

        /// <summary>
        ///     Loads the tracker data from our custom text file.
        /// </summary>
        /// <param name="trackerData">The contents of our text file.</param>
        /// <returns>Loaded AgentWorkTracker object.</returns>
        [NotNull]
        public IAgentWorkTracker FromString(string trackerData)
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
                    tracker.IncrementAgentWorkCount(giftName, long.Parse(agentData[0], CultureInfo.InvariantCulture),
                        float.Parse(agentData[1], CultureInfo.InvariantCulture));
                }
            }

            return tracker;
        }

        public float GetLastAgentWorkCountByGift(string giftName)
        {
            // Make sure this gift has actually been worked on before doing lookups
            if (!_mostRecentAgentIdByGift.ContainsKey(giftName)) { return 0; }

            var agentId = _mostRecentAgentIdByGift[giftName];
            var agent = GetAgent(giftName, agentId);
            return agent.GetWorkCount();
        }

        public void IncrementAgentWorkCount(string giftName, long agentId, float numberOfTimes = 1f)
        {
            var agent = GetAgent(giftName, agentId);
            agent.IncrementWorkCount(numberOfTimes);
            _mostRecentAgentIdByGift[giftName] = agent.GetId();
        }

        [NotNull]
        private IAgent GetAgent([NotNull] string giftName, long agentId)
        {
            var gift = _gifts.FirstOrDefault(g => g?.GetName().Equals(giftName, StringComparison.Ordinal) == true);
            if (gift != null)
            {
                return gift.GetOrAddAgent(agentId);
            }

            // Gift not found, start tracking the gift
            gift = new Gift(giftName);
            _gifts.Add(gift);
            return gift.GetOrAddAgent(agentId);
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
            for (var i = 0; i < _gifts.Count; i++)
            {
                var gift = _gifts[i] ?? throw new InvalidOperationException(nameof(_gifts));
                if (i > 0)
                {
                    builder.Append('|');
                }

                builder.Append(gift.GetName());
                foreach (var agent in gift.GetAgents())
                {
                    builder.Append("^" + agent?.GetId() + ";" +
                                   agent?.GetWorkCount().ToString(CultureInfo.InvariantCulture));
                }
            }

            return builder.ToString();
        }
    }
}
