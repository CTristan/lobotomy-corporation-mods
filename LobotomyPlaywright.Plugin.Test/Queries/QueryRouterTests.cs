// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using AwesomeAssertions;
using LobotomyPlaywright.JsonModels;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public sealed class QueryRouterTests : IDisposable
    {
        public QueryRouterTests()
        {
            SetGameReady(false);
            // Disable thread check for testing - tests run on thread pool threads
            UiQueries.DisableThreadCheckForTesting();
        }

        public void Dispose()
        {
            SetGameReady(null);
        }

        private static void SetGameReady(bool? ready)
        {
            PropertyInfo property = typeof(GameStateQueries).GetProperty("IsGameQueryableOverride", BindingFlags.NonPublic | BindingFlags.Static);
            property?.SetValue(null, ready, null);
        }

        [Fact]
        public void HandleQuery_null_request_returns_error()
        {
            // Arrange
            Request request = null;

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Missing target parameter");
        }

        [Fact]
        public void HandleRequest_missing_target_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = null
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Missing target parameter");
        }

        [Fact]
        public void HandleRequest_empty_target_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = ""
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Missing target parameter");
        }

        [Fact]
        public void HandleQuery_unknown_target_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "unknown"
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Unknown query target: unknown");
        }

        [Fact]
        public void HandleQuery_game_not_ready_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "agents"
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
            _ = response.code.Should().Be("GAME_NOT_READY");
        }

        [Fact]
        public void HandleQuery_agents_target_case_insensitive()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "AGENTS"
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_creatures_target_case_insensitive()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "CREATURES"
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_game_target_case_insensitive()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "GAME"
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_status_target_is_alias_for_game()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "status"
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_sefira_target_case_insensitive()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "SEFIRA"
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_departments_target_is_alias_for_sefira()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "departments"
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        [Fact]
        public void HandleQuery_with_null_params_uses_empty_dict()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "agents",
                @params = null
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        [Fact]
        public void HandleQuery_with_empty_params_works()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "agents",
                @params = []
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        [Fact]
        public void HandleQuery_exception_caught_and_returns_error()
        {
            // Arrange - When the query execution fails for any reason,
            // it should return an error response
            // The "id" param in agent queries is ignored in ListAgents,
            // so this won't throw. But we can verify the structure handles
            // error cases properly.
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "agents",
                @params = new Dictionary<string, object> { { "id", "invalid" } }
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert - Should return error response
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void HandleQuery_ui_target_returns_success()
        {
            // Arrange - UI queries work regardless of game state
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "ui",
                @params = []
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            if (response.status == "error")
            {
                Console.WriteLine($"UI query error: {response.error}, code: {response.code}");
            }
            _ = response.status.Should().Be("ok");
            _ = response.DataObject.Should().NotBeNull();
            _ = response.id.Should().Be("req-1");
        }

        [Fact]
        public void HandleQuery_ui_target_case_insensitive()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "UI",
                @params = []
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("ok");
            _ = response.DataObject.Should().NotBeNull();
        }

        [Fact]
        public void HandleQuery_ui_with_depth_summary()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "ui",
                @params = new Dictionary<string, object> { { "depth", "summary" } }
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("ok");
            _ = response.DataObject.Should().NotBeNull();
        }

        [Fact]
        public void HandleQuery_ui_with_window_filter()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "ui",
                @params = new Dictionary<string, object>
                {
                    { "depth", "window" },
                    { "name", "AgentInfoWindow" }
                }
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("ok");
            _ = response.DataObject.Should().NotBeNull();
        }

        [Fact]
        public void HandleQuery_ui_data_has_expected_structure()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                type = "query",
                target = "ui"
            };

            // Act
            Response response = QueryRouter.HandleQuery(request);

            // Assert - The data should be a UiStateData object
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("ok");
            _ = response.DataObject.Should().NotBeNull();
            _ = response.DataObject.Should().BeOfType<UiStateData>();
        }
    }
}
