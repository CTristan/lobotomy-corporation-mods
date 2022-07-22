using System.Diagnostics.CodeAnalysis;
using Inventory;
using UnityEngine;

#pragma warning disable CA1707
#pragma warning disable CA1822
namespace LobotomyCorporationMods.AutoEquipTeam
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class AutoEquipButton : MonoBehaviour
    {
        public void OnClick(InventoryUI __instance)
        {
            Harmony_Patch.EquipAllAgents();
        }
    }
}
#pragma warning restore CA1707
#pragma warning restore CA1822
