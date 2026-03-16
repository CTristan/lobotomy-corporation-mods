// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.JsonModels;
using LobotomyCorporationMods.Playwright.Server;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test.Server
{
    public sealed class RequestHandlerTests
    {
        [Fact]
        public void ProcessRequest_throws_when_request_is_null()
        {
            Action act = () => RequestHandler.ProcessRequest(null, null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ProcessRequest_returns_error_for_unknown_type()
        {
            var request = new Request { Id = "req-1", Type = "invalid" };

            var response = RequestHandler.ProcessRequest(request, null);

            response.Status.Should().Be("error");
            response.Code.Should().Be("UNKNOWN_TYPE");
            response.Error.Should().Contain("Unknown message type");
        }

        [Fact]
        public void ProcessRequest_returns_error_for_null_type()
        {
            var request = new Request { Id = "req-2", Type = null };

            var response = RequestHandler.ProcessRequest(request, null);

            response.Status.Should().Be("error");
            response.Code.Should().Be("UNKNOWN_TYPE");
        }

        [Fact]
        public void ProcessRequest_routes_query_type_to_query_router()
        {
            var request = new Request { Id = "req-3", Type = "query", Target = "invalid_target" };

            var response = RequestHandler.ProcessRequest(request, null);

            // QueryRouter will return UNKNOWN_TARGET for invalid target
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
        }

        [Fact]
        public void ProcessRequest_routes_command_type_to_command_router()
        {
            var request = new Request { Id = "req-4", Type = "command", Action = "unknown_action" };

            var response = RequestHandler.ProcessRequest(request, null);

            // CommandRouter will return UNKNOWN_ACTION for invalid action
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Code.Should().Be("UNKNOWN_ACTION");
        }

        [Fact]
        public void ProcessRequest_routes_subscribe_type()
        {
            var request = new Request { Id = "req-5", Type = "subscribe" };

            var response = RequestHandler.ProcessRequest(request, null);

            // EventSubscriptionManager returns an error or success
            response.Should().NotBeNull();
        }

        [Fact]
        public void ProcessRequest_routes_unsubscribe_type()
        {
            var request = new Request { Id = "req-6", Type = "unsubscribe" };

            var response = RequestHandler.ProcessRequest(request, null);

            response.Should().NotBeNull();
        }
    }
}
