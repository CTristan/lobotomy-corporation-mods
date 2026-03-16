// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.JsonModels;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test.JsonModels
{
    public sealed class ResponseTests
    {
        [Fact]
        public void CreateSuccess_sets_ok_status_and_response_type()
        {
            var response = Response.CreateSuccess("req-1", new { value = 42 });

            response.Status.Should().Be("ok");
            response.Type.Should().Be("response");
            response.Id.Should().Be("req-1");
            response.DataObject.Should().NotBeNull();
        }

        [Fact]
        public void CreateSuccess_with_null_id_sets_null_id()
        {
            var response = Response.CreateSuccess(null, "data");

            response.Id.Should().BeNull();
            response.Status.Should().Be("ok");
        }

        [Fact]
        public void CreateError_sets_error_status_and_message()
        {
            var response = Response.CreateError("req-2", "Something failed", "FAIL_CODE");

            response.Status.Should().Be("error");
            response.Type.Should().Be("response");
            response.Id.Should().Be("req-2");
            response.Error.Should().Be("Something failed");
            response.Code.Should().Be("FAIL_CODE");
        }

        [Fact]
        public void CreateError_with_null_code_defaults_to_unknown_error()
        {
            var response = Response.CreateError("req-3", "Error message");

            response.Code.Should().Be("UNKNOWN_ERROR");
        }

        [Fact]
        public void CreateEvent_sets_event_type_and_timestamp()
        {
            var response = Response.CreateEvent("agent_dead", new { agentId = 1 });

            response.Type.Should().Be("event");
            response.Event.Should().Be("agent_dead");
            response.DataObject.Should().NotBeNull();
            response.Id.Should().BeNull();
            response.Timestamp.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void PascalCase_accessors_map_to_lowercase_fields()
        {
            var response = new Response
            {
                Id = "test-id",
                Type = "response",
                Status = "ok",
                Data = "some-data",
                Error = "some-error",
                Code = "some-code",
                Event = "some-event",
                Timestamp = "2026-01-01T00:00:00Z"
            };

            response.id.Should().Be("test-id");
            response.type.Should().Be("response");
            response.status.Should().Be("ok");
            response.data.Should().Be("some-data");
            response.error.Should().Be("some-error");
            response.code.Should().Be("some-code");
            response.@event.Should().Be("some-event");
            response.timestamp.Should().Be("2026-01-01T00:00:00Z");
        }
    }
}
