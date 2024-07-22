// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    public sealed class CrowdControlClient
    {
        private const string IpAddress = "127.0.0.1";
        private const int Port = 3330;
        private const int NetworkSleepInterval = 10000;
        private const int RetryTimeout = 10000;
        private const int TickDuration = 200;

        internal static Socket Socket { get; private set; }

        internal bool IsRunning { get; private set; } = true;

        private IPEndPoint Endpoint { get; } = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
        private Queue<CrowdControlRequest> RequestQueue { get; } = new Queue<CrowdControlRequest>();

        private Dictionary<string, CrowdControlAction> DelegateDictionary { get; } = new Dictionary<string, CrowdControlAction>
        {
            {
                nameof(CrowdControlDelegates.AddEnergy), CrowdControlDelegates.AddEnergy
            },
            {
                nameof(CrowdControlDelegates.RemoveEnergy), CrowdControlDelegates.RemoveEnergy
            },
            {
                nameof(CrowdControlDelegates.RandomMeltdown), CrowdControlDelegates.RandomMeltdown
            },
        };

        private bool Paused { get; set; }

        internal void NetworkLoop()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            while (IsRunning)
            {
                Harmony_Patch.Instance.Logger.LogInfo("Attempting to connect to Crowd Control");

                try
                {
                    Socket = new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    if (Socket.BeginConnect(Endpoint, null, null).AsyncWaitHandle.WaitOne(RetryTimeout, true) && Socket.Connected)
                    {
                        ClientLoop();
                    }
                    else
                    {
                        Harmony_Patch.Instance.Logger.LogInfo("Failed to connect to Crowd Control");
                    }

                    Socket.Close();
                }
                catch (Exception e)
                {
                    Harmony_Patch.Instance.Logger.LogInfo(e.GetType().Name);
                    Harmony_Patch.Instance.Logger.LogInfo("Failed to connect to Crowd Control");
                    throw;
                }

                Thread.Sleep(NetworkSleepInterval);
            }
        }

        private void ClientLoop()
        {
            Harmony_Patch.Instance.Logger.LogInfo("Connected to Crowd Control");

            using (new System.Threading.Timer(TimeUpdate, null, 0, 200))
            {
                try
                {
                    while (IsRunning)
                    {
                        var req = CrowdControlRequest.Receive(this, Socket);
                        if (req == null || req.IsKeepAlive)
                        {
                            continue;
                        }

                        lock (RequestQueue)
                        {
                            RequestQueue.Enqueue(req);
                        }
                    }
                }
                catch (Exception)
                {
                    Harmony_Patch.Instance.Logger.LogInfo("Disconnected from Crowd Control");
                    Socket.Close();
                    throw;
                }
            }
        }

        private void TimeUpdate(object state)
        {
            if (!IsReady())
            {
                TimedThread.AddTime(TickDuration);
                Paused = true;
            }
            else if (Paused)
            {
                Paused = false;
                TimedThread.UnPause();
                TimedThread.TickTime(TickDuration);
            }
            else
            {
                TimedThread.TickTime(TickDuration);
            }
        }

        private static bool IsReady()
        {
            try
            {
                return IsGameStateReady();
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogException(e);
                throw;
            }
        }

        private static bool IsGameStateReady()
        {
            var gameManager = GameManager.currentGameManager;

            return gameManager.IsNotNull() && gameManager.ManageStarted && gameManager.state == GameState.PLAYING;
        }

        internal void RequestLoop()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            while (IsRunning)
            {
                try
                {
                    CrowdControlRequest request = null;
                    lock (RequestQueue)
                    {
                        if (RequestQueue.Count == 0)
                        {
                            continue;
                        }

                        request = RequestQueue.Dequeue();
                    }

                    var code = request.Code;
                    try
                    {
                        CrowdControlResponse response;
                        if (!IsReady())
                        {
                            response = new CrowdControlResponse(request.Id, CrowdControlResponseStatus.Retry);
                        }
                        else
                        {
                            response = DelegateDictionary[code](this, request);
                        }

                        if (response == null)
                        {
                            response = new CrowdControlResponse(request.Id, CrowdControlResponseStatus.Failure, $"Request error for '{code}'");
                        }

                        response.Send(Socket);
                    }
                    catch (KeyNotFoundException)
                    {
                        new CrowdControlResponse(request.Id, CrowdControlResponseStatus.Failure, $"Request error for '{code}'").Send(Socket);
                    }
                }
                catch (Exception ex)
                {
                    Harmony_Patch.Instance.Logger.LogException(ex);
                    Harmony_Patch.Instance.Logger.LogInfo("Disconnected from Crowd Control");
                    Socket.Close();
                    throw;
                }
            }
        }

        [UsedImplicitly]
        public void Stop()
        {
            IsRunning = false;
        }
    }
}
