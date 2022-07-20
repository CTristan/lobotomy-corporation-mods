using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using NSubstitute;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Test
{
    internal static class TestExtensions
    {
        [NotNull]
        public static AgentInfoWindow.WorkerPrimaryStatUI CreateAgentInfoWindow_WorkerPrimaryStatUI(RwbpType type)
        {
            var ui = Substitute.For<AgentInfoWindow.WorkerPrimaryStatUI>();
            ui.type = type;
            ui.StatName = Substitute.For<Text>();
            ui.list = new List<AgentInfoWindow.WorkerPrimaryStatUnit>
            {
                new AgentInfoWindow.WorkerPrimaryStatUnit { StatValue = Substitute.For<Text>() },
                new AgentInfoWindow.WorkerPrimaryStatUnit { StatValue = Substitute.For<Text>() }
            };

            return ui;
        }

        [NotNull]
        public static AgentModel CreateAgentModel(long id, int primaryStat = 1, int egoBonus = 0,
            float primaryStatExp = 0f)
        {
            var agent = CreateUninitializedObject<AgentModel>();
            agent.instanceId = id;
            agent.primaryStat = new WorkerPrimaryStat
            {
                battle = primaryStat, hp = primaryStat, mental = primaryStat, work = primaryStat
            };
            agent.primaryStatExp = new WorkerPrimaryStatExp
            {
                battle = primaryStatExp, hp = primaryStatExp, mental = primaryStatExp, work = primaryStatExp
            };


            var newAgent = CreateUninitializedObject<AgentModel>();
            var fields = GetUninitializedObjectFields(agent.GetType());
            var newValues = new Dictionary<string, object>
            {
                { "_statBufList", new List<UnitStatBuf>() },
                {
                    "_equipment", new UnitEquipSpace
                    {
                        gifts = new UnitEGOgiftSpace
                        {
                            addedGifts = new List<EGOgiftModel>
                            {
                                EGOgiftModel.MakeGift(new EquipmentTypeInfo
                                {
                                    bonus = new EGObonusInfo
                                    {
                                        // Red
                                        hp = egoBonus,

                                        // White
                                        mental = egoBonus,

                                        // Black
                                        cubeSpeed = egoBonus,
                                        workProb = egoBonus,

                                        // Purple
                                        attackSpeed = egoBonus
                                    }
                                })
                            }
                        }
                    }
                }
            };
            agent = GetPopulatedUninitializedObject(agent, fields, newValues);

            return agent;
        }

        [NotNull]
        public static CreatureEquipmentMakeInfo CreateCreatureEquipmentMakeInfo(string giftName)
        {
            var info = Substitute.For<CreatureEquipmentMakeInfo>();
            info.equipTypeInfo = new EquipmentTypeInfo
            {
                localizeData = new Dictionary<string, string> { { "name", giftName } },
                type = EquipmentTypeInfo.EquipmentType.SPECIAL
            };

            LocalizeTextDataModel.instance?.Init(new Dictionary<string, string> { { giftName, giftName } });

            return info;
        }

        [NotNull]
        public static UseSkill CreateUseSkill(string giftName, long agentId, int numberOfSuccesses)
        {
            var useSkill = Substitute.For<UseSkill>();
            useSkill.agent = CreateUninitializedObject<AgentModel>();
            useSkill.agent.instanceId = agentId;
            useSkill.targetCreature = CreateUninitializedObject<CreatureModel>();
            useSkill.targetCreature.metaInfo = new CreatureTypeInfo
            {
                equipMakeInfos = new List<CreatureEquipmentMakeInfo> { CreateCreatureEquipmentMakeInfo(giftName) }
            };
            useSkill.successCount = numberOfSuccesses;

            return useSkill;
        }

        #region Uninitialized Object Functions

        /// <summary>
        ///     Create an uninitialized object without calling a constructor. Needed because some of the classes we need
        ///     to mock either don't have a public constructor or cause a Unity exception.
        /// </summary>
        private static TObject CreateUninitializedObject<TObject>()
        {
            return (TObject)FormatterServices.GetSafeUninitializedObject(typeof(TObject));
        }

        /// <summary>
        ///     Get the fields for an uninitialized object. Can be used to later initialize the individual fields as needed.
        /// </summary>
        [NotNull]
        private static MemberInfo[] GetUninitializedObjectFields(Type type)
        {
            var fields = new List<MemberInfo>();

            while (type != typeof(object))
            {
                fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                               BindingFlags.DeclaredOnly));
                type = type?.BaseType;
            }

            return fields.ToArray();
        }

        /// <summary>
        ///     Populate the fields of an uninitialized object with a provided list of objects.
        /// </summary>
        [NotNull]
        private static TObject GetPopulatedUninitializedObject<TObject>(TObject obj, MemberInfo[] fields,
            Dictionary<string, object> newValues)
        {
            var newObj = CreateUninitializedObject<TObject>();
            var values = FormatterServices.GetObjectData(obj, fields.ToArray());

            for (var i = 0; i < fields.Length; i++)
            {
                if (newValues.ContainsKey(fields[i].Name))
                {
                    values[i] = newValues[fields[i].Name];
                }
            }

            FormatterServices.PopulateObjectMembers(newObj, fields.ToArray(), values);

            return newObj;
        }

        #endregion
    }
}
