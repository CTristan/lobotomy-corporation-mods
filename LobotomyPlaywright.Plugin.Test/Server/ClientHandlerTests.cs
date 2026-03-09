// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using AwesomeAssertions;
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
            TcpClient tcpClient = new();
            tcpClient.Connect(IPAddress.Loopback, _port);

            // Give server time to accept
            Thread.Sleep(100);

            // Act & Assert - if we got here, the client connected successfully
            _ = tcpClient.Connected.Should().BeTrue();

            // Cleanup
            tcpClient.Close();

            // Give server time to process disconnect
            Thread.Sleep(100);
        }

        [Fact]
        public void ClientHandler_disconnects_gracefully_on_client_close()
        {
            // Arrange
            TcpClient tcpClient = new();
            tcpClient.Connect(IPAddress.Loopback, _port);

            Thread.Sleep(100);

            // Act
            tcpClient.Close();

            // Give server time to process disconnect
            Thread.Sleep(100);

            // Assert
            _ = _server.ClientCount.Should().Be(0);
        }

        [Fact]
        public void ClientHandler_can_send_json_request()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                return;
            }

            // Arrange
            TcpClient tcpClient = new();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            string request = /*lang=json,strict*/ "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\"}";
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            NetworkStream stream = tcpClient.GetStream();
            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Flush();

            // Assert - Response should be received (even if error due to no game running)
            byte[] buffer = new byte[4096];
            int readCount = stream.Read(buffer, 0, buffer.Length);

            _ = readCount.Should().BeGreaterThan(0);
            string response = Encoding.UTF8.GetString(buffer, 0, readCount);
            _ = response.Should().Contain("\"type\":\"response\"");

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_handles_multiple_requests()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                return;
            }

            // Arrange
            TcpClient tcpClient = new();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            NetworkStream stream = tcpClient.GetStream();

            // Act - Send multiple requests
            string[] requests =
            [
                /*lang=json,strict*/
                                     "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\"}",
                /*lang=json,strict*/
                                     "{\"id\":\"req-2\",\"type\":\"query\",\"target\":\"creatures\"}",
                /*lang=json,strict*/
                                     "{\"id\":\"req-3\",\"type\":\"query\",\"target\":\"game\"}"
            ];

            foreach (string request in requests)
            {
                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                stream.Write(requestBytes, 0, requestBytes.Length);
                stream.Flush();

                // Read response
                byte[] buffer = new byte[4096];
                int readCount = stream.Read(buffer, 0, buffer.Length);

                _ = readCount.Should().BeGreaterThan(0);
            }

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_handles_malformed_json_gracefully()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                return;
            }

            // Arrange
            TcpClient tcpClient = new();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            string malformedRequest = "not valid json";
            byte[] requestBytes = Encoding.UTF8.GetBytes(malformedRequest);

            // Act
            NetworkStream stream = tcpClient.GetStream();
            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Flush();

            // Assert - Connection should still be open for next request
            _ = tcpClient.Connected.Should().BeTrue();

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_with_large_request_handles_gracefully()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                return;
            }

            // Arrange
            TcpClient tcpClient = new();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            // Create a request with a large params object
            string largeParams = new('x', 10000);
            string request = $"{{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\",\"params\":{{\"data\":\"{largeParams}\"}}}}";
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            NetworkStream stream = tcpClient.GetStream();
            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Flush();

            // Assert - Should still receive a response
            byte[] buffer = new byte[4096];
            int readCount = stream.Read(buffer, 0, buffer.Length);

            _ = readCount.Should().BeGreaterThan(0);

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_json_line_protocol_uses_newline_delimiter()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                return;
            }

            // Arrange
            TcpClient tcpClient = new();
            tcpClient.Connect(IPAddress.Loopback, _port);
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.SendTimeout = 5000;

            Thread.Sleep(100);

            string request = /*lang=json,strict*/ "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\"}\n";
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);

            // Act
            NetworkStream stream = tcpClient.GetStream();
            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Flush();

            // Assert
            byte[] buffer = new byte[4096];
            int readCount = stream.Read(buffer, 0, buffer.Length);

            _ = readCount.Should().BeGreaterThan(0);
            string response = Encoding.UTF8.GetString(buffer, 0, readCount);
            _ = response.Should().EndWith("\n");

            // Cleanup
            tcpClient.Close();
        }

        [Fact]
        public void ClientHandler_handles_concurrent_clients()
        {
            if (!UnityTestHelper.IsUnityAvailable)
            {
                return;
            }

            // Arrange
            TcpClient client1 = new();
            TcpClient client2 = new();

            client1.Connect(IPAddress.Loopback, _port);
            client2.Connect(IPAddress.Loopback, _port);

            client1.ReceiveTimeout = 5000;
            client2.ReceiveTimeout = 5000;
            client1.SendTimeout = 5000;
            client2.SendTimeout = 5000;

            Thread.Sleep(100);

            // Act - Send requests from both clients
            string request = /*lang=json,strict*/ "{\"id\":\"req-1\",\"type\":\"query\",\"target\":\"agents\"}\n";
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);

            NetworkStream stream1 = client1.GetStream();
            NetworkStream stream2 = client2.GetStream();

            stream1.Write(requestBytes, 0, requestBytes.Length);
            stream1.Flush();

            stream2.Write(requestBytes, 0, requestBytes.Length);
            stream2.Flush();

            // Assert - Both clients should receive responses
            byte[] buffer1 = new byte[4096];
            int readCount1 = stream1.Read(buffer1, 0, buffer1.Length);

            byte[] buffer2 = new byte[4096];
            int readCount2 = stream2.Read(buffer2, 0, buffer2.Length);

            _ = readCount1.Should().BeGreaterThan(0);
            _ = readCount2.Should().BeGreaterThan(0);

            // Cleanup
            client1.Close();
            client2.Close();
        }
    }
}
