// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;

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
        public bool hasCheckpointData;
        public bool hasUnlimitData;
        public int lastDay;
        public int checkpointDay;
        public string currentLanguage;
        public string buildVersion;
        public List<string> availableSaveTypes;
    }
}
