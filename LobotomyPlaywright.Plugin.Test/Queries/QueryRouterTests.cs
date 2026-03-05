// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using FluentAssertions;
using LobotomyPlaywright.Protocol;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public class QueryRouterTests
    {
        [Fact]
        public void HandleQuery_null_request_returns_error()
        {
            // Arrange
            Request request = null;

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Missing target");
        }

        [Fact]
        public void HandleRequest_missing_target_returns_error()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = null
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Missing target");
        }

        [Fact]
        public void HandleRequest_empty_target_returns_error()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = ""
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Missing target");
        }

        [Fact]
        public void HandleQuery_unknown_target_returns_error()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "unknown"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Unknown query target");
        }

        [Fact]
        public void HandleQuery_game_not_ready_returns_error()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "agents"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Game is not in a queryable state");
            response.Code.Should().Be("GAME_NOT_READY");
        }

        [Fact]
        public void HandleQuery_agents_target_case_insensitive()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "AGENTS"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_creatures_target_case_insensitive()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "CREATURES"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_game_target_case_insensitive()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "GAME"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_status_target_is_alias_for_game()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "status"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_sefira_target_case_insensitive()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "SEFIRA"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_departments_target_is_alias_for_sefira()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "departments"
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_with_null_params_uses_empty_dict()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "agents",
                Params = null
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
        }

        [Fact]
        public void HandleQuery_with_empty_params_works()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "agents",
                Params = new Dictionary<string, object>()
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
        }

        [Fact]
        public void HandleQuery_exception_caught_and_returns_error()
        {
            // Arrange
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "agents",
                Params = new Dictionary<string, object> { { "id", "invalid" } }
            };

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be("error");
            response.Error.Should().Contain("Query failed");
            response.Code.Should().Be("QUERY_ERROR");
        }
    }
}
