// SPDX-License-Identifier: MIT

#region

using CommandWindow;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions
{
    internal static class AgentSlotExtensions
    {
        internal static Image CreateGiftAvailabilityImage(this AgentSlot agentSlot)
        {
            const float localPositionX = -1f;
            const float localPositionY = 1f;
            const float localPositionZ = -1f;
            const float localScaleX = 0.5f;
            const float localScaleY = 0.5f;

            var gameObject = new GameObject();
            var image = gameObject.AddComponent<Image>();
            image.transform.SetParent(agentSlot.gameObject.transform.GetChild(0));
            gameObject.transform.localScale = new Vector3(localScaleX, localScaleY);
            gameObject.transform.localPosition = new Vector3(localPositionX, localPositionY, localPositionZ);
            gameObject.SetActive(true);

            return image;
        }
    }
}
