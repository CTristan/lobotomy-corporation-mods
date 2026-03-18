// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace Hemocode.Playwright.JsonModels
{
    /// <summary>
    /// Data model for title menu state.
    /// </summary>
    [Serializable]
    public sealed class TitleMenuData
    {
        public string currentScene;
        public bool hasSaveData;
        public bool hasUnlimitData;
        public int lastDay;
        public string currentLanguage;
        public string buildVersion;
    }
}
