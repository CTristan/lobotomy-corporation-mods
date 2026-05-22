// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;
using LobotomyCorporationMods.DontChatMe.Constants;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     IMGUI window that lets the user configure the Connection-section settings without
    ///     LobCorp.ConfigurationManager. Opened by the gear button on the status overlay or
    ///     the F9 keybind. Server URL and Auth Token start masked on every open; the user
    ///     reveals each independently via a Show/Hide button.
    ///     All rendering and Unity-coupled behavior lives here; validation and the apply
    ///     orchestration live in <see cref="SettingsController" /> (testable).
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class SettingsWindow : MonoBehaviour
    {
        private const int WindowId = 0xDC0F;
        private const float WindowWidth = 420f;
        private const float WindowHeight = 240f;
        private const float FieldWidth = 280f;
        private const float RevealButtonWidth = 60f;
        private const char MaskChar = '*';

        private SettingsState _state;
        private SettingsController _controller;
        private IDontChatMeConfig _config;
        private Rect _windowRect;
        private bool _rectInitialized;
        private GUIStyle _errorStyle;

        public static SettingsWindow Attach(
            SettingsState state,
            SettingsController controller,
            IDontChatMeConfig config
        )
        {
            ThrowHelper.ThrowIfNull(state, nameof(state));
            ThrowHelper.ThrowIfNull(controller, nameof(controller));
            ThrowHelper.ThrowIfNull(config, nameof(config));

            var go = new GameObject("DontChatMe.SettingsWindow");
            DontDestroyOnLoad(go);
            var window = go.AddComponent<SettingsWindow>();
            window._state = state;
            window._controller = controller;
            window._config = config;
            return window;
        }

        private void Update()
        {
            if (_state == null || _config == null)
            {
                return;
            }

            if (!Input.GetKeyDown(KeyCode.F9))
            {
                return;
            }

            var snapshot = _state.Snapshot;
            if (snapshot.IsOpen)
            {
                _state.Close();
            }
            else
            {
                _state.Open(SettingsDraft.FromConfig(_config));
            }
        }

        private void OnGUI()
        {
            if (_state == null)
            {
                return;
            }

            var snapshot = _state.Snapshot;
            if (!snapshot.IsOpen)
            {
                return;
            }

            EnsureRect();

            var title = LocalizationIds.DisplaySettingsTitle.GetLocalized();
            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, title);
        }

        private void EnsureRect()
        {
            if (_rectInitialized)
            {
                return;
            }

            _windowRect = new Rect(
                (Screen.width - WindowWidth) / 2f,
                (Screen.height - WindowHeight) / 2f,
                WindowWidth,
                WindowHeight
            );
            _rectInitialized = true;
        }

        private void DrawWindow(int windowId)
        {
            var snapshot = _state.Snapshot;

            GUILayout.BeginVertical();

            GUILayout.Label(LocalizationIds.DisplayServerUrl.GetLocalized());
            var newServerUrl = DrawMaskableField(
                snapshot.Draft.ServerUrl,
                snapshot.IsServerUrlRevealed,
                SettingsField.ServerUrl
            );

            if (!string.IsNullOrEmpty(snapshot.LastError))
            {
                EnsureStyles();
                GUILayout.Label(snapshot.LastError, _errorStyle);
            }

            GUILayout.Space(6f);
            GUILayout.Label(LocalizationIds.DisplayAuthToken.GetLocalized());
            var newAuthToken = DrawMaskableField(
                snapshot.Draft.AuthToken,
                snapshot.IsAuthTokenRevealed,
                SettingsField.AuthToken
            );

            GUILayout.Space(6f);
            var newEnabled = GUILayout.Toggle(
                snapshot.Draft.Enabled,
                LocalizationIds.DisplayEnabled.GetLocalized()
            );

            if (
                newServerUrl != snapshot.Draft.ServerUrl
                || newAuthToken != snapshot.Draft.AuthToken
                || newEnabled != snapshot.Draft.Enabled
            )
            {
                _state.UpdateDraft(new SettingsDraft(newServerUrl, newAuthToken, newEnabled));
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(LocalizationIds.DisplayApply.GetLocalized()))
            {
                HandleApplyClicked();
            }

            if (GUILayout.Button(LocalizationIds.DisplayCancel.GetLocalized()))
            {
                _state.Close();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, WindowWidth, 20f));
        }

        private string DrawMaskableField(string value, bool isRevealed, SettingsField field)
        {
            GUILayout.BeginHorizontal();
            string updated;
            if (isRevealed)
            {
                updated = GUILayout.TextField(value, GUILayout.Width(FieldWidth));
            }
            else
            {
                updated = GUILayout.PasswordField(value, MaskChar, GUILayout.Width(FieldWidth));
            }

            var revealLabel = (
                isRevealed ? LocalizationIds.DisplayHide : LocalizationIds.DisplayShow
            ).GetLocalized();
            if (GUILayout.Button(revealLabel, GUILayout.Width(RevealButtonWidth)))
            {
                _state.ToggleReveal(field);
            }

            GUILayout.EndHorizontal();
            return updated;
        }

        private void HandleApplyClicked()
        {
            var snapshot = _state.Snapshot;
            var result = _controller.Apply(snapshot.Draft);
            switch (result)
            {
                case SettingsApplyResult.Ok:
                    _state.Close();
                    break;
                case SettingsApplyResult.InvalidServerUrl:
                    _state.SetError(LocalizationIds.ErrorInvalidServerUrl.GetLocalized());
                    break;
                case SettingsApplyResult.InvalidScheme:
                    _state.SetError(LocalizationIds.ErrorInvalidServerUrl.GetLocalized());
                    break;
            }
        }

        private void EnsureStyles()
        {
            if (_errorStyle != null)
            {
                return;
            }

            _errorStyle = new GUIStyle(GUI.skin.label);
            _errorStyle.normal.textColor = new Color(1f, 0.4f, 0.4f);
        }
    }
}
