// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright.Events
{
    /// <summary>
    /// Manages event subscriptions and broadcasts.
    /// Implemented in Phase 2.
    /// </summary>
    public static class EventSubscriptionManager
    {
        public static Protocol.Response HandleSubscribe(Protocol.Request request)
        {
            return Protocol.Response.CreateError(
                request.Id,
                "Event subscriptions are not yet implemented (Phase 2)",
                "NOT_IMPLEMENTED"
            );
        }

        public static Protocol.Response HandleUnsubscribe(Protocol.Request request)
        {
            return Protocol.Response.CreateError(
                request.Id,
                "Event unsubscriptions are not yet implemented (Phase 2)",
                "NOT_IMPLEMENTED"
            );
        }

        public static void Initialize(Server.TcpServer server)
        {
            // Initialize in Phase 2
        }
    }
}
