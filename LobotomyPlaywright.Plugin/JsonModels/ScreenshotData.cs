// SPDX-License-Identifier: MIT

using System;

namespace LobotomyPlaywright.JsonModels
{
    /// <summary>
    /// Data container for screenshot responses.
    /// Uses lowercase fields for JsonUtility compatibility.
    /// </summary>
    [Serializable]
    public class ScreenshotData
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
