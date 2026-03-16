// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.JsonModels;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test.JsonModels
{
    public sealed class RequestTests
    {
        [Fact]
        public void PascalCase_accessors_map_to_lowercase_fields()
        {
            var request = new Request
            {
                Id = "req-1",
                Type = "query",
                Target = "agents",
                Action = "list",
                Params = new Dictionary<string, object> { { "key", "value" } },
                Events = ["event1"]
            };

            request.id.Should().Be("req-1");
            request.type.Should().Be("query");
            request.target.Should().Be("agents");
            request.action.Should().Be("list");
            request.@params.Should().ContainKey("key");
            request.events.Should().Contain("event1");
        }

        [Fact]
        public void Lowercase_fields_map_to_PascalCase_accessors()
        {
            var request = new Request
            {
                id = "req-2",
                type = "command",
                target = "game",
                action = "pause"
            };

            request.Id.Should().Be("req-2");
            request.Type.Should().Be("command");
            request.Target.Should().Be("game");
            request.Action.Should().Be("pause");
        }

        [Fact]
        public void Default_values_are_null()
        {
            var request = new Request();

            request.Id.Should().BeNull();
            request.Type.Should().BeNull();
            request.Target.Should().BeNull();
            request.Action.Should().BeNull();
            request.Params.Should().BeNull();
            request.Events.Should().BeNull();
        }
    }
}
