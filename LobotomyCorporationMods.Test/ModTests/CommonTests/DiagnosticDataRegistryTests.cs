// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CommonTests
{
    public sealed class DiagnosticDataRegistryTests : IDisposable
    {
        public DiagnosticDataRegistryTests()
        {
            DiagnosticDataRegistry.Unregister();
        }

        public void Dispose()
        {
            DiagnosticDataRegistry.Unregister();
        }

        [Fact]
        public void IsRegistered_returns_false_when_no_provider_registered()
        {
            DiagnosticDataRegistry.IsRegistered.Should().BeFalse();
        }

        [Fact]
        public void IsRegistered_returns_true_after_provider_registered()
        {
            var mockProvider = new Mock<IDiagnosticDataProvider>();

            DiagnosticDataRegistry.Register(mockProvider.Object);

            DiagnosticDataRegistry.IsRegistered.Should().BeTrue();
        }

        [Fact]
        public void Provider_returns_null_when_no_provider_registered()
        {
            DiagnosticDataRegistry.Provider.Should().BeNull();
        }

        [Fact]
        public void Provider_returns_registered_provider()
        {
            var mockProvider = new Mock<IDiagnosticDataProvider>();

            DiagnosticDataRegistry.Register(mockProvider.Object);

            DiagnosticDataRegistry.Provider.Should().BeSameAs(mockProvider.Object);
        }

        [Fact]
        public void Register_throws_when_provider_is_null()
        {
            Action act = () => DiagnosticDataRegistry.Register(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("provider");
        }

        [Fact]
        public void Unregister_clears_registered_provider()
        {
            var mockProvider = new Mock<IDiagnosticDataProvider>();
            DiagnosticDataRegistry.Register(mockProvider.Object);

            DiagnosticDataRegistry.Unregister();

            DiagnosticDataRegistry.IsRegistered.Should().BeFalse();
            DiagnosticDataRegistry.Provider.Should().BeNull();
        }

        [Fact]
        public void Unregister_does_not_throw_when_no_provider_registered()
        {
            DiagnosticDataRegistry.Unregister();

            DiagnosticDataRegistry.IsRegistered.Should().BeFalse();
        }

        [Fact]
        public void Register_replaces_existing_provider()
        {
            var firstProvider = new Mock<IDiagnosticDataProvider>();
            var secondProvider = new Mock<IDiagnosticDataProvider>();

            DiagnosticDataRegistry.Register(firstProvider.Object);
            DiagnosticDataRegistry.Register(secondProvider.Object);

            DiagnosticDataRegistry.Provider.Should().BeSameAs(secondProvider.Object);
        }
    }
}
