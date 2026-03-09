// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace LobotomyPlaywright.Interfaces.Network;

/// <summary>
/// Interface for TCP communication with the game plugin.
/// </summary>
public interface ITcpClient : IDisposable
{
    /// <summary>
    /// Connects to the TCP server.
    /// </summary>
    /// <param name="host">The host to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="timeout">Connection timeout in seconds.</param>
    void Connect(string host, int port, double timeout = 5.0);

    /// <summary>
    /// Disconnects from the TCP server.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Sends a query request and returns the response data.
    /// </summary>
    /// <param name="target">The query target (agents, creatures, game, etc.).</param>
    /// <param name="parameters">Optional query parameters.</param>
    /// <returns>The response data.</returns>
    Dictionary<string, object> Query(string target, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Sends a command to the server.
    /// </summary>
    /// <param name="action">The command action.</param>
    /// <param name="parameters">Optional command parameters.</param>
    /// <returns>True if the command succeeded.</returns>
    bool SendCommand(string action, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Sends a command to the server and returns the response data.
    /// </summary>
    /// <param name="action">The command action.</param>
    /// <param name="parameters">Optional command parameters.</param>
    /// <returns>The response data.</returns>
    Dictionary<string, object> SendCommandWithData(string action, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    /// <param name="events">List of event names to subscribe to.</param>
    /// <returns>True if subscription succeeded.</returns>
    bool Subscribe(string[] events);

    /// <summary>
    /// Waits for the next event message.
    /// </summary>
    /// <param name="timeoutSeconds">Maximum time to wait in seconds.</param>
    /// <returns>The event data, or null if timeout.</returns>
    Dictionary<string, object>? WaitForEvent(double timeoutSeconds = 60.0);

    /// <summary>
    /// Checks if the client is connected.
    /// </summary>
    bool IsConnected { get; }
}
