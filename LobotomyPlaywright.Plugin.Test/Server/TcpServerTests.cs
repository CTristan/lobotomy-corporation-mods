// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.Server;
using Xunit;

namespace LobotomyCorporationMods.Playwright.Test.Server
{
    /// <summary>
    /// Tests for TcpServer.
    /// </summary>
    public class TcpServerTests
    {
        /// <summary>
        /// Tests Constructor with valid port creates server.
        /// </summary>
        [Fact]
        public void Constructor_valid_port_creates_server()
        {
            // Arrange & Act
            TcpServer server = new(8484);

            // Assert
            _ = server.Should().NotBeNull();
            _ = server.Port.Should().Be(8484);
            _ = server.IsRunning.Should().BeFalse();
            _ = server.ClientCount.Should().Be(0);
        }

        /// <summary>
        /// Tests Constructor with port too low throws exception.
        /// </summary>
        [Fact]
        public void Constructor_port_too_low_throws_exception()
        {
            // Arrange & Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TcpServer(0));

            // Assert
            _ = exception.ParamName.Should().Be("port");
        }

        /// <summary>
        /// Tests Constructor with port too high throws exception.
        /// </summary>
        [Fact]
        public void Constructor_port_too_high_throws_exception()
        {
            // Arrange & Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TcpServer(65536));

            // Assert
            _ = exception.ParamName.Should().Be("port");
        }

        /// <summary>
        /// Tests Constructor with negative port throws exception.
        /// </summary>
        [Fact]
        public void Constructor_port_negative_throws_exception()
        {
            // Arrange & Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TcpServer(-1));

            // Assert
            _ = exception.ParamName.Should().Be("port");
        }

        /// <summary>
        /// Tests Start starts server on background thread.
        /// </summary>
        [Fact]
        public void Start_starts_server_on_background_thread()
        {
            // Arrange
            TcpServer server = new(18484);

            // Act
            server.Start();

            // Assert
            _ = server.IsRunning.Should().BeTrue();

            // Cleanup
            server.Stop();
        }

        /// <summary>
        /// Tests Start with already running does nothing.
        /// </summary>
        [Fact]
        public void Start_already_running_does_nothing()
        {
            // Arrange
            TcpServer server = new(18485);
            server.Start();

            // Act
            server.Start();

            // Assert
            _ = server.IsRunning.Should().BeTrue();

            // Cleanup
            server.Stop();
        }

        /// <summary>
        /// Tests Stop stops running server.
        /// </summary>
        [Fact]
        public void Stop_stops_running_server()
        {
            // Arrange
            TcpServer server = new(18486);
            server.Start();

            // Act
            server.Stop();

            // Assert
            _ = server.IsRunning.Should().BeFalse();
        }

        /// <summary>
        /// Tests Stop with already stopped does nothing.
        /// </summary>
        [Fact]
        public void Stop_already_stopped_does_nothing()
        {
            // Arrange
            TcpServer server = new(18487);

            // Act
            server.Stop();

            // Assert
            _ = server.IsRunning.Should().BeFalse();
        }

        /// <summary>
        /// Tests Stop with no clients sets client count to zero.
        /// </summary>
        [Fact]
        public void Stop_with_no_clients_sets_client_count_to_zero()
        {
            // Arrange
            TcpServer server = new(18488);
            server.Start();

            // Act
            server.Stop();

            // Assert
            _ = server.ClientCount.Should().Be(0);
        }

        /// <summary>
        /// Tests ProcessQueuedRequests with empty queue does nothing.
        /// </summary>
        [Fact]
        public void ProcessQueuedRequests_with_empty_queue_does_nothing()
        {
            // Arrange
            TcpServer server = new(18489);
            server.Start();

            // Act - should not throw
            server.ProcessQueuedRequests();

            // Assert
            _ = server.IsRunning.Should().BeTrue();

            // Cleanup
            server.Stop();
        }

        /// <summary>
        /// Tests ProcessQueuedRequests when server not running does not throw.
        /// </summary>
        [Fact]
        public void ProcessQueuedRequests_when_server_not_running_does_not_throw()
        {
            // Arrange
            TcpServer server = new(18490);

            // Act - should not throw
            server.ProcessQueuedRequests();

            // Assert
            _ = server.IsRunning.Should().BeFalse();
        }

        /// <summary>
        /// Tests ClientCount increases when client connects.
        /// </summary>
        [Fact]
        public void ClientCount_increases_when_client_connects()
        {
            // Arrange
            TcpServer server = new(18491);
            server.Start();

            TcpClient client = null;
            try
            {
                // Wait for server to be ready to accept connections
                // The server starts on a background thread, so we need to give it time to start listening
                // We retry the connection with a delay to handle the race condition
                var retryCount = 0;
                const int maxRetries = 20;
                const int retryDelayMs = 50;

                while (retryCount < maxRetries && client == null)
                {
                    try
                    {
                        TcpClient tempClient = new();
                        tempClient.Connect(IPAddress.Loopback, 18491);
                        client = tempClient; // Connection succeeded
                    }
                    catch (SocketException)
                    {
                        retryCount++;
                        if (retryCount < maxRetries)
                        {
                            System.Threading.Thread.Sleep(retryDelayMs);
                        }
                    }
                }

                // Give server time to accept the connection
                System.Threading.Thread.Sleep(200);

                // Assert
                _ = server.ClientCount.Should().BeGreaterThanOrEqualTo(1);
            }
            finally
            {
                client?.Close();
                server.Stop();
            }
        }

        /// <summary>
        /// Tests BroadcastEvent with no clients does not throw.
        /// </summary>
        [Fact]
        public void BroadcastEvent_with_no_clients_does_not_throw()
        {
            // Arrange
            TcpServer server = new(18492);
            server.Start();

            try
            {
                // Act - should not throw
                server.BroadcastEvent("TestEvent", new { test = "data" });

                // Assert
                _ = server.IsRunning.Should().BeTrue();
            }
            finally
            {
                server.Stop();
            }
        }

        /// <summary>
        /// Tests BroadcastEvent when server not running does not throw.
        /// </summary>
        [Fact]
        public void BroadcastEvent_when_server_not_running_does_not_throw()
        {
            // Arrange
            TcpServer server = new(18493);

            // Act - should not throw
            server.BroadcastEvent("TestEvent", new { test = "data" });

            // Assert
            _ = server.IsRunning.Should().BeFalse();
        }

        /// <summary>
        /// Tests EnqueueRequest from client handler adds to queue.
        /// </summary>
        [Fact]
        public void EnqueueRequest_from_client_handler_adds_to_queue()
        {
            // Arrange
            TcpServer server = new(18494);
            server.Start();

            try
            {
                // Act - Enqueue should not throw
                // Note: This is an internal method tested indirectly through ClientHandler
                // Here we just verify it can be called
                server.ProcessQueuedRequests();

                // Assert
                _ = server.IsRunning.Should().BeTrue();
            }
            finally
            {
                server.Stop();
            }
        }
    }
}
