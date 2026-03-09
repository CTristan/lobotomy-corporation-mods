// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    public sealed class AgentWorkTracker : IAgentWorkTracker
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        // We load the FileManager later when applying the patch, so this will be null in the constructor
        private readonly IFileManager _fileManager;
        private readonly List<IGift> _gifts = new List<IGift>();
        private readonly Dictionary<string, long> _mostRecentAgentIdByGift = new Dictionary<string, long>();
        private readonly string _trackerFile = string.Empty;

        public AgentWorkTracker([CanBeNull] IFileManager fileManager,
            string dataFileName)
        {
            if (fileManager.IsNull())
            {
                return;
            }

            _fileManager = fileManager;
            _trackerFile = _fileManager.GetFile(dataFileName);
            Load();
        }

        public float GetLastAgentWorkCountByGift([NotNull] string giftName)
        {
            // Make sure this gift has actually been worked on before doing lookups
            if (!_mostRecentAgentIdByGift.TryGetValue(giftName, out long value))
            {
                return 0;
            }

            IAgent agent = GetAgent(giftName, value);

            return agent.GetWorkCount();
        }

        public void IncrementAgentWorkCount([NotNull] string giftName,
            long agentId)
        {
            IncrementAgentWorkCount(giftName, agentId, 1f);
        }

        public void IncrementAgentWorkCount([NotNull] string giftName,
            long agentId,
            float numberOfTimes)
        {
            IAgent agent = GetAgent(giftName, agentId);
            agent.IncrementWorkCount(numberOfTimes);
            _mostRecentAgentIdByGift[giftName] = agent.GetId();
        }

        public void Load()
        {
            LoadFromString(_fileManager.ReadAllText(_trackerFile, true));
        }

        public void Reset()
        {
            _gifts.Clear();
            _mostRecentAgentIdByGift.Clear();
            Save();
        }

        public void Save()
        {
            _fileManager.WriteAllText(_trackerFile, ToString());
        }

        private IAgent GetAgent(string giftName,
            long agentId)
        {
            IGift gift = _gifts.Find(g => g.GetName().Equals(giftName, StringComparison.Ordinal));
            if (gift.IsNull())
            {
                gift = CreateAndAddGift(giftName);
            }

            return gift.GetOrAddAgent(agentId);
        }

        [NotNull]
        private Gift CreateAndAddGift(string giftName)
        {
            Gift gift = new Gift(giftName);
            _gifts.Add(gift);

            return gift;
        }

        /// <summary>Loads the tracker data from our custom text file.</summary>
        private void LoadFromString([NotNull] string trackerData)
        {
            // Clear any existing data so that we aren't duplicating work progress
            _gifts.Clear();
            _mostRecentAgentIdByGift.Clear();

            string[] gifts = trackerData.Split('|');
            foreach (string gift in gifts)
            {
                string[] giftData = gift.Split('^');
                string giftName = giftData[0];
                for (int i = 1; i < giftData.Length; i++)
                {
                    string[] agentData = giftData[i].Split(';');
                    IncrementAgentWorkCount(giftName, long.Parse(agentData[0], CultureInfo.InvariantCulture), float.Parse(agentData[1], CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        ///     Converts the AgentWorkTracker object to a custom string format. The format delimits gifts by '|', agents for each gift by '^', and agent id and work count are separated
        ///     by ';'. A gift can have multiple agents, and we don't duplicate the gift names. Example: (gift1)^(agent1);(work-count1)^(agent2);(work-count2)|(gift2)^(agent1);(work-count2) I
        ///     would have preferred to use json, but the Unity json support is very minimal and does not support nested objects, so I had to make my own format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < _gifts.Count; i++)
            {
                IGift gift = _gifts[i];

                if (i > 0)
                {
                    _ = builder.Append('|');
                }

                string agentData = GetGiftAgentData(gift);
                string giftData = $"{gift.GetName()}{agentData}";

                _ = builder.Append(giftData);
            }

            return builder.ToString();
        }

        [NotNull]
        private static string GetGiftAgentData([NotNull] IGift gift)
        {
            StringBuilder agentDataBuilder = new StringBuilder();

            foreach (string agentData in gift.GetAgents().Select(agent => $"^{agent.GetId()};{agent.GetWorkCount().ToString(CultureInfo.InvariantCulture)}"))
            {
                _ = agentDataBuilder.Append(agentData);
            }

            return agentDataBuilder.ToString();
        }
    }
}
