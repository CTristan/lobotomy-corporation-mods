// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.DontChatMe.Interfaces;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    public class WebSocketUi : MonoBehaviour
    {
        private readonly IWebSocketClient _webSocketClient = Harmony_Patch.Instance.WebSocketClient;
        private Button _connectButton;
        private Text _statusText;

        private void Start()
        {
            var modCanvas = new GameObject("ModCanvas");
            DontDestroyOnLoad(modCanvas);
            var canvas = modCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            modCanvas.AddComponent<CanvasScaler>();
            modCanvas.AddComponent<GraphicRaycaster>();

            // Button
            var connectButton = new GameObject("ConnectBtn");
            connectButton.transform.SetParent(modCanvas.transform);
            _connectButton = connectButton.AddComponent<Button>();
            var buttonText = new GameObject("BtnText");
            buttonText.transform.SetParent(connectButton.transform);
            var btnText = buttonText.AddComponent<Text>();
            btnText.text = "Connect";
            btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            btnText.alignment = TextAnchor.MiddleCenter;

            var btnRect = connectButton.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(160, 30);
            btnRect.anchoredPosition = new Vector2(100, -100);

            // Textbox
            var statusTextBox = new GameObject("StatusText");
            statusTextBox.transform.SetParent(modCanvas.transform);
            _statusText = statusTextBox.AddComponent<Text>();
            _statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _statusText.alignment = TextAnchor.UpperLeft;
            _statusText.rectTransform.sizeDelta = new Vector2(400, 200);
            _statusText.rectTransform.anchoredPosition = new Vector2(100, -150);
            _statusText.text = "Disconnected.";

            _connectButton.onClick.AddListener(OnConnectClicked);
        }

        private void OnConnectClicked()
        {
            if (_webSocketClient.IsAlive)
            {
                _webSocketClient.Close();
            }
            else
            {
                _webSocketClient.Connect();
            }
        }
    }
}
