// SPDX-License-Identifier:MIT

#region

using LobotomyCorporationMods.Common.UiComponents;
using LobotomyCorporationMods.DontChatMe.Configuration;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    public class WebSocketSettingsUi : MonoBehaviour
    {
        private void Start()
        {
            // var root = UiFactory.CreateVerticalGroup(transform, nameof(WebSocketSettingsUi));

            UiFactory.CreateLabeledInputField(
                transform,
                "Server URL",
                WebSocketSettings.ServerPath,
                "wss://localhost:8080");

            Harmony_Patch.Instance.Logger.Log("WebSocketSettingsUi initialized.");
        }
    }
}
