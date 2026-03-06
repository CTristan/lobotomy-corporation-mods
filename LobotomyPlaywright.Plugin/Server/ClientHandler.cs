// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LobotomyPlaywright.Events;
using UnityEngine;

namespace LobotomyPlaywright.Server
{
    /// <summary>
    /// Handles a single TCP client connection.
    /// Reads JSON-line messages and queues them for main-thread processing.
    /// </summary>
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly TcpServer _server;
        private Thread _thread;
        private volatile bool _isRunning;

        public bool IsConnected => _client != null && _client.Connected && _isRunning;

        public ClientHandler(TcpClient client, TcpServer server)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            if (server == null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            _client = client;
            _server = server;
            _isRunning = true;
        }

        public void Start()
        {
            _thread = new Thread(ReceiveLoop)
            {
                IsBackground = true,
                Name = "LobotomyPlaywright-ClientHandler"
            };
            _thread.Start();
        }

        public void Disconnect()
        {
            _isRunning = false;

            try
            {
                if (_client != null && _client.Connected)
                {
                    _client.Close();
                }
            }
            catch (Exception ex)
            {
                TcpServer.LogError($"[LobotomyPlaywright] Error disconnecting client: {ex.Message}");
            }
            finally
            {
                // Clean up event subscriptions
                EventSubscriptionManager.RemoveClient(this);
            }
        }

        public void Send(string message)
        {
            if (!IsConnected)
            {
                return;
            }

            try
            {
                var stream = _client.GetStream();
                var data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                TcpServer.LogError($"[LobotomyPlaywright] Failed to send to client: {ex.Message}");
                Disconnect();
            }
        }

        private void ReceiveLoop()
        {
            var buffer = new StringBuilder();
            var readBuffer = new byte[4096];
            Stream stream = null;

            try
            {
                stream = _client.GetStream();
                var networkStream = stream as System.Net.Sockets.NetworkStream;

                while (_isRunning && _client.Connected)
                {
                    if (networkStream != null && !networkStream.DataAvailable)
                    {
                        if (_client.Client != null && _client.Client.Poll(0, System.Net.Sockets.SelectMode.SelectRead))
                        {
                            break;
                        }
                        Thread.Sleep(10);
                        continue;
                    }

                    var bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    var chunk = Encoding.UTF8.GetString(readBuffer, 0, bytesRead);
                    buffer.Append(chunk);

                    // Process complete lines
                    var content = buffer.ToString();
                    var newlineIndex = content.IndexOf('\n');

                    while (newlineIndex >= 0)
                    {
                        var line = content.Substring(0, newlineIndex).Trim();
                        buffer.Remove(0, newlineIndex + 1);
                        content = buffer.ToString();

                        if (!string.IsNullOrEmpty(line))
                        {
                            ProcessMessage(line);
                        }

                        newlineIndex = content.IndexOf('\n');
                    }
                }
            }
            catch (IOException ex)
            {
                // Client disconnected normally
                if (_isRunning)
                {
                    TcpServer.LogDebug($"[LobotomyPlaywright] Client disconnected: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                TcpServer.LogError($"[LobotomyPlaywright] Client error: {ex.Message}");
            }
            finally
            {
                _server.RemoveClient(this);
                if (stream != null)
                {
                    try { stream.Close(); } catch { }
                }
                Disconnect();
            }
        }

        private void ProcessMessage(string json)
        {
            try
            {
                TcpServer.LogDebug($"[ClientHandler] Received JSON: {json}");
                var request = Protocol.MessageSerializer.DeserializeRequest(json);

                if (string.IsNullOrEmpty(request.Type))
                {
                    var errorResponse = Protocol.Response.CreateError(
                        null,
                        "Missing message type",
                        "INVALID_MESSAGE"
                    );
                    Send(Protocol.MessageSerializer.Serialize(errorResponse) + "\n");
                    return;
                }

                TcpServer.LogDebug($"[ClientHandler] Enqueueing request: id={request.Id}, type={request.Type}, target={request.Target}");
                _server.EnqueueRequest(request, _client, _thread);
            }
            catch (Exception ex)
            {
                TcpServer.LogError($"[ClientHandler] Failed to parse request: {ex.Message}");
                var errorResponse = Protocol.Response.CreateError(
                    null,
                    $"Failed to parse request: {ex.Message}",
                    "PARSE_ERROR"
                );
                Send(Protocol.MessageSerializer.Serialize(errorResponse) + "\n");
            }
        }
    }
}
