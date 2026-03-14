// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public static class KeyCodeParser
    {
        private static readonly Dictionary<string, KeyCode> s_keyCodeMap = BuildKeyCodeMap();

        public static KeyCode Parse(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                return KeyCode.F9;
            }

            var trimmed = keyName.Trim();
            if (trimmed.Length == 0)
            {
                return KeyCode.F9;
            }

            foreach (var entry in s_keyCodeMap)
            {
                if (string.Equals(entry.Key, trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Value;
                }
            }

            return KeyCode.F9;
        }

        private static Dictionary<string, KeyCode> BuildKeyCodeMap()
        {
            var map = new Dictionary<string, KeyCode>(StringComparer.OrdinalIgnoreCase)
            {
                { "F1", KeyCode.F1 },
                { "F2", KeyCode.F2 },
                { "F3", KeyCode.F3 },
                { "F4", KeyCode.F4 },
                { "F5", KeyCode.F5 },
                { "F6", KeyCode.F6 },
                { "F7", KeyCode.F7 },
                { "F8", KeyCode.F8 },
                { "F9", KeyCode.F9 },
                { "F10", KeyCode.F10 },
                { "F11", KeyCode.F11 },
                { "F12", KeyCode.F12 },
                { "F13", KeyCode.F13 },
                { "F14", KeyCode.F14 },
                { "F15", KeyCode.F15 },
                { "Alpha0", KeyCode.Alpha0 },
                { "Alpha1", KeyCode.Alpha1 },
                { "Alpha2", KeyCode.Alpha2 },
                { "Alpha3", KeyCode.Alpha3 },
                { "Alpha4", KeyCode.Alpha4 },
                { "Alpha5", KeyCode.Alpha5 },
                { "Alpha6", KeyCode.Alpha6 },
                { "Alpha7", KeyCode.Alpha7 },
                { "Alpha8", KeyCode.Alpha8 },
                { "Alpha9", KeyCode.Alpha9 },
                { "Space", KeyCode.Space },
                { "Return", KeyCode.Return },
                { "Escape", KeyCode.Escape },
                { "Tab", KeyCode.Tab },
                { "Backspace", KeyCode.Backspace },
                { "Delete", KeyCode.Delete },
                { "Insert", KeyCode.Insert },
                { "Home", KeyCode.Home },
                { "End", KeyCode.End },
                { "PageUp", KeyCode.PageUp },
                { "PageDown", KeyCode.PageDown },
                { "UpArrow", KeyCode.UpArrow },
                { "DownArrow", KeyCode.DownArrow },
                { "LeftArrow", KeyCode.LeftArrow },
                { "RightArrow", KeyCode.RightArrow },
                { "BackQuote", KeyCode.BackQuote },
                { "Minus", KeyCode.Minus },
                { "Equals", KeyCode.Equals },
                { "LeftBracket", KeyCode.LeftBracket },
                { "RightBracket", KeyCode.RightBracket },
                { "Semicolon", KeyCode.Semicolon },
                { "Quote", KeyCode.Quote },
                { "Comma", KeyCode.Comma },
                { "Period", KeyCode.Period },
                { "Slash", KeyCode.Slash },
                { "Backslash", KeyCode.Backslash },
            };

            return map;
        }
    }
}
