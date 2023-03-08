// SPDX-License-Identifier: MIT

#region

using System.IO;
using CommandWindow;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions
{
    internal static class ManagementSlotExtensions
    {
        internal static GameObject CreateGiftAvailabilityImage(this ManagementSlot managementSlot)
        {
            const float LocalPositionX = -12f;
            const float LocalPositionY = 28f;
            const float LocalPositionZ = -1f;
            const float LocalScaleX = 0.2f;
            const float LocalScaleY = 0.2f;

            var gameObject = new GameObject();
            gameObject.transform.SetParent(managementSlot.gameObject.transform.GetChild(0));
            gameObject.transform.localScale = new Vector3(LocalScaleX, LocalScaleY);
            gameObject.transform.localPosition = new Vector3(LocalPositionX, LocalPositionY, LocalPositionZ);
            gameObject.SetActive(true);

            var image = gameObject.AddComponent<Image>();
            var fileWithPath = Harmony_Patch.Instance.PublicFileManager.GetOrCreateFile("Assets/gift.png");
            var texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(File.ReadAllBytes(fileWithPath));
            var sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            image.sprite = sprite;

            var tooltip = image.gameObject.AddComponent<TooltipMouseOver>();
            tooltip.transform.SetParent(gameObject.transform.parent.parent);
            tooltip.ID = "Tooltip_AgentSlotExtensions@Testing";
            tooltip.SetDynamicTooltip("Should see this!");

            return gameObject;
        }

        internal static Image GetGiftAvailabilityImage(this ManagementSlot instance, GameObject slotImage)
        {
            return slotImage.GetComponent<Image>();
        }
    }
}
