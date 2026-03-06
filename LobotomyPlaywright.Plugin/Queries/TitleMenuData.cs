// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;

namespace LobotomyPlaywright.Queries
{
    /// <summary>
    /// Data model for title menu state.
    /// </summary>
    [Serializable]
    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
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
