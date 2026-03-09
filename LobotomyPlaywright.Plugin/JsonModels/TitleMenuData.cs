// SPDX-License-Identifier: MIT

using System;

namespace LobotomyPlaywright.JsonModels
{
    /// <summary>
    /// Data model for title menu state.
    /// </summary>
    [Serializable]
    public class TitleMenuData
    {
        public string currentScene;
        public bool hasSaveData;
        public bool hasUnlimitData;
        public int lastDay;
        public string currentLanguage;
        public string buildVersion;
    }
}
