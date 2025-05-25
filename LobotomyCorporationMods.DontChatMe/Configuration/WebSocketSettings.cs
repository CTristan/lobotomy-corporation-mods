// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.DontChatMe.Configuration
{
    public static class WebSocketSettings
    {
        private const string Key = nameof(WebSocketSettings) + ".";

        public static string ServerPath
        {
            get
                => PlayerPrefs.GetString(Key + nameof(ServerPath), "ws://localhost:8080/");
            set
            {
                PlayerPrefs.SetString(Key + nameof(ServerPath), value);
                PlayerPrefs.Save();
            }
        }
    }
}
