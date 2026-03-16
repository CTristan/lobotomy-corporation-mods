// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.JsonModels;
using LobotomyCorporationMods.Playwright.Queries;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test.Queries
{
    public sealed class QueryRouterTests
    {
        [Fact]
        public void HandleQuery_returns_error_for_null_request()
        {
            var response = QueryRouter.HandleQuery(null);

            response.Status.Should().Be("error");
            response.Code.Should().Be("MISSING_TARGET");
        }

        [Fact]
        public void HandleQuery_returns_error_for_null_target()
        {
            var request = new Request { Id = "req-1", Target = null };

            var response = QueryRouter.HandleQuery(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("MISSING_TARGET");
        }

        [Fact]
        public void HandleQuery_returns_error_for_empty_target()
        {
            var request = new Request { Id = "req-1", Target = "" };

            var response = QueryRouter.HandleQuery(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("MISSING_TARGET");
        }

        [Fact]
        public void HandleQuery_returns_error_for_unknown_target()
        {
            var request = new Request { Id = "req-1", Target = "nonexistent" };

            var response = QueryRouter.HandleQuery(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("UNKNOWN_TARGET");
        }

        [Fact]
        public void HandleQuery_preserves_request_id_in_error_response()
        {
            var request = new Request { Id = "my-unique-id", Target = "invalid_target" };

            var response = QueryRouter.HandleQuery(request);

            response.Id.Should().Be("my-unique-id");
        }
    }
}
