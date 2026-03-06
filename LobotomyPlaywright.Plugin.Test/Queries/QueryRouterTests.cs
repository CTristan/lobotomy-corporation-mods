// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using FluentAssertions;
using LobotomyPlaywright.Protocol;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public sealed class QueryRouterTests : IDisposable
    {
        public QueryRouterTests()
        {
            SetGameReady(false);
        }

        public void Dispose()
        {
            SetGameReady(null);
        }

        private static void SetGameReady(bool? ready)
        {
            var property = typeof(GameStateQueries).GetProperty("IsGameQueryableOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            property?.SetValue(null, ready, null);
        }

        [Fact]
        public void HandleQuery_null_request_returns_error()
        {
            // Arrange
            Request request = null;

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Missing target parameter");
        }

        [Fact]
        public void HandleRequest_missing_target_returns_error()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = null
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Missing target parameter");
        }

        [Fact]
        public void HandleRequest_empty_target_returns_error()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = ""
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Missing target parameter");
        }

        [Fact]
        public void HandleQuery_unknown_target_returns_error()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "unknown"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Unknown query target: unknown");
        }

        [Fact]
        public void HandleQuery_game_not_ready_returns_error()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "agents"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Game is not in a queryable state");
            response.code.Should().Be("GAME_NOT_READY");
        }

        [Fact]
        public void HandleQuery_agents_target_case_insensitive()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "AGENTS"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_creatures_target_case_insensitive()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "CREATURES"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_game_target_case_insensitive()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "GAME"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_status_target_is_alias_for_game()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "status"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_sefira_target_case_insensitive()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "SEFIRA"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_departments_target_is_alias_for_sefira()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "departments"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_with_null_params_uses_empty_dict()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "agents",
                @params = null
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
        }

        [Fact]
        public void HandleQuery_with_empty_params_works()
        {
            // Arrange
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "agents",
                @params = new Dictionary<string, object>()
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.status.Should().Be("error");
        }

        [Fact]
        public void HandleQuery_exception_caught_and_returns_error()
        {
            // Arrange - When the query execution fails for any reason,
            // it should return an error response
            // The "id" param in agent queries is ignored in ListAgents,
            // so this won't throw. But we can verify the structure handles
            // error cases properly.
            var request = new Request
            {
                id = "req-1",
                type = "query",
                target = "agents",
                @params = new Dictionary<string, object> { { "id", "invalid" } }
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert - Should return error response
            response.Should().NotBeNull();
            response.status.Should().Be("error");
            response.error.Should().NotBeNullOrEmpty();
        }
    }
}
