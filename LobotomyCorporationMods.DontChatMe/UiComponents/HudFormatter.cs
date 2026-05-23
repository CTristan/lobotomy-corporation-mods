// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Globalization;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     Pure formatter: turns a <see cref="HudSnapshot" /> into the rich-text string the overlay
    ///     renders. Returns <c>null</c> when the overlay should not be drawn.
    ///     Color tags use Unity's IMGUI rich-text format (<c>&lt;color=#rrggbb&gt;</c>); GUIStyle must
    ///     have <c>richText = true</c> for them to render.
    /// </summary>
    public static class HudFormatter
    {
        private const string ColorDisabled = "#888888";
        private const string ColorDisconnected = "#ff5555";
        private const string ColorConnecting = "#ffcc55";
        private const string ColorConnected = "#55ff77";

        public static string Format(HudSnapshot snapshot)
        {
            ThrowHelper.ThrowIfNull(snapshot, nameof(snapshot));
            if (!snapshot.Visible)
            {
                return null;
            }

            var parts = new List<string>();
            parts.Add(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "DCM <color={0}>●</color> {1}",
                    ColorForState(snapshot.State),
                    LabelForState(snapshot.State)
                )
            );

            if (snapshot.QueuedCount > 0)
            {
                parts.Add(
                    string.Format(CultureInfo.InvariantCulture, "{0} queued", snapshot.QueuedCount)
                );
            }

            if (!string.IsNullOrEmpty(snapshot.LastEffectSlug))
            {
                parts.Add("last: " + snapshot.LastEffectSlug);
            }

            return string.Join(" · ", parts.ToArray());
        }

        private static string ColorForState(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Disabled:
                    return ColorDisabled;
                case ConnectionState.Disconnected:
                    return ColorDisconnected;
                case ConnectionState.Connecting:
                    return ColorConnecting;
                case ConnectionState.Connected:
                    return ColorConnected;
                default:
                    return ColorDisabled;
            }
        }

        private static string LabelForState(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Disabled:
                    return "disabled";
                case ConnectionState.Disconnected:
                    return "disconnected";
                case ConnectionState.Connecting:
                    return "connecting…";
                case ConnectionState.Connected:
                    return "connected";
                default:
                    return "unknown";
            }
        }
    }
}
