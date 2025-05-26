// SPDX-License-Identifier:MIT

#region

using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.UiComponents;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    public class WebSocketUi : MonoBehaviour
    {
        private readonly IWebSocketClient _webSocketClient = Harmony_Patch.Instance.WebSocketClient;
        private Canvas _canvas;

        private Button _connectButton;

        private bool _isConnected;
        private string _lastMessage;
        private bool _settingsShown;
        private Text _statusText;

        private void Start()
        {
            EnsureEventSystem();

            // Create a persistent overlay canvas
            _canvas = UiFactory.CreateOverlayCanvas("DontChatMeOverlay");

            var panel = UiFactory.CreateLayoutGroupWithBackground(_canvas.transform, nameof(WebSocketUi), true);

            // Create status text
            _statusText = UiFactory.CreateText(panel.transform, "Disconnected.", UiLayoutMode.Grouped);

            // Create the Connect button
            _connectButton = UiFactory.CreateButton(
                panel.transform,
                "Connect",
                OnConnectClicked,
                UiLayoutMode.Grouped
            );

            var settingsButton = UiFactory.CreateButton(
                panel.transform,
                "Settings",
                () =>
                {
                    // Prevent duplicate settings panels
                    if (_settingsShown)
                    {
                        return;
                    }

                    _settingsShown = true;

                    var settingsObject = new GameObject("WebSocketSettings", typeof(RectTransform));
                    settingsObject.transform.SetParent(panel.transform, false);

                    settingsObject.AddComponent<ContentSizeFitter>().verticalFit =
                        ContentSizeFitter.FitMode.PreferredSize;
                    settingsObject.AddComponent<LayoutElement>().preferredHeight = 150f;

                    settingsObject.AddComponent<WebSocketSettingsUi>();
                },
                UiLayoutMode.Grouped);

            // Subscribe to status changes
            _webSocketClient.ConnectionStatusChanged += (sender, args) =>
            {
                _isConnected = args.IsConnected;
                Invoke(nameof(UpdateUiStatus), 0f);
            };

            // Subscribe to messages
            _webSocketClient.MessageReceived += (sender, args) =>
            {
                _lastMessage = args.Data;
                Invoke(nameof(AppendMessage), 0f);
            };
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                _canvas.enabled = !_canvas.enabled;
            }
        }

        private void OnConnectClicked()
        {
            Harmony_Patch.Instance.Logger.Log("Connect button clicked.");
            if (_webSocketClient.IsAlive)
            {
                Harmony_Patch.Instance.Logger.Log("Disconnecting.");
                _webSocketClient.Close();
            }
            else
            {
                Harmony_Patch.Instance.Logger.Log("Connecting.");
                _webSocketClient.Connect();
            }
        }

        private void UpdateUiStatus()
        {
            _statusText.text += "\n[Status] " + (_isConnected ? "Connected" : "Disconnected");
            _connectButton.GetComponentInChildren<Text>().text = _isConnected ? "Disconnect" : "Connect";
        }

        private void AppendMessage()
        {
            _statusText.text += "\n< " + _lastMessage;
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>())
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(eventSystem);
        }
    }
}
