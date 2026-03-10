// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using LobotomyPlaywright.JsonModels;
using LobotomyPlaywright.Protocol;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Protocol
{
    /// <summary>
    /// Tests for MessageSerializer.
    /// </summary>
    [Trait("Category", "RequiresUnity")]
    public class MessageSerializerTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;

        /// <summary>
        /// Tests SerializeResponse with success response produces valid JSON.
        /// </summary>
        [Fact]
        public void SerializeResponse_success_response_produces_valid_json()
        {
            if (!TestHelpers.UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            Response response = Response.CreateSuccess("req-1", new { name = "Test Agent", hp = 100 });

            // Act
            var json = MessageSerializer.Serialize(response);

            // Assert
            _ = json.Should().NotBeNullOrEmpty();
            _ = json.Should().Contain("\"id\":\"req-1\"");
            _ = json.Should().Contain("\"type\":\"response\"");
            _ = json.Should().Contain("\"status\":\"ok\"");
        }

        /// <summary>
        /// Tests SerializeResponse with error response produces valid JSON.
        /// </summary>
        [Fact]
        public void SerializeResponse_error_response_produces_valid_json()
        {
            if (!TestHelpers.UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            Response response = Response.CreateError("req-2", "Agent not found", "NOT_FOUND");

            // Act
            var json = MessageSerializer.Serialize(response);

            // Assert
            _ = json.Should().NotBeNullOrEmpty();
            _ = json.Should().Contain("\"id\":\"req-2\"");
            _ = json.Should().Contain("\"type\":\"response\"");
            _ = json.Should().Contain("\"status\":\"error\"");
            _ = json.Should().Contain("\"error\":\"Agent not found\"");
            _ = json.Should().Contain("\"code\":\"NOT_FOUND\"");
        }

        /// <summary>
        /// Tests SerializeResponse with event response produces valid JSON.
        /// </summary>
        [Fact]
        public void SerializeResponse_event_response_produces_valid_json()
        {
            if (!TestHelpers.UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            Response response = Response.CreateEvent("OnAgentDead", new { agentId = 3, agentName = "Sarah" });

            // Act
            var json = MessageSerializer.Serialize(response);

            // Assert
            _ = json.Should().NotBeNullOrEmpty();
            _ = json.Should().Contain("\"id\":null");
            _ = json.Should().Contain("\"type\":\"event\"");
            _ = json.Should().Contain("\"event\":\"OnAgentDead\"");
            _ = json.Should().Contain("\"agentId\":3");
            _ = json.Should().Contain("\"agentName\":\"Sarah\"");
            _ = json.Should().Contain("\"timestamp\"");
        }

        /// <summary>
        /// Tests DeserializeRequest with valid JSON produces request object.
        /// </summary>
        [Fact]
        public void DeserializeRequest_valid_json_produces_request_object()
        {
            if (!TestHelpers.UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var json = /*lang=json,strict*/ "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\",\"params\":{}}";

            // Act
            var request = MessageSerializer.DeserializeRequest(json);

            // Assert
            _ = request.Should().NotBeNull();
            _ = request.Id.Should().Be("req-1");
            _ = request.Type.Should().Be("query");
            _ = request.Target.Should().Be("agents");
            _ = request.Params.Should().NotBeNull();
        }

        /// <summary>
        /// Tests DeserializeRequest with params produces request with params.
        /// </summary>
        [Fact]
        public void DeserializeRequest_with_params_produces_request_with_params()
        {
            if (!TestHelpers.UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var json = /*lang=json,strict*/ "{\"id\":\"req-2\",\"type\":\"query\",\"target\":\"agents\",\"params\":{\"id\":3}}";

            // Act
            var request = MessageSerializer.DeserializeRequest(json);

            // Assert
            _ = request.Should().NotBeNull();
            _ = request.Params.Should().NotBeNull();
        }

        /// <summary>
        /// Tests DeserializeRequest with null JSON throws exception.
        /// </summary>
        [Fact]
        public void DeserializeRequest_null_json_throws_exception()
        {
            if (!TestHelpers.UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            string json = null;

            // Act
            Action act = () => MessageSerializer.DeserializeRequest(json);

            // Assert
            _ = act.Should().Throw<ArgumentNullException>()
                .WithParameterName("json");
        }

        /// <summary>
        /// Tests DeserializeRequest with empty JSON throws exception.
        /// </summary>
        [Fact]
        public void DeserializeRequest_empty_json_throws_exception()
        {
            if (!TestHelpers.UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var json = "";

            // Act
            Action act = () => MessageSerializer.DeserializeRequest(json);

            // Assert
            _ = act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Tests DeserializeRequest with invalid JSON throws exception.
        /// </summary>
        [Fact]
        public void DeserializeRequest_invalid_json_throws_exception()
        {
            if (!TestHelpers.UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            var json = "not valid json";

            // Act
            Action act = () => MessageSerializer.DeserializeRequest(json);

            // Assert
            _ = act.Should().Throw<InvalidOperationException>();
        }

        /// <summary>
        /// Tests Serialize deserialize round trip preserves data.
        /// </summary>
        [Fact]
        public void Serialize_deserialize_round_trip_preserves_data()
        {
            if (!TestHelpers.UnityTestHelper.IsUnityAvailable)
            {
                _output.WriteLine("Skipping test - Unity runtime not available");
                return;
            }

            // Arrange
            Response originalResponse = Response.CreateSuccess("req-1", new { name = "Test", value = 42 });

            // Act
            var json = MessageSerializer.Serialize(originalResponse);
            var deserializedRequest = MessageSerializer.DeserializeRequest(json);

            // Assert
            _ = deserializedRequest.Should().NotBeNull();
            _ = deserializedRequest.Id.Should().Be("req-1");
            _ = deserializedRequest.Type.Should().Be("response");
        }
    }
}
