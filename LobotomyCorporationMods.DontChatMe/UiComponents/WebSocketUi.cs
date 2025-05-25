// SPDX-License-Identifier: MIT

#region

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

        private Button _connectButton;

        private bool _isConnected;
        private string _lastMessage;
        private Text _statusText;

        private void Start()
        {
            EnsureEventSystem();

            // Create a persistent overlay canvas
            var canvas = UiFactory.CreateOverlayCanvas("DontChatMeOverlay");

            // Create status text
            _statusText = UiFactory.CreateText(canvas.transform, "Disconnected.", new Vector2(100, -100), 16);

            // Create the Connect button
            _connectButton = UiFactory.CreateButton(
                canvas.transform,
                "Connect",
                new Vector2(300, -100),
                new Vector2(160, 40),
                OnConnectClicked
            );

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
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
                DontDestroyOnLoad(es);
            }
        }
    }
}
