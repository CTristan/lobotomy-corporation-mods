// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace LobotomyCorporationMods.Playwright.JsonModels
{
    /// <summary>
    /// Data container for screenshot responses.
    /// Uses lowercase fields for JsonUtility compatibility.
    /// </summary>
    [Serializable]
    public sealed class ScreenshotData
    {
        public string filename;
        public string path;
        public long size;
        public string timestamp;
        public string format;
        public string note;
        public string base64;
    }
}
