// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models.EffectMessages;
using SharpJson;
using WebSocketSharp;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations
{
    public sealed class WebSocketClient : IWebSocketClient
    {
        private readonly JsonDecoder _decoder;
        private readonly IEffectExecutor _effectExecutor;
        private readonly Dictionary<string, Func<EffectContext, EffectResponse>> _handlers = new();
        private readonly WebSocket _webSocket;

        public WebSocketClient(string serverUrl) : this(serverUrl, new EffectExecutor())
        {
        }

        public WebSocketClient(string serverUrl, IEffectExecutor effectExecutor)
        {
            _webSocket = new WebSocket(serverUrl);
            _webSocket.OnError += (sender, ex) => Harmony_Patch.Instance.Logger.WriteException(ex.Exception);
            _webSocket.OnMessage += HandleMessage;

            _decoder = new JsonDecoder();
            _effectExecutor = effectExecutor;
        }

        public void Connect()
        {
            _webSocket.Connect();
        }

        public void Close()
        {
            _webSocket.Close();
        }

        private void HandleMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var json = e.Data;
                var request = JsonDecoder.DecodeText(json) as Dictionary<string, object>;
                var effectTrigger = EffectRequest.FromJson(request);
                if (effectTrigger.MessageType != MessageTypes.Effect || string.IsNullOrEmpty(effectTrigger.EffectId))
                {
                    SendErrorResponse("unknown", "unknown", "Invalid request type.");
                    return;
                }

                if (!_handlers.TryGetValue(effectTrigger.EffectId, out var handler))
                {
                    SendErrorResponse(request.EffectId, request.Context.RequestId, "Unknown effect.");
                    return;
                }

                var response = handler(request.Context);
                SendJson(response.ToJson());
            }
            catch (Exception ex)
            {
                SendErrorResponse("unknown", "unknown", $"Exception: {ex.Message}");
            }
        }

        private void SendJson(string json)
        {
            _webSocket.Send(json);
        }
    }
}
