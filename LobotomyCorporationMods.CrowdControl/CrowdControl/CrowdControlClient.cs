// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CrowdControl.Constants;

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    public sealed class CrowdControlClient
    {
        private const string IpAddress = "127.0.0.1";
        private const int Port = 3330;
        private const int NetworkSleepInterval = 1000;
        private const int RetryTimeout = 1000;
        private const int TickDuration = 200;

        internal static Socket Socket { get; private set; }

        internal bool IsRunning { get; private set; } = true;

        private IPEndPoint Endpoint { get; } = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
        private Queue<CrowdControlRequest> RequestQueue { get; } = new Queue<CrowdControlRequest>();

        private Dictionary<string, CrowdControlAction> DelegateDictionary { get; } = new Dictionary<string, CrowdControlAction>
        {
            {
                "Example", CrowdControlDelegates.Example
            },
        };

        private bool Paused { get; set; }

        public void NetworkLoop()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            while (IsRunning)
            {
                try
                {
                    HandleServerConnection();
                }
                catch (Exception e)
                {
                    Harmony_Patch.Instance.Logger.LogException(e);
                    Harmony_Patch.Instance.Logger.LogInfo(LogMessages.ConnectionFailed);
                    throw;
                }

                Thread.Sleep(NetworkSleepInterval);
            }
        }

        private void HandleServerConnection()
        {
            Harmony_Patch.Instance.Logger.LogInfo($"Network Loop:{LogMessages.ConnectionAttempt}");
            Socket = new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            if (Socket.BeginConnect(Endpoint, null, null).AsyncWaitHandle.WaitOne(RetryTimeout, true) && Socket.Connected)
            {
                ClientLoop();
            }
            else
            {
                Harmony_Patch.Instance.Logger.LogInfo($"Network Loop:{LogMessages.ConnectionFailed}");
            }

            Socket.Close();
        }

        private void ClientLoop()
        {
            Harmony_Patch.Instance.Logger.LogInfo($"Network Loop:{LogMessages.ConnectionSuccess}");

            using (new System.Threading.Timer(TimeUpdate, null, 0, 200))
            {
                try
                {
                    ProcessRequests();
                }
                catch (Exception e)
                {
                    Harmony_Patch.Instance.Logger.LogException(e);
                    Harmony_Patch.Instance.Logger.LogInfo($"Network Loop:{LogMessages.Disconnected}");
                    Socket.Close();
                    throw;
                }
            }
        }

        private void ProcessRequests()
        {
            while (IsRunning)
            {
                var request = CrowdControlRequest.Receive(this, Socket);

                if (request.IsNotNull())
                {
                    Harmony_Patch.Instance.Logger.LogInfo($"Request Received: Id: {request.Id}, Type: {request.Type}, Code: {request.Code}");
                }

                if (!IsValidRequest(request))
                {
                    continue;
                }

                Harmony_Patch.Instance.Logger.LogInfo("Valid request! Queueing...");
                EnqueueRequest(request);
            }
        }

        private static bool IsValidRequest([CanBeNull] CrowdControlRequest request)
        {
            return request != null && !request.IsKeepAlive && !string.IsNullOrEmpty(request.Code);
        }

        private void EnqueueRequest([NotNull] CrowdControlRequest request)
        {
            lock (RequestQueue)
            {
                RequestQueue.Enqueue(request);
                Harmony_Patch.Instance.Logger.LogInfo($"Request Enqueued: {request.Id}");
            }
        }

        private void TimeUpdate(object state)
        {
            if (!IsReady())
            {
                TimedThread.AddTime(TickDuration);
                Paused = true;
            }
            else
            {
                HandleInGameStatus();
            }
        }

        private void HandleInGameStatus()
        {
            if (Paused)
            {
                Paused = false;
                TimedThread.UnPause();
            }

            TickTime();
        }

        private static void TickTime()
        {
            TimedThread.TickTime(TickDuration);
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
                ProcessRequestsInQueue();
            }
        }

        private void ProcessRequestsInQueue()
        {
            try
            {
                CrowdControlRequest request;
                lock (RequestQueue)
                {
                    if (RequestQueue.Count == 0)
                    {
                        return;
                    }

                    Harmony_Patch.Instance.Logger.LogInfo($"Dequeueing request: {RequestQueue.Peek().Id}");
                    request = RequestQueue.Dequeue();
                }

                HandleSingleRequest(request);
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogException(e);
                Harmony_Patch.Instance.Logger.LogInfo("Disconnected from Crowd Control");
                Socket.Close();
                throw;
            }
        }

        private void HandleSingleRequest([NotNull] CrowdControlRequest request)
        {
            var code = request.Code;

            var response = !IsReady() ? new CrowdControlResponse(request.Id, CrowdControlResponseStatus.STATUS_RETRY) : DelegateDictionary[code](this, request);

            if (response.IsNull())
            {
                var errorMessage = $"Request error for '{code}'";
                response = new CrowdControlResponse(request.Id, CrowdControlResponseStatus.STATUS_FAILURE, errorMessage);
                Harmony_Patch.Instance.Logger.LogWarning(errorMessage);
            }

            Harmony_Patch.Instance.Logger.LogInfo($"Current effect queue count: {RequestQueue.Count}");

            response.Send(Socket);
        }

        [UsedImplicitly]
        public void Stop()
        {
            IsRunning = false;
        }
    }
}
