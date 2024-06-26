// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Linq;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Test.Mods.WarnWhenAgentWillDieFromWorking
{
    public class WarnWhenAgentWillDieFromWorkingTests
    {
        private const string DeadAgentString = "AgentState_Dead";
        protected const int StatLevelFive = 85;
        protected const int StatLevelFour = 65;
        protected const int StatLevelOne = 1;
        protected const int StatLevelThree = 45;
        protected const int StatLevelTwo = 30;

        protected WarnWhenAgentWillDieFromWorkingTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);
        }

        protected Color DeadAgentColor { get; } = Color.red;

        internal bool AgentWillDie(IImageAdapter workFilterFill, ITextAdapter workFilterText)
        {
            var agentWillDie = workFilterFill.Color == DeadAgentColor && workFilterText.Text == DeadAgentString;

            return agentWillDie;
        }

        protected static AgentModel GetAgentWithGift(EquipmentId giftId, IEnumerable<UnitBuf> unitBuffs)
        {
            var agent = TestExtensions.CreateAgentModel(bufList: unitBuffs.ToList());
            var gift = TestExtensions.CreateEgoGiftModel();
            gift.metaInfo.id = (int)giftId;
            agent.Equipment.gifts.addedGifts.Add(gift);

            return agent;
        }

        protected static CreatureModel GetCreature(CreatureIds creatureId, int qliphothCounter)
        {
            var creature = TestExtensions.CreateCreatureModel(qliphothCounter: qliphothCounter);
            creature.instanceId = (long)creatureId;
            creature.metadataId = (long)creatureId;
            SetMaxObservation(creature);

            // Need to initialize the CreatureLayer with our new creature
            var creatureUnit = TestExtensions.CreateCreatureUnit();
            TestExtensions.CreateCreatureLayer(new Dictionary<long, CreatureUnit> { { (long)creatureId, creatureUnit } });

            return creature;
        }

        protected CommandWindow.CommandWindow InitializeCommandWindow(UnitModel currentTarget)
        {
            return InitializeCommandWindow(currentTarget, (RwbpType)1);
        }

        protected CommandWindow.CommandWindow InitializeCommandWindow(UnitModel currentTarget, RwbpType rwbpType)
        {
            // Need existing game instances
            InitializeLocalizeTextDataModel();
            InitializeSkillTypeList(rwbpType);

            var commandWindow = TestExtensions.CreateCommandWindow(currentTarget, CommandType.Management, (long)rwbpType);
            commandWindow.DeadColor = DeadAgentColor;
            CommandWindow.CommandWindow.CurrentWindow.DeadColor = DeadAgentColor;

            return commandWindow;
        }

        private static void InitializeLocalizeTextDataModel()
        {
            var list = new Dictionary<string, string> { { DeadAgentString, DeadAgentString } };

            _ = TestExtensions.CreateLocalizeTextDataModel(list);
        }

        private static void InitializeSkillTypeList(RwbpType rwbpType)
        {
            SkillTypeInfo[] skillTypeInfos = { new SkillTypeInfo { id = (long)rwbpType } };
            _ = TestExtensions.CreateSkillTypeList(skillTypeInfos);
        }

        private static void SetMaxObservation(CreatureModel creature)
        {
            var observeRegions = new List<ObserveInfoData>
            {
                new ObserveInfoData { regionName = "stat" },
                new ObserveInfoData { regionName = "defense" },
                new ObserveInfoData { regionName = "work_r" },
                new ObserveInfoData { regionName = "work_w" },
                new ObserveInfoData { regionName = "work_b" },
                new ObserveInfoData { regionName = "work_p" }
            };
            creature.observeInfo.InitObserveRegion(observeRegions);
            creature.observeInfo.ObserveAll();
        }
    }
}
