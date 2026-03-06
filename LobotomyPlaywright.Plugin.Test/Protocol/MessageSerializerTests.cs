// SPDX-License-Identifier: MIT

using System;
using FluentAssertions;
using LobotomyPlaywright.Protocol;
using Xunit;
using Xunit.Abstractions;

namespace LobotomyPlaywright.Plugin.Test.Protocol
{
    [Trait("Category", "RequiresUnity")]
    public class MessageSerializerTests
    {
        private readonly ITestOutputHelper _output;

        public MessageSerializerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SerializeResponse_success_response_produces_valid_json()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var response = Response.CreateSuccess("req-1", new { name = "Test Agent", hp = 100 });

            // Act
            var json = MessageSerializer.Serialize(response);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"id\":\"req-1\"");
            json.Should().Contain("\"type\":\"response\"");
            json.Should().Contain("\"status\":\"ok\"");
        }

        [Fact]
        public void SerializeResponse_error_response_produces_valid_json()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var response = Response.CreateError("req-2", "Agent not found", "NOT_FOUND");

            // Act
            var json = MessageSerializer.Serialize(response);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"id\":\"req-2\"");
            json.Should().Contain("\"type\":\"response\"");
            json.Should().Contain("\"status\":\"error\"");
            json.Should().Contain("\"error\":\"Agent not found\"");
            json.Should().Contain("\"code\":\"NOT_FOUND\"");
        }

        [Fact]
        public void SerializeResponse_event_response_produces_valid_json()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var response = Response.CreateEvent("OnAgentDead", new { agentId = 3, agentName = "Sarah" });

            // Act
            var json = MessageSerializer.Serialize(response);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"id\":null");
            json.Should().Contain("\"type\":\"event\"");
            json.Should().Contain("\"event\":\"OnAgentDead\"");
            json.Should().Contain("\"agentId\":3");
            json.Should().Contain("\"agentName\":\"Sarah\"");
            json.Should().Contain("\"timestamp\"");
        }

        [Fact]
        public void DeserializeRequest_valid_json_produces_request_object()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var json = "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\",\"params\":{}}";

            // Act
            var request = MessageSerializer.DeserializeRequest(json);

            // Assert
            request.Should().NotBeNull();
            request.Id.Should().Be("req-1");
            request.Type.Should().Be("query");
            request.Target.Should().Be("agents");
            request.Params.Should().NotBeNull();
        }

        [Fact]
        public void DeserializeRequest_with_params_produces_request_with_params()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var json = "{\"id\":\"req-2\",\"type\":\"query\",\"target\":\"agents\",\"params\":{\"id\":3}}";

            // Act
            var request = MessageSerializer.DeserializeRequest(json);

            // Assert
            request.Should().NotBeNull();
            request.Params.Should().NotBeNull();
        }

        [Fact]
        public void DeserializeRequest_null_json_throws_exception()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            string json = null;

            // Act
            Action act = () => MessageSerializer.DeserializeRequest(json);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("json");
        }

        [Fact]
        public void DeserializeRequest_empty_json_throws_exception()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var json = "";

            // Act
            Action act = () => MessageSerializer.DeserializeRequest(json);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DeserializeRequest_invalid_json_throws_exception()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var json = "not valid json";

            // Act
            Action act = () => MessageSerializer.DeserializeRequest(json);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Serialize_deserialize_round_trip_preserves_data()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var originalResponse = Response.CreateSuccess("req-1", new { name = "Test", value = 42 });

            // Act
            var json = MessageSerializer.Serialize(originalResponse);
            var deserializedRequest = MessageSerializer.DeserializeRequest(json);

            // Assert
            deserializedRequest.Should().NotBeNull();
            deserializedRequest.Id.Should().Be("req-1");
            deserializedRequest.Type.Should().Be("response");
        }
    }
}
