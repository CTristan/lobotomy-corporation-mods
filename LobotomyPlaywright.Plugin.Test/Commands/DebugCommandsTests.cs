// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.Commands;
using LobotomyCorporationMods.Playwright.JsonModels;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test.Commands
{
    public sealed class DebugCommandsTests
    {
        [Fact]
        public void HandleSetAgentStats_throws_when_request_is_null()
        {
            Action act = () => DebugCommands.HandleSetAgentStats(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleSetAgentStats_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = DebugCommands.HandleSetAgentStats(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleSetAgentStats_returns_error_for_invalid_agent_id()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "agentId", 0L } }
            };

            var response = DebugCommands.HandleSetAgentStats(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_AGENT_ID");
        }

        [Fact]
        public void HandleAddGift_throws_when_request_is_null()
        {
            Action act = () => DebugCommands.HandleAddGift(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleAddGift_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = DebugCommands.HandleAddGift(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleAddGift_returns_error_for_invalid_ids()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "agentId", 0L }, { "giftId", 0 } }
            };

            var response = DebugCommands.HandleAddGift(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleRemoveGift_throws_when_request_is_null()
        {
            Action act = () => DebugCommands.HandleRemoveGift(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleRemoveGift_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = DebugCommands.HandleRemoveGift(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleSetQliphoth_throws_when_request_is_null()
        {
            Action act = () => DebugCommands.HandleSetQliphoth(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleSetQliphoth_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = DebugCommands.HandleSetQliphoth(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleSetQliphoth_returns_error_for_invalid_creature_id()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "creatureId", 0L }, { "counter", 5 } }
            };

            var response = DebugCommands.HandleSetQliphoth(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_CREATURE_ID");
        }

        [Fact]
        public void HandleFillEnergy_throws_when_request_is_null()
        {
            Action act = () => DebugCommands.HandleFillEnergy(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleSetGameSpeed_throws_when_request_is_null()
        {
            Action act = () => DebugCommands.HandleSetGameSpeed(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleSetGameSpeed_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = DebugCommands.HandleSetGameSpeed(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleSetGameSpeed_returns_error_for_invalid_speed()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "speed", 10 } }
            };

            var response = DebugCommands.HandleSetGameSpeed(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleSetGameSpeed_returns_error_for_zero_speed()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "speed", 0 } }
            };

            var response = DebugCommands.HandleSetGameSpeed(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleSetAgentInvincible_throws_when_request_is_null()
        {
            Action act = () => DebugCommands.HandleSetAgentInvincible(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleSetAgentInvincible_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = DebugCommands.HandleSetAgentInvincible(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleSetAgentInvincible_returns_error_for_invalid_agent_id()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "agentId", 0L }, { "invincible", true } }
            };

            var response = DebugCommands.HandleSetAgentInvincible(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_AGENT_ID");
        }
    }
}
