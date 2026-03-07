// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;

namespace LobotomyPlaywright.Protocol
{
    /// <summary>
    /// Data container for screenshot responses.
    /// Uses lowercase fields for JsonUtility compatibility.
    /// </summary>
    [Serializable]
    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Required for Unity's JsonUtility")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Required for JSON serialization")]
    [SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "Required for JSON serialization")]
    public class ScreenshotData
    {
        public string filename;
        public string path;
        public long size;
        public string timestamp;
        public string format;
        public string note;
    }
}
