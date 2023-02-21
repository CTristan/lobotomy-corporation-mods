// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    public sealed class AgentWorkTracker : IAgentWorkTracker
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        // We load the FileManager later when applying the patch, so this will be null in the constructor
        private readonly IFileManager _fileManager = default!;
        private readonly List<IGift> _gifts = new();
        private readonly Dictionary<string, long> _mostRecentAgentIdByGift = new();
        private readonly string _trackerFile = string.Empty;

        public AgentWorkTracker(IFileManager? fileManager, string dataFileName)
        {
            if (fileManager is not null)
            {
                _fileManager = fileManager;
                _trackerFile = _fileManager.GetOrCreateFile(dataFileName);
                Load();
            }
        }

        public float GetLastAgentWorkCountByGift(string giftName)
        {
            // Make sure this gift has actually been worked on before doing lookups
            if (!_mostRecentAgentIdByGift.ContainsKey(giftName))
            {
                return 0;
            }

            var agentId = _mostRecentAgentIdByGift[giftName];
            var agent = GetAgent(giftName, agentId);

            return agent.GetWorkCount();
        }

        public void IncrementAgentWorkCount(string giftName, long agentId)
        {
            IncrementAgentWorkCount(giftName, agentId, 1f);
        }

        public void IncrementAgentWorkCount(string giftName, long agentId, float numberOfTimes)
        {
            var agent = GetAgent(giftName, agentId);
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

        private IAgent GetAgent(string giftName, long agentId)
        {
            var gift = _gifts.FirstOrDefault(g => g is not null && g.GetName().Equals(giftName, StringComparison.Ordinal));
            if (gift is not null)
            {
                return gift.GetOrAddAgent(agentId);
            }

            // Gift not found, start tracking the gift
            gift = new Gift(giftName);
            _gifts.Add(gift);

            return gift.GetOrAddAgent(agentId);
        }

        /// <summary>
        ///     Loads the tracker data from our custom text file.
        /// </summary>
        private void LoadFromString(string trackerData)
        {
            // Clear any existing data so we aren't duplicating work progress
            _gifts.Clear();
            _mostRecentAgentIdByGift.Clear();

            var gifts = trackerData.Split('|');
            foreach (var gift in gifts)
            {
                var giftData = gift.Split('^');
                var giftName = giftData[0];
                for (var i = 1; i < giftData.Length; i++)
                {
                    var agentData = giftData[i].Split(';');
                    IncrementAgentWorkCount(giftName, long.Parse(agentData[0], CultureInfo.InvariantCulture), float.Parse(agentData[1], CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        ///     Converts the AgentWorkTracker object to a custom string format. The format delimits gifts by '|', agents for each
        ///     gift by '^', and agent id and work count are separated by ';'. A gift can have multiple agents and we don't
        ///     duplicate the gift names.
        ///     Example: (gift1)^(agent1);(work-count1)^(agent2);(work-count2)|(gift2)^(agent1);(work-count2)
        ///     I would have preferred to use json, but the Unity json support is very minimal and does not support nested
        ///     objects, so I had to make my own format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < _gifts.Count; i++)
            {
                var gift = _gifts[i];
                if (i > 0)
                {
                    builder.Append('|');
                }

                builder.Append(gift.GetName());
                foreach (var agent in gift.GetAgents())
                {
                    builder.Append("^" + agent?.GetId() + ";" + agent?.GetWorkCount().ToString(CultureInfo.InvariantCulture));
                }
            }

            return builder.ToString();
        }
    }
}
