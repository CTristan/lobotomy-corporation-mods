using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using Inventory;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.AutoEquipTeam
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.AutoEquipTeam.dll";
        private static IFileManager s_fileManager;

        // TODO: Verify the set names
        private static readonly List<string> s_validSets = new List<string>
        {
            // "Da Capo",
            // "Green Stem",
            // "Noise",
            // "Pink",
            // "Sanguine Desire"
            "Magic Bullet"
        };

        /// <summary>
        ///     Do not use for testing as it causes an exception. Use the other constructor instead.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public Harmony_Patch()
        {
            s_fileManager = new FileManager(ModFileName);
            InitializeHarmonyPatch();
        }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public Harmony_Patch(IFileManager fileManager)
        {
            s_fileManager = fileManager;
        }

        private static void CreateButtons([NotNull] InventoryUI ui)
        {
            var gameObject = new GameObject("AutoEquipButton");
            var image = gameObject.AddComponent<Image>();
            image.transform.SetParent(ui.gameObject.transform.GetChild(0));
            var button = gameObject.AddComponent<Button>();
            var autoEquipButton = gameObject.AddComponent<AutoEquipButton>();
            button.targetGraphic = image;
            button.onClick.AddListener(delegate
            {
                autoEquipButton.OnClick(ui);
            });
            gameObject.transform.localScale = new Vector3(1f, 1f);
            gameObject.transform.localPosition = new Vector3(-900f, 10f, -1f);
            gameObject.SetActive(true);
        }

        public static void EquipAllAgents()
        {
            var itemList = InventoryModel.Instance.GetAllEquipmentList();
            var armorList = SortItems(itemList.Where(item => item.metaInfo.type == EquipmentTypeInfo.EquipmentType.ARMOR),
                EquipmentTypeInfo.EquipmentType.ARMOR);
            var weaponList =
                SortItems(itemList.Where(item => item.metaInfo.type == EquipmentTypeInfo.EquipmentType.WEAPON),
                    EquipmentTypeInfo.EquipmentType.WEAPON);

            // First need to unequip all items to make sure every item is available
            UnequipAllItems(itemList);

            var inventoryWindow = InventoryUI.CurrentWindow;
            var agentsList = SortAgents(AgentManager.instance.GetAgentList());

            foreach (var agent in agentsList)
            {
                inventoryWindow.AgentController.SetAgent(agent);
                EquipAgent(inventoryWindow, agent, armorList, weaponList);
            }

            inventoryWindow.AgentController.SetAgent(null);
        }

        private static void EquipAgent([NotNull] InventoryUI inventoryWindow, AgentModel agent,
            [NotNull] ICollection<EquipmentModel> armorList, [NotNull] ICollection<EquipmentModel> weaponList)
        {
            var agentArmorList = armorList.Where(item => item.CheckRequire(agent)).ToList();
            var agentWeaponList = weaponList.Where(item => item.CheckRequire(agent)).ToList();

            // Equip the best weapon and armor the agent can equip based on the sort order
            // while keeping matched sets together
            if (agentArmorList.Count > 0)
            {
                EquipBestArmor(inventoryWindow, agent, armorList, agentArmorList);
            }

            if (agentWeaponList.Count > 0 && !TryEquipMatchingSet(inventoryWindow, agent, weaponList, agentWeaponList))
            {
                EquipBestWeapon(inventoryWindow, agent, weaponList, agentWeaponList);
            }
        }

        /// <summary>
        ///     Tries to equip the weapon matching the equipped armor if having a matching set grants a bonus.
        /// </summary>
        /// <param name="inventoryWindow"></param>
        /// <param name="agent"></param>
        /// <param name="armor"></param>
        /// <param name="weaponList"></param>
        /// <param name="agentWeaponList"></param>
        /// <returns></returns>
        private static bool TryEquipMatchingSet(InventoryUI inventoryWindow, [NotNull] AgentModel agent,
            ICollection<EquipmentModel> weaponList, [NotNull] ICollection<EquipmentModel> agentWeaponList)
        {
            var armorName = agent.Equipment?.armor?.metaInfo?.Name;
            var armorIsInSet = s_validSets.Contains(armorName, StringComparer.CurrentCultureIgnoreCase);
            if (armorName == null || !armorIsInSet)
            {
                return false;
            }

            var matchingWeapon = agentWeaponList.FirstOrDefault(weapon => weapon.metaInfo.Name == armorName);
            if (matchingWeapon == null) { return false; }

            inventoryWindow.ItemController.OnEquipAction(matchingWeapon, agent);
            weaponList.Remove(matchingWeapon);
            agentWeaponList.Clear();

            return true;
        }

        private static void EquipBestArmor([NotNull] InventoryUI inventoryWindow, AgentModel agent,
            [NotNull] ICollection<EquipmentModel> armorList, [NotNull] IEnumerable<EquipmentModel> agentArmorList)
        {
            var armor = agentArmorList.First();
            inventoryWindow.ItemController.OnEquipAction(armor, agent);
            armorList.Remove(armor);
        }

        private static void EquipBestWeapon([NotNull] InventoryUI inventoryWindow, AgentModel agent,
            [NotNull] ICollection<EquipmentModel> weaponList, [NotNull] IEnumerable<EquipmentModel> agentWeaponList)
        {
            var filteredList = agentWeaponList.Where(w => !s_validSets.Contains(w.metaInfo.Name)).ToList();

            if (filteredList.Count <= 0)
            {
                return;
            }

            var weapon = filteredList.FirstOrDefault();
            inventoryWindow.ItemController.OnEquipAction(weapon, agent);
            weaponList.Remove(weapon);
        }

        /// <summary>
        ///     Sorts agents by reverse team order, so that agents further down with the more difficult abnormalities get the
        ///     better equipment.
        /// </summary>
        /// <param name="agentList"></param>
        /// <returns></returns>
        [NotNull]
        private static IEnumerable<AgentModel> SortAgents([NotNull] ICollection<AgentModel> agentList)
        {
            // Remove any agents not assigned to a Sephirah
            var newList = agentList.Where(agent => agent.currentSefiraEnum != SefiraEnum.DUMMY).ToList();

            newList = newList.OrderByDescending(agent => agent.currentSefiraEnum).ToList();

            // Add back in the unassigned agents at the end
            var unassignedAgents = agentList.Where(agent => agent.currentSefiraEnum == SefiraEnum.DUMMY).ToList();
            newList.AddRange(unassignedAgents);

            return newList;
        }

        /// <summary>
        ///     Items are ordered by their Risk Level, then by the total combined stat value, then by the best stat for Pale,
        ///     Black, White, then Red stats.
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="equipmentType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [NotNull]
        private static ICollection<EquipmentModel> SortItems([NotNull] IEnumerable<EquipmentModel> itemList,
            EquipmentTypeInfo.EquipmentType equipmentType)
        {
            List<EquipmentModel> newList;

            switch (equipmentType)
            {
                case EquipmentTypeInfo.EquipmentType.ARMOR:
                    newList = itemList.OrderByDescending(armor => armor.metaInfo.grade)
                        .ThenBy(GetTotalArmorStatValue)
                        .ThenBy(armor => armor.metaInfo.defenseInfo.P)
                        .ThenBy(armor => armor.metaInfo.defenseInfo.B)
                        .ThenBy(armor => armor.metaInfo.defenseInfo.W)
                        .ThenBy(armor => armor.metaInfo.defenseInfo.R)
                        .ToList();
                    break;
                case EquipmentTypeInfo.EquipmentType.WEAPON:
                    newList = itemList.OrderByDescending(weapon => weapon.metaInfo.grade)
                        .ThenByDescending(weapon => weapon.metaInfo.damageInfo.type)
                        // Sorting by weapon stats last since otherwise damage type ordering won't matter since weapon stats vary so much
                        .ThenByDescending(GetAverageWeaponStats)
                        .ToList();
                    break;
                case EquipmentTypeInfo.EquipmentType.SPECIAL:
                default:
                    throw new ArgumentOutOfRangeException(nameof(equipmentType), equipmentType, null);
            }

            return newList;
        }

        private static float GetTotalArmorStatValue([NotNull] EquipmentModel item)
        {
            return item.metaInfo.defenseInfo.B + item.metaInfo.defenseInfo.P + item.metaInfo.defenseInfo.R +
                   item.metaInfo.defenseInfo.W;
        }

        private static float GetAverageWeaponStats([NotNull] EquipmentModel item)
        {
            var averageDamage = (item.metaInfo.damageInfo.min + item.metaInfo.damageInfo.max) / 2;
            var dps = averageDamage / item.metaInfo.attackSpeed;
            var rangeMultiplier = item.metaInfo.range;

            return dps * rangeMultiplier;
        }

        private static void UnequipAllItems([NotNull] IEnumerable<EquipmentModel> itemList)
        {
            var inventoryWindow = InventoryUI.CurrentWindow;

            foreach (var item in itemList)
            {
                var owner = (AgentModel)item.owner;
                if (owner == null) { continue; }

                inventoryWindow.ReleaseEquipment(item, owner);
            }
        }

        #region Harmony Overrides

        /// <summary>
        ///     Patches all of the relevant method calls through Harmony.
        /// </summary>
        private static void InitializeHarmonyPatch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("AutoEquipTeam");
                if (harmonyInstance == null)
                {
                    throw new InvalidOperationException(nameof(harmonyInstance));
                }

                var harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("AwakePostfix"));
                harmonyInstance.Patch(typeof(InventoryUI).GetMethod("Awake", AccessTools.all), null, harmonyMethod);
            }
            catch (Exception ex)
            {
                s_fileManager.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static void AwakePostfix([NotNull] InventoryUI __instance)
        {
            CreateButtons(__instance);
        }

        #endregion
    }
}
