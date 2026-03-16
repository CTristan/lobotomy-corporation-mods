// SPDX-License-Identifier: MIT

using System;
using LobotomyPlaywright.JsonModels;

namespace LobotomyPlaywright.Server
{
    /// <summary>
    /// Routes incoming requests to the appropriate handler (query, command, subscribe).
    /// Runs on the main Unity thread.
    /// </summary>
    public static class RequestHandler
    {
        public static Response ProcessRequest(Request request, ClientHandler clientHandler)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                switch (request.Type?.ToLowerInvariant())
                {
                    case "query":
                        return Queries.QueryRouter.HandleQuery(request);

                    case "command":
                        return Commands.CommandRouter.HandleCommand(request);

                    case "subscribe":
                        return Events.EventSubscriptionManager.HandleSubscribe(request, clientHandler);

                    case "unsubscribe":
                        return Events.EventSubscriptionManager.HandleUnsubscribe(request, clientHandler);

                    default:
                        return Response.CreateError(
                            request.Id,
                            $"Unknown message type: {request.Type}",
                            "UNKNOWN_TYPE"
                        );
                }
            }
            catch (Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "ProcessRequest");
                return Response.CreateError(
                    request.Id,
                    $"Error processing request: {ex.Message}",
                    "PROCESSING_ERROR"
                );
            }
        }
    }
}
