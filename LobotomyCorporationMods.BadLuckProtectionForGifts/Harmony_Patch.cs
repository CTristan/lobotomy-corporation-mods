using System;
using System.IO;
using Harmony;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    public class Harmony_Patch
    {
        public Harmony_Patch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("BadLuckProtectionForGifts");
            }
            catch (Exception ex)
            {
                File.WriteAllText(Application.dataPath + "/BaseMods/BadLuckProtectionForGifts_Log.txt",
                    ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
