// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace LobotomyPlaywright.Server
{
    /// <summary>
    /// TCP server that listens for incoming connections and manages client handlers.
    /// Runs on a background thread; all game state access happens on the main Unity thread.
    /// </summary>
    public class TcpServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private Thread _listenerThread;
        private volatile bool _isRunning;
        private readonly List<ClientHandler> _clients = new List<ClientHandler>();
        private readonly object _clientsLock = new object();
        private readonly Queue<QueuedRequest> _requestQueue = new Queue<QueuedRequest>();
        private readonly object _queueLock = new object();
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

        private static void LogDebug(string message)
        {
            InitializeDebugMethods();

            if (s_debugLogMethod != null)
            {
                try
                {
                    s_debugLogMethod.Invoke(null, new object[] { message });
                }
                catch
                {
                    // Silently fail if Debug.Log throws
                }
            }
        }

        private static void LogError(string message)
        {
            InitializeDebugMethods();

            if (s_debugLogErrorMethod != null)
            {
                try
                {
                    s_debugLogErrorMethod.Invoke(null, new object[] { message });
                }
                catch
                {
                    // Silently fail if Debug.LogError throws
                }
            }
        }

        public bool IsRunning => _isRunning;
        public int Port => _port;
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

            _port = port;
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

            if (_listener != null)
            {
                _listener.Stop();
            }

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
                    var response = RequestHandler.ProcessRequest(queued.Request);
                    SendResponse(queued.Client, response);
                }
                catch (Exception ex)
                {
                    var errorResponse = Protocol.Response.CreateError(
                        queued.Request.Id,
                        ex.Message,
                        "PROCESSING_ERROR"
                    );
                    SendResponse(queued.Client, errorResponse);
                }
            }
        }

        public void BroadcastEvent(string eventName, object eventData)
        {
            ClientHandler[] clients;

            lock (_clientsLock)
            {
                clients = _clients.ToArray();
            }

            var eventMessage = Protocol.Response.CreateEvent(eventName, eventData);
            var json = Protocol.MessageSerializer.Serialize(eventMessage) + "\n";

            foreach (var client in clients)
            {
                if (client.IsConnected)
                {
                    client.Send(json);
                }
            }
        }

        private void ListenThread()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, _port);
                _listener.Start();

                LogDebug($"[LobotomyPlaywright] TCP server listening on 127.0.0.1:{_port}");

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
                if (_listener != null)
                {
                    _listener.Stop();
                }
            }
        }

        internal void EnqueueRequest(Protocol.Request request, TcpClient client, Thread thread)
        {
            lock (_queueLock)
            {
                _requestQueue.Enqueue(new QueuedRequest(request, client, thread));
            }
        }

        private void SendResponse(TcpClient client, Protocol.Response response)
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
                _clients.Remove(handler);
            }

            LogDebug($"[LobotomyPlaywright] Client disconnected. Total clients: {_clients.Count}");
        }
    }
}
