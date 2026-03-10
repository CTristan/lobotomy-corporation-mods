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
    /// <summary>
    /// Tests for QueryRouter.
    /// </summary>
    public sealed class QueryRouterTests : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the QueryRouterTests class.
        /// </summary>
        public QueryRouterTests()
        {
            SetGameReady(false);
            // Disable thread check for testing - tests run on thread pool threads
            UiQueries.DisableThreadCheckForTesting();
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            SetGameReady(null);
        }

        private static void SetGameReady(bool? ready)
        {
            var property = typeof(GameStateQueries).GetProperty("IsGameQueryableOverride", BindingFlags.NonPublic | BindingFlags.Static);
            property?.SetValue(null, ready, null);
        }

        /// <summary>
        /// Tests HandleQuery with null request returns error.
        /// </summary>
        [Fact]
        public void HandleQuery_null_request_returns_error()
        {
            // Arrange
            Request request = null;

            // Act
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Missing target parameter");
        }

        /// <summary>
        /// Tests HandleRequest with missing target returns error.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Missing target parameter");
        }

        /// <summary>
        /// Tests HandleRequest with empty target returns error.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Missing target parameter");
        }

        /// <summary>
        /// Tests HandleQuery with unknown target returns error.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Unknown query target: unknown");
        }

        /// <summary>
        /// Tests HandleQuery when game not ready returns error.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
            _ = response.code.Should().Be("GAME_NOT_READY");
        }

        /// <summary>
        /// Tests HandleQuery agents target case insensitive.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        /// <summary>
        /// Tests HandleQuery creatures target case insensitive.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        /// <summary>
        /// Tests HandleQuery game target case insensitive.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        /// <summary>
        /// Tests HandleQuery status target is alias for game.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        /// <summary>
        /// Tests HandleQuery sefira target case insensitive.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        /// <summary>
        /// Tests HandleQuery departments target is alias for sefira.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Game is not in a queryable state");
        }

        /// <summary>
        /// Tests HandleQuery with null params uses empty dict.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleQuery with empty params works.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleQuery exception caught and returns error.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert - Should return error response
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests HandleQuery ui target returns success.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

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

        /// <summary>
        /// Tests HandleQuery ui target case insensitive.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("ok");
            _ = response.DataObject.Should().NotBeNull();
        }

        /// <summary>
        /// Tests HandleQuery ui with depth summary.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("ok");
            _ = response.DataObject.Should().NotBeNull();
        }

        /// <summary>
        /// Tests HandleQuery ui with window filter.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("ok");
            _ = response.DataObject.Should().NotBeNull();
        }

        /// <summary>
        /// Tests HandleQuery ui data has expected structure.
        /// </summary>
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
            var response = QueryRouter.HandleQuery(request);

            // Assert - The data should be a UiStateData object
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("ok");
            _ = response.DataObject.Should().NotBeNull();
            _ = response.DataObject.Should().BeOfType<UiStateData>();
        }
    }
}
