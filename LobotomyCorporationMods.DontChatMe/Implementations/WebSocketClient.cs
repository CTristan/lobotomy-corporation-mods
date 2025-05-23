// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Threading;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models.EffectMessages;
using SharpJson;
using WebSocketSharp;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations
{
    public sealed class WebSocketClient : IWebSocketClient, IDisposable
    {
        private readonly Dictionary<string, Func<EffectRequest, EffectResponse>> _effectHandlers
            = new Dictionary<string, Func<EffectRequest, EffectResponse>>();

        private readonly WebSocket _webSocket;

        public WebSocketClient(string serverPath)
        {
            _webSocket = new WebSocket(serverPath);
            _webSocket.OnClose += (sender, e) =>
            {
                Harmony_Patch.Instance.Logger.WriteInfo("Connection closed. Attempting reconnect...");
                ThreadPool.QueueUserWorkItem(_ => ReconnectWithBackoff());
            };
            _webSocket.OnError += (sender, ex) => Harmony_Patch.Instance.Logger.WriteException(ex.Exception);
            _webSocket.OnMessage += HandleMessage;
        }

        public void Dispose()
        {
            if (_webSocket != null)
            {
                _webSocket.Close();
                _webSocket.OnMessage -= HandleMessage;
            }
        }

        public void Connect()
        {
            _webSocket.Connect();
        }

        public void Close()
        {
            _webSocket.Close();
        }

        public void RegisterEffectHandler(string effectId, Func<EffectRequest, EffectResponse> handler)
        {
            _effectHandlers[effectId] = handler;
        }

        private void HandleMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var json = e.Data;

                if (!(JsonDecoder.DecodeText(json) is Dictionary<string, object> rawRequest))
                {
                    SendJson(ErrorResponse("unknown", "Invalid JSON"));
                    return;
                }

                if (!rawRequest.ContainsKey("type"))
                {
                    SendJson(ErrorResponse(rawRequest["requestId"]?.ToString() ?? "unknown", "Missing 'type' field"));
                    return;
                }

                var effectRequest = EffectRequest.FromJson(rawRequest);
                if (!_effectHandlers.TryGetValue(effectRequest.EffectId, out var handler))
                {
                    SendJson(ErrorResponse(effectRequest.RequestId, $"Unknown effect: {effectRequest.EffectId}"));
                    return;
                }

                var response = handler(effectRequest);
                SendJson(response.ToJson());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);
                SendJson(ErrorResponse("unknown", "Internal error"));
                throw;
            }
        }

        private static string ErrorResponse(string requestId, string message)
        {
            var error = new EffectResponse
            {
                RequestId = requestId,
                EffectId = "unknown",
                Status = "error",
                Message = message
            };

            return error.ToJson();
        }

        private void ReconnectWithBackoff()
        {
            const int MaxAttempts = 5;
            const int DelayMilliseconds = 3000;

            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    _webSocket.Connect();

                    if (_webSocket.IsAlive)
                    {
                        Harmony_Patch.Instance.Logger.WriteInfo("WebSocket reconnected successfully.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Harmony_Patch.Instance.Logger.WriteException(ex);
                    throw;
                }

                Harmony_Patch.Instance.Logger.WriteInfo(
                    $"Reconnect attempt {attempt} failed. Retrying in {DelayMilliseconds}ms...");
                Thread.Sleep(DelayMilliseconds);
            }

            Harmony_Patch.Instance.Logger.WriteInfo("WebSocket reconnect failed after all attempts.");
        }

        private void SendJson(string json)
        {
            _webSocket.Send(json);
        }
    }
}
