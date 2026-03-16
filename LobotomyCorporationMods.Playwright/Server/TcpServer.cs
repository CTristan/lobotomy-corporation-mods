// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using LobotomyCorporationMods.Playwright.JsonModels;

#endregion

namespace LobotomyCorporationMods.Playwright.Server
{
    /// <summary>
    /// TCP server that listens for incoming connections and manages client handlers.
    /// Runs on a background thread; all game state access happens on the main Unity thread.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class TcpServer
    {
        private TcpListener _listener;
        private Thread _listenerThread;
        private volatile bool _isRunning;
        private readonly List<ClientHandler> _clients = new List<ClientHandler>();
        private readonly object _clientsLock = new object();
        private readonly Queue<QueuedRequest> _requestQueue = new Queue<QueuedRequest>();
        private readonly object _queueLock = new object();
        private readonly Dictionary<TcpClient, ClientHandler> _clientHandlers =
            new Dictionary<TcpClient, ClientHandler>();
        private static MethodInfo s_debugLogMethod;
        private static MethodInfo s_debugLogErrorMethod;
        private static bool s_methodsInitialized;

        private static void InitializeDebugMethods()
        {
            if (s_methodsInitialized)
            {
                return;
            }

            try
            {
                var unityCoreAssembly = Assembly.Load("UnityEngine.CoreModule");
                if (unityCoreAssembly != null)
                {
                    var debugType = unityCoreAssembly.GetType("UnityEngine.Debug");
                    if (debugType != null)
                    {
                        s_debugLogMethod = debugType.GetMethod("Log", new[] { typeof(object) });
                        s_debugLogErrorMethod = debugType.GetMethod("LogError", new[] { typeof(object) });
                    }
                }
            }
            catch
            {
                // Methods will remain null if initialization fails
            }

            s_methodsInitialized = true;
        }

        internal static void LogDebug(string message)
        {
            InitializeDebugMethods();

            if (s_debugLogMethod != null)
            {
                try
                {
                    _ = s_debugLogMethod.Invoke(null, new object[] { message });
                }
                catch
                {
                    // Silently fail if Debug.Log throws
                }
            }
        }

        internal static void LogError(string message)
        {
            InitializeDebugMethods();

            if (s_debugLogErrorMethod != null)
            {
                try
                {
                    _ = s_debugLogErrorMethod.Invoke(null, new object[] { message });
                }
                catch
                {
                    // Silently fail if Debug.LogError throws
                }
            }
        }

        public bool IsRunning => _isRunning;
        public int Port { get; }
        public int ClientCount
        {
            get
            {
                lock (_clientsLock)
                {
                    return _clients.Count;
                }
            }
        }

        public TcpServer(int port)
        {
            if (port < 1 || port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
            }

            Port = port;
        }

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            _listenerThread = new Thread(ListenThread)
            {
                IsBackground = true,
                Name = "LobotomyPlaywright-TcpServer"
            };
            _listenerThread.Start();
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;

            _listener?.Stop();

            // Disconnect all clients
            lock (_clientsLock)
            {
                foreach (var client in _clients)
                {
                    client.Disconnect();
                }
                _clients.Clear();
            }

            if (_listenerThread != null && _listenerThread.IsAlive)
            {
                if (!_listenerThread.Join(1000))
                {
                    _listenerThread.Abort();
                }
            }
        }

        /// <summary>
        /// Broadcasts an event to all subscribed clients.
        /// </summary>
        public void BroadcastEvent(string eventName, object eventData)
        {
            if (!_isRunning)
            {
                return;
            }

            Events.EventSubscriptionManager.BroadcastEvent(eventName, eventData);
        }

        public void ProcessQueuedRequests()
        {
            QueuedRequest[] requestsToProcess;

            lock (_queueLock)
            {
                requestsToProcess = _requestQueue.ToArray();
                _requestQueue.Clear();
            }

            foreach (var queued in requestsToProcess)
            {
                try
                {
                    // Get the ClientHandler for this request
                    ClientHandler clientHandler;
                    lock (_clientsLock)
                    {
                        _ = _clientHandlers.TryGetValue(queued.Client, out clientHandler);
                    }

                    var response = RequestHandler.ProcessRequest(queued.Request, clientHandler);
                    SendResponse(queued.Client, response);
                }
                catch (Exception ex)
                {
                    var errorResponse = Response.CreateError(
                        queued.Request.Id,
                        ex.Message,
                        "PROCESSING_ERROR"
                    );
                    SendResponse(queued.Client, errorResponse);
                }
            }
        }

        private void ListenThread()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, Port);
                _listener.Start();

                LogDebug($"[LobotomyPlaywright] TCP server listening on 127.0.0.1:{Port}");

                while (_isRunning)
                {
                    try
                    {
                        if (_listener.Pending())
                        {
                            var client = _listener.AcceptTcpClient();
                            var handler = new ClientHandler(client, this);
                            handler.Start();

                            lock (_clientsLock)
                            {
                                _clients.Add(handler);
                                _clientHandlers[client] = handler;
                            }

                            LogDebug($"[LobotomyPlaywright] Client connected. Total clients: {_clients.Count}");
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (_isRunning)
                        {
                            LogError($"[LobotomyPlaywright] Socket error: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    LogError($"[LobotomyPlaywright] Listener error: {ex.Message}");
                }
            }
            finally
            {
                _listener?.Stop();
            }
        }

        internal void EnqueueRequest(Request request, TcpClient client, Thread thread)
        {
            lock (_queueLock)
            {
                _requestQueue.Enqueue(new QueuedRequest(request, client, thread));
            }
        }

        private void SendResponse(TcpClient client, Response response)
        {
            var json = Protocol.MessageSerializer.Serialize(response) + "\n";

            try
            {
                if (client != null && client.Connected)
                {
                    var stream = client.GetStream();
                    var data = System.Text.Encoding.UTF8.GetBytes(json);
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                LogError($"[LobotomyPlaywright] Failed to send response: {ex.Message}");
            }
        }

        internal void RemoveClient(ClientHandler handler)
        {
            lock (_clientsLock)
            {
                _ = _clients.Remove(handler);
                // Also remove from the client handler mapping
                // We need to find the TcpClient associated with this handler
                foreach (var kvp in _clientHandlers)
                {
                    if (kvp.Value == handler)
                    {
                        _ = _clientHandlers.Remove(kvp.Key);
                        break;
                    }
                }
            }

            LogDebug($"[LobotomyPlaywright] Client disconnected. Total clients: {_clients.Count}");
        }
    }
}
