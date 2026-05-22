// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     IMGUI overlay anchored middle-top. Reads <see cref="HudState" /> on each repaint and
    ///     renders the current connection state, queued count, and last-effect slug. Press
    ///     <c>F8</c> to toggle visibility without disconnecting.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class StatusOverlay : MonoBehaviour
    {
        private const float TopMargin = 8f;
        private const float HorizontalPadding = 14f;
        private const float VerticalPadding = 6f;
        private const int FontSize = 14;

        private HudState _state;
        private GUIStyle _labelStyle;

        public static StatusOverlay Attach(HudState state)
        {
            var go = new GameObject("DontChatMe.StatusOverlay");
            DontDestroyOnLoad(go);
            var overlay = go.AddComponent<StatusOverlay>();
            overlay._state = state;
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
            var rect = new Rect((Screen.width - width) / 2f, TopMargin, width, height);

            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;

            GUI.Label(rect, content, _labelStyle);
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
