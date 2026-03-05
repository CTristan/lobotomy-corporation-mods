// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FluentAssertions;
using LobotomyPlaywright.Server;
using Xunit;
using Xunit.Abstractions;

namespace LobotomyPlaywright.Plugin.Test.Server
{
    public sealed class ClientHandlerTests : IDisposable
    {
        private readonly TcpServer _server;
        private readonly int _port = 18495;

        public ClientHandlerTests(ITestOutputHelper output)
        {
            _server = new TcpServer(_port);
            _server.Start();
        }

        public void Dispose()
        {
            _server?.Stop();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void ClientHandler_with_valid_tcp_client_starts_successfully()
        {
            // Arrange
            var tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, _port);

            // Give server time to accept
            Thread.Sleep(100);

            // Act & Assert - if we got here, the client connected successfully
            tcpClient.Connected.Should().BeTrue();

            // Cleanup
            tcpClient.Close();

            // Give server time to process disconnect
            Thread.Sleep(100);
        }

        [Fact]
        public void ClientHandler_disconnects_gracefully_on_client_close()
        {
            // Arrange
            var tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, _port);

            Thread.Sleep(100);

            // Act
            tcpClient.Close();

            // Give server time to process disconnect
            Thread.Sleep(100);

            // Assert
            _server.ClientCount.Should().Be(0);
        }

        [Fact]
        public void ClientHandler_can_send_json_request()
        {
            // Arrange
            var tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            var request = "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\"}";
            var requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            var stream = tcpClient.GetStream();
            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Flush();

            // Assert - Response should be received (even if error due to no game running)
            var buffer = new byte[4096];
            var readCount = stream.Read(buffer, 0, buffer.Length);

            readCount.Should().BeGreaterThan(0);
            var response = Encoding.UTF8.GetString(buffer, 0, readCount);
            response.Should().Contain("\"type\":\"response\"");

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_handles_multiple_requests()
        {
            // Arrange
            var tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            var stream = tcpClient.GetStream();

            // Act - Send multiple requests
            var requests = new[]
            {
                "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\"}",
                "{\"id\":\"req-2\",\"type\":\"query\",\"target\":\"creatures\"}",
                "{\"id\":\"req-3\",\"type\":\"query\",\"target\":\"game\"}"
            };

            foreach (var request in requests)
            {
                var requestBytes = Encoding.UTF8.GetBytes(request);
                stream.Write(requestBytes, 0, requestBytes.Length);
                stream.Flush();

                // Read response
                var buffer = new byte[4096];
                var readCount = stream.Read(buffer, 0, buffer.Length);

                readCount.Should().BeGreaterThan(0);
            }

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_handles_malformed_json_gracefully()
        {
            // Arrange
            var tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            var malformedRequest = "not valid json";
            var requestBytes = Encoding.UTF8.GetBytes(malformedRequest);

            // Act
            var stream = tcpClient.GetStream();
            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Flush();

            // Assert - Connection should still be open for next request
            tcpClient.Connected.Should().BeTrue();

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_with_large_request_handles_gracefully()
        {
            // Arrange
            var tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            // Create a request with a large params object
            var largeParams = new string('x', 10000);
            var request = $"{{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\",\"params\":{{\"data\":\"{largeParams}\"}}}}";
            var requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            var stream = tcpClient.GetStream();
            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Flush();

            // Assert - Should still receive a response
            var buffer = new byte[4096];
            var readCount = stream.Read(buffer, 0, buffer.Length);

            readCount.Should().BeGreaterThan(0);

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_json_line_protocol_uses_newline_delimiter()
        {
            // Arrange
            var tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            var request = "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\"}\n";
            var requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            var stream = tcpClient.GetStream();
            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Flush();

            // Assert
            var buffer = new byte[4096];
            var readCount = stream.Read(buffer, 0, buffer.Length);

            readCount.Should().BeGreaterThan(0);
            var response = Encoding.UTF8.GetString(buffer, 0, readCount);
            response.Should().EndWith("\n");

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_handles_concurrent_clients()
        {
            // Arrange
            var client1 = new TcpClient();
            var client2 = new TcpClient();

            client1.Connect(IPAddress.Loopback, _port);
            client2.Connect(IPAddress.Loopback, _port);

            client1.ReceiveTimeout = 5000;
            client2.ReceiveTimeout = 5000;
            client1.SendTimeout = 5000;
            client2.SendTimeout = 5000;

            Thread.Sleep(100);

            // Act - Send requests from both clients
            var request = "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\"}\n";
            var requestBytes = Encoding.UTF8.GetBytes(request);

            var stream1 = client1.GetStream();
            var stream2 = client2.GetStream();

            stream1.Write(requestBytes, 0, requestBytes.Length);
            stream1.Flush();

            stream2.Write(requestBytes, 0, requestBytes.Length);
            stream2.Flush();

            // Assert - Both clients should receive responses
            var buffer1 = new byte[4096];
            var readCount1 = stream1.Read(buffer1, 0, buffer1.Length);

            var buffer2 = new byte[4096];
            var readCount2 = stream2.Read(buffer2, 0, buffer2.Length);

            readCount1.Should().BeGreaterThan(0);
            readCount2.Should().BeGreaterThan(0);

            // Cleanup
            client1.Close();
            client2.Close();
        }
    }
}
