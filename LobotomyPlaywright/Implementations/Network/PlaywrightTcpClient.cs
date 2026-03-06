// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using LobotomyPlaywright.Interfaces.Network;

namespace LobotomyPlaywright.Implementations.Network;

/// <summary>
/// TCP client for communicating with the LobotomyPlaywright plugin.
/// </summary>
internal sealed class PlaywrightTcpClient : ITcpClient
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private int _requestId;
    private bool _disposed;

    public bool IsConnected => _tcpClient?.Connected == true && _stream != null;

    public void Connect(string host, int port, double timeout = 5.0)
    {
        _tcpClient = new TcpClient();
        _tcpClient.ReceiveTimeout = (int)(timeout * 1000);
        _tcpClient.SendTimeout = (int)(timeout * 1000);
        _tcpClient.Connect(host, port);
        _stream = _tcpClient.GetStream();
        _requestId = 0;
    }

    public void Disconnect()
    {
        _stream?.Close();
        _tcpClient?.Close();
        _stream = null;
        _tcpClient = null;
    }

    public Dictionary<string, object> Query(string target, Dictionary<string, object>? parameters = null)
    {
        var requestId = GenerateRequestId();
        var request = new Dictionary<string, object>
        {
            { "id", requestId },
            { "type", "query" },
            { "target", target },
            { "params", parameters ?? new Dictionary<string, object>() }
        };

        Send(request);
        var response = Receive();

        var responseId = GetResponseId(response);
        if (responseId != requestId)
        {
            throw new InvalidOperationException($"Response ID mismatch: expected {requestId}, got {responseId}");
        }

        var status = GetResponseStatus(response);
        if (status != "ok")
        {
            var error = GetResponseError(response);
            var code = GetResponseCode(response);
            throw new InvalidOperationException($"Query failed: {error} (code: {code})");
        }

        return GetResponseData(response);
    }

    public bool SendCommand(string action, Dictionary<string, object>? parameters = null)
    {
        var requestId = GenerateRequestId();
        var request = new Dictionary<string, object>
        {
            { "id", requestId },
            { "type", "command" },
            { "action", action },
            { "params", parameters ?? new Dictionary<string, object>() }
        };

        Send(request);

        try
        {
            var response = Receive();
            var status = GetResponseStatus(response);
            return status == "ok";
        }
        catch
        {
            return false;
        }
    }

    public bool Subscribe(string[] events)
    {
        var requestId = GenerateRequestId();
        var request = new Dictionary<string, object>
        {
            { "id", requestId },
            { "type", "subscribe" },
            { "events", events }
        };

        Send(request);
        var response = Receive();

        var status = GetResponseStatus(response);
        if (status != "ok")
        {
            var error = GetResponseError(response);
            var code = GetResponseCode(response);
            throw new InvalidOperationException($"Subscribe failed: {error} (code: {code})");
        }

        return true;
    }

    public Dictionary<string, object>? WaitForEvent(double timeoutSeconds = 60.0)
    {
        var startTime = DateTime.UtcNow;
        var timeoutMs = (int)(timeoutSeconds * 1000);

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
        {
            var remainingMs = Math.Max(100, timeoutMs - (int)(DateTime.UtcNow - startTime).TotalMilliseconds);

            if (_stream == null || _tcpClient == null || !_tcpClient.Connected)
            {
                throw new InvalidOperationException("Not connected to server");
            }

            _stream.ReadTimeout = remainingMs;

            try
            {
                var message = ReceiveMessage();
                if (message == null)
                {
                    continue;
                }

                var messageType = GetMessageType(message);
                if (messageType == "event")
                {
                    return message;
                }

                // Ignore response messages
                if (messageType == "response")
                {
                    continue;
                }
            }
            catch (IOException)
            {
                // Timeout, continue loop
            }
            catch (SocketException)
            {
                // Connection error
                return null;
            }
        }

        return null;
    }

    private string GenerateRequestId()
    {
        _requestId++;
        return $"req-{_requestId}";
    }

    private void Send(Dictionary<string, object> message)
    {
        if (_stream == null)
        {
            throw new InvalidOperationException("Not connected to server");
        }

        var json = JsonSerializer.Serialize(message) + "\n";
        var bytes = Encoding.UTF8.GetBytes(json);
        _stream.Write(bytes, 0, bytes.Length);
        _stream.Flush();
    }

    private Dictionary<string, object> Receive()
    {
        var message = ReceiveMessage();
        if (message == null)
        {
            throw new InvalidOperationException("Connection closed by server");
        }

        return message;
    }

    private Dictionary<string, object>? ReceiveMessage()
    {
        if (_stream == null)
        {
            throw new InvalidOperationException("Not connected to server");
        }

        var buffer = new List<byte>();
        var data = new byte[4096];

        while (true)
        {
            var bytesRead = _stream.Read(data, 0, data.Length);
            if (bytesRead == 0)
            {
                return null;
            }

            buffer.AddRange(data.Take(bytesRead));

            if (buffer.Contains((byte)'\n'))
            {
                break;
            }
        }

        var json = Encoding.UTF8.GetString(buffer.ToArray());
        var message = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        return message;
    }

    private static string GetResponseId(Dictionary<string, object> response)
    {
        return response.TryGetValue("id", out var id) ? id?.ToString() ?? string.Empty : string.Empty;
    }

    private static string GetResponseStatus(Dictionary<string, object> response)
    {
        return response.TryGetValue("status", out var status) ? status?.ToString() ?? string.Empty : string.Empty;
    }

    private static string GetResponseError(Dictionary<string, object> response)
    {
        return response.TryGetValue("error", out var error) ? error?.ToString() ?? string.Empty : string.Empty;
    }

    private static string GetResponseCode(Dictionary<string, object> response)
    {
        return response.TryGetValue("code", out var code) ? code?.ToString() ?? string.Empty : string.Empty;
    }

    private static Dictionary<string, object> GetResponseData(Dictionary<string, object> response)
    {
        if (response.TryGetValue("data", out var data) && data is Dictionary<string, object> dataDict)
        {
            return dataDict;
        }

        return new Dictionary<string, object>();
    }

    private static string GetMessageType(Dictionary<string, object> message)
    {
        return message.TryGetValue("type", out var type) ? type?.ToString() ?? string.Empty : string.Empty;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Disconnect();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
