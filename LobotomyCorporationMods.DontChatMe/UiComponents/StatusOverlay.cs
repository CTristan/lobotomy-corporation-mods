// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     IMGUI overlay anchored middle-top. Reads <see cref="HudState" /> on each repaint and
    ///     renders the current connection state, queued count, and last-effect slug. Press
    ///     <c>F8</c> to toggle visibility without disconnecting. A gear button to the right of
    ///     the label opens the Settings window; the gear is hidden along with the rest of the
    ///     overlay when the user presses F8.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class StatusOverlay : MonoBehaviour
    {
        private const float TopMargin = 8f;
        private const float HorizontalPadding = 14f;
        private const float VerticalPadding = 6f;
        private const float GearWidth = 28f;
        private const float GearGap = 4f;
        private const int FontSize = 14;

        private HudState _state;
        private SettingsState _settingsState;
        private IDontChatMeConfig _config;
        private GUIStyle _labelStyle;

        public static StatusOverlay Attach(
            HudState state,
            SettingsState settingsState,
            IDontChatMeConfig config
        )
        {
            ThrowHelper.ThrowIfNull(state, nameof(state));
            ThrowHelper.ThrowIfNull(settingsState, nameof(settingsState));
            ThrowHelper.ThrowIfNull(config, nameof(config));

            var go = new GameObject("DontChatMe.StatusOverlay");
            DontDestroyOnLoad(go);
            var overlay = go.AddComponent<StatusOverlay>();
            overlay._state = state;
            overlay._settingsState = settingsState;
            overlay._config = config;
            return overlay;
        }

        private void Update()
        {
            if (_state == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                _state.ToggleVisibility();
            }
        }

        private void OnGUI()
        {
            if (_state == null)
            {
                return;
            }

            var snapshot = _state.Snapshot;
            var text = HudFormatter.Format(snapshot);
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            EnsureStyles();

            var content = new GUIContent(text);
            var size = _labelStyle.CalcSize(content);
            var width = size.x + (HorizontalPadding * 2f);
            var height = size.y + (VerticalPadding * 2f);
            var totalWidth = width + GearGap + GearWidth;
            var rect = new Rect((Screen.width - totalWidth) / 2f, TopMargin, width, height);

            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;

            GUI.Label(rect, content, _labelStyle);

            var gearRect = new Rect(rect.xMax + GearGap, rect.y, GearWidth, height);
            if (GUI.Button(gearRect, "⚙"))
            {
                _settingsState.Open(SettingsDraft.FromConfig(_config));
            }
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = FontSize,
                alignment = TextAnchor.MiddleCenter,
            };
            _labelStyle.normal.textColor = Color.white;
        }
    }
}
