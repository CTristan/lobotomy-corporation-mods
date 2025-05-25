// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Threading;
using LobotomyCorporationMods.DontChatMe.EventArgs;
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

        private bool _disposed;
        private volatile bool _isReconnecting;
        private volatile bool _shutdownRequested;

        public WebSocketClient(string serverPath)
        {
            _webSocket = new WebSocket(serverPath);
            _webSocket.OnClose += WebSocket_OnClose;
            _webSocket.OnError += WebSocket_OnError;
            _webSocket.OnMessage += WebSocket_OnMessage;
        }

        public void Dispose()
        {
            _shutdownRequested = true;
            Dispose(true);
        }

        public void Close()
        {
            _webSocket.Close();
            _isReconnecting = false;
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(false));
        }

        public void Connect()
        {
            try
            {
                _webSocket.Connect();

                if (_webSocket.IsAlive)
                {
                    ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(true));
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Harmony_Patch.Instance.Logger.Log($"Connect() threw: {ex.Message}");
                Harmony_Patch.Instance.Logger.LogException(ex);
            }
        }

        public event EventHandler<CloseEventArgs> ConnectionClosed;
        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        public bool IsAlive
        {
            get => _webSocket.IsAlive;
        }

        public event EventHandler<MessageEventArgs> MessageReceived;

        public void RegisterEffectHandler(string effectId, Func<EffectRequest, EffectResponse> handler)
        {
            _effectHandlers[effectId] = handler;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Detach event handlers
                _webSocket.OnClose -= WebSocket_OnClose;
                _webSocket.OnError -= WebSocket_OnError;
                _webSocket.OnMessage -= WebSocket_OnMessage;

                // Dispose of managed resources
                _webSocket.Close();
                ((IDisposable)_webSocket).Dispose();
            }

            // No unmanaged resources to free

            _disposed = true;
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

        private void HandleMessage(MessageEventArgs e)
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
                Harmony_Patch.Instance.Logger.LogException(ex);
                SendJson(ErrorResponse("unknown", "Internal error"));
                throw;
            }
        }

        private void ReconnectWithBackoff()
        {
            Harmony_Patch.Instance.Logger.Log($"Reconnect thread {Thread.CurrentThread.ManagedThreadId} starting.");
            const int MaxAttempts = 5;
            const int DelayMilliseconds = 3000;

            for (var attempt = 1; attempt <= MaxAttempts && !_shutdownRequested; attempt++)
            {
                try
                {
                    _webSocket.Connect();

                    if (_webSocket.IsAlive)
                    {
                        Harmony_Patch.Instance.Logger.Log("WebSocket reconnected successfully.");
                        ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(true));
                        return;
                    }

                    Harmony_Patch.Instance.Logger.Log(
                        $"Reconnect attempt {attempt} failed. Retrying in {DelayMilliseconds}ms...");
                }
#pragma warning disable CA1031
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    Harmony_Patch.Instance.Logger.LogException(ex);
                    Harmony_Patch.Instance.Logger.Log(
                        $"Reconnect attempt {attempt} threw an exception. Retrying in {DelayMilliseconds}ms...");
                }

                Thread.Sleep(DelayMilliseconds);
            }

            Harmony_Patch.Instance.Logger.Log("WebSocket reconnect failed after all attempts.");
        }

        private void SendJson(string json)
        {
            _webSocket.Send(json);
        }


        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            Harmony_Patch.Instance.Logger.Log($"Connection closed. Code: {e.Code}, Reason: {e.Reason}");
            ConnectionClosed?.Invoke(this, e);
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(false));

            if (_isReconnecting)
            {
                return;
            }

            _isReconnecting = true;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                ReconnectWithBackoff();
                _isReconnecting = false;
            });
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            Harmony_Patch.Instance.Logger.Log($"WebSocket error: {e.Message}.");

            if (e.Exception != null)
            {
                Harmony_Patch.Instance.Logger.LogException(e.Exception);
            }

            ErrorOccurred?.Invoke(this, e);
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(false));
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            HandleMessage(e);
            MessageReceived?.Invoke(this, e);
        }
    }
}
