// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using LobotomyPlaywright.JsonModels;
using LobotomyPlaywright.Server;

namespace LobotomyPlaywright.Events
{
    /// <summary>
    /// Manages event subscriptions for TCP clients.
    /// Tracks which clients are subscribed to which events.
    /// </summary>
    public static class EventSubscriptionManager
    {
        private static readonly Dictionary<Server.ClientHandler, HashSet<string>> s_clientSubscriptions =
            new Dictionary<Server.ClientHandler, HashSet<string>>();

        private static readonly object s_lock = new object();
        private static Server.TcpServer s_server;
        private static NoticeSubscriber s_noticeSubscriber;
        private static bool s_hasSubscribed;

        /// <summary>
        /// Initialize the event subscription system.
        /// </summary>
        public static void Initialize(Server.TcpServer server)
        {
            if (server == null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            s_server = server;

            // Create Notice subscriber but defer subscription
            // Subscription will happen when Notice system is ready
            s_noticeSubscriber = new NoticeSubscriber();
            s_hasSubscribed = false;
        }

        /// <summary>
        /// Attempt to subscribe to events if not already subscribed and Notice is ready.
        /// Should be called periodically (e.g., from Update).
        /// </summary>
        public static void TrySubscribeToEvents()
        {
            // Check if Notice instance is ready and NoticeName is initialized
            // (Done outside the lock to avoid holding it during potential blocking operations)
            if (Notice.instance == null || NoticeName.OnStageStart == null)
            {
                return;
            }

            lock (s_lock)
            {
                if (s_hasSubscribed)
                {
                    return;
                }

                try
                {
                    s_noticeSubscriber.SubscribeToAllEvents();
                    s_hasSubscribed = true;
                }
                catch (Exception ex)
                {
                    LobotomyPlaywright.Server.TcpServer.LogError($"[LobotomyPlaywright] Failed to subscribe to events: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Broadcast an event to all subscribed clients.
        /// </summary>
        public static void BroadcastEvent(string eventName, object eventData)
        {
            if (string.IsNullOrEmpty(eventName) || s_server == null)
            {
                return;
            }

            try
            {
                // Get all clients subscribed to this event
                var subscribedClients = GetSubscribersForEvent(eventName);

                // Create the event message
                var eventMessage = Response.CreateEvent(eventName, eventData);
                var json = Protocol.MessageSerializer.Serialize(eventMessage) + "\n";

                // Send to each subscribed client
                foreach (var client in subscribedClients)
                {
                    if (client.IsConnected)
                    {
                        client.Send(json);
                    }
                }
            }
            catch (Exception ex)
            {
                LobotomyPlaywright.Server.TcpServer.LogError($"[LobotomyPlaywright] Failed to broadcast event {eventName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle a subscribe request from a client.
        /// </summary>
        public static Response HandleSubscribe(Request request, ClientHandler clientHandler)
        {
            if (request.Events == null || request.Events.Count == 0)
            {
                return Response.CreateError(
                    request.Id,
                    "No events specified in subscribe request",
                    "NO_EVENTS"
                );
            }

            // Subscribe the client to the requested events
            if (clientHandler != null)
            {
                AddClient(clientHandler, request.Events);
            }
            else
            {
                // If no client handler is provided, subscribe all connected clients
                // (fallback for backward compatibility)
                lock (s_lock)
                {
                    foreach (var client in s_clientSubscriptions.Keys.ToList())
                    {
                        AddClient(client, request.Events);
                    }
                }
            }

            return Response.CreateSuccess(
                request.Id,
                new { subscribed = request.Events }
            );
        }

        /// <summary>
        /// Handle an unsubscribe request from a client.
        /// </summary>
        public static Response HandleUnsubscribe(Request request, ClientHandler clientHandler)
        {
            if (request.Events == null || request.Events.Count == 0)
            {
                return Response.CreateError(
                    request.Id,
                    "No events specified in unsubscribe request",
                    "NO_EVENTS"
                );
            }

            // Unsubscribe the client from the requested events
            if (clientHandler != null)
            {
                RemoveClientEvents(clientHandler, request.Events);
            }

            return Response.CreateSuccess(
                request.Id,
                new { unsubscribed = request.Events }
            );
        }

        /// <summary>
        /// Add a client's subscriptions.
        /// </summary>
        public static void AddClient(Server.ClientHandler client, IEnumerable<string> events)
        {
            if (client == null)
            {
                return;
            }

            lock (s_lock)
            {
                if (!s_clientSubscriptions.ContainsKey(client))
                {
                    s_clientSubscriptions[client] = new HashSet<string>();
                }

                foreach (var eventName in events)
                {
                    if (!string.IsNullOrEmpty(eventName))
                    {
                        s_clientSubscriptions[client].Add(eventName);
                    }
                }
            }
        }

        /// <summary>
        /// Remove a client's subscriptions.
        /// </summary>
        public static void RemoveClient(Server.ClientHandler client)
        {
            if (client == null)
            {
                return;
            }

            lock (s_lock)
            {
                s_clientSubscriptions.Remove(client);
            }
        }

        /// <summary>
        /// Remove specific event subscriptions for a client.
        /// </summary>
        public static void RemoveClientEvents(Server.ClientHandler client, IEnumerable<string> events)
        {
            if (client == null)
            {
                return;
            }

            lock (s_lock)
            {
                if (s_clientSubscriptions.ContainsKey(client))
                {
                    foreach (var eventName in events)
                    {
                        s_clientSubscriptions[client].Remove(eventName);
                    }
                }
            }
        }

        /// <summary>
        /// Check if a client is subscribed to an event.
        /// </summary>
        public static bool IsClientSubscribed(Server.ClientHandler client, string eventName)
        {
            if (client == null || string.IsNullOrEmpty(eventName))
            {
                return false;
            }

            lock (s_lock)
            {
                return s_clientSubscriptions.ContainsKey(client) &&
                       s_clientSubscriptions[client].Contains(eventName);
            }
        }

        /// <summary>
        /// Get all clients subscribed to a specific event.
        /// </summary>
        public static Server.ClientHandler[] GetSubscribersForEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return new Server.ClientHandler[0];
            }

            lock (s_lock)
            {
                return s_clientSubscriptions
                    .Where(kvp => kvp.Value.Contains(eventName))
                    .Select(kvp => kvp.Key)
                    .Where(c => c.IsConnected)
                    .ToArray();
            }
        }

        /// <summary>
        /// Get all event names a client is subscribed to.
        /// </summary>
        public static string[] GetClientEvents(Server.ClientHandler client)
        {
            if (client == null)
            {
                return new string[0];
            }

            lock (s_lock)
            {
                return s_clientSubscriptions.ContainsKey(client)
                    ? s_clientSubscriptions[client].ToArray()
                    : new string[0];
            }
        }

        /// <summary>
        /// Cleanup resources.
        /// </summary>
        public static void Shutdown()
        {
            lock (s_lock)
            {
                s_clientSubscriptions.Clear();
            }

            if (s_noticeSubscriber != null)
            {
                s_noticeSubscriber.UnsubscribeFromAllEvents();
                s_noticeSubscriber = null;
            }

            s_server = null;
        }
    }
}
