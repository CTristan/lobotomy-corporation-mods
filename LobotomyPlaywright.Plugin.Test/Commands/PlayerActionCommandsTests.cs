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
    public sealed class PlayerActionCommandsTests
    {
        [Fact]
        public void HandlePause_throws_when_request_is_null()
        {
            Action act = () => PlayerActionCommands.HandlePause(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleUnpause_throws_when_request_is_null()
        {
            Action act = () => PlayerActionCommands.HandleUnpause(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleAssignWork_throws_when_request_is_null()
        {
            Action act = () => PlayerActionCommands.HandleAssignWork(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleAssignWork_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = PlayerActionCommands.HandleAssignWork(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleAssignWork_returns_error_for_invalid_ids()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object>
                {
                    { "agentId", 0L },
                    { "creatureId", 0L },
                    { "workType", "instinct" }
                }
            };

            var response = PlayerActionCommands.HandleAssignWork(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleAssignWork_returns_error_for_missing_work_type()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object>
                {
                    { "agentId", 1L },
                    { "creatureId", 1L },
                    { "workType", "" }
                }
            };

            var response = PlayerActionCommands.HandleAssignWork(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleDeployAgent_throws_when_request_is_null()
        {
            Action act = () => PlayerActionCommands.HandleDeployAgent(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleDeployAgent_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = PlayerActionCommands.HandleDeployAgent(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleDeployAgent_returns_error_for_invalid_agent_id()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "agentId", 0L }, { "sefira", "MALKUTH" } }
            };

            var response = PlayerActionCommands.HandleDeployAgent(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_AGENT_ID");
        }

        [Fact]
        public void HandleDeployAgent_returns_error_for_missing_sefira()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "agentId", 1L }, { "sefira", "" } }
            };

            var response = PlayerActionCommands.HandleDeployAgent(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleRecallAgent_throws_when_request_is_null()
        {
            Action act = () => PlayerActionCommands.HandleRecallAgent(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleRecallAgent_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = PlayerActionCommands.HandleRecallAgent(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleRecallAgent_returns_error_for_invalid_agent_id()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "agentId", 0L } }
            };

            var response = PlayerActionCommands.HandleRecallAgent(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_AGENT_ID");
        }

        [Fact]
        public void HandleSuppress_throws_when_request_is_null()
        {
            Action act = () => PlayerActionCommands.HandleSuppress(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleSuppress_returns_error_when_params_is_null()
        {
            var request = new Request { Id = "req-1", Params = null };

            var response = PlayerActionCommands.HandleSuppress(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_PARAMS");
        }

        [Fact]
        public void HandleSuppress_returns_error_for_invalid_creature_id()
        {
            var request = new Request
            {
                Id = "req-1",
                Params = new Dictionary<string, object> { { "creatureId", 0L } }
            };

            var response = PlayerActionCommands.HandleSuppress(request);

            response.Status.Should().Be("error");
            response.Code.Should().Be("INVALID_CREATURE_ID");
        }
    }
}
