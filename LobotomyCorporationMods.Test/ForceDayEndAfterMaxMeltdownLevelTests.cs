using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public sealed class ForceDayEndAfterMaxMeltdownLevelTests
    {
        private const int MaxLevel = 10;

        public ForceDayEndAfterMaxMeltdownLevelTests()
        {
            var fileManager = TestExtensions.GetFileManager();
            _ = new Harmony_Patch(fileManager);
        }

        /// <summary>
        ///     Needed to verify that constructor is public and externally accessible.
        /// </summary>
        [Fact]
        public void Constructor_IsUntestable()
        {
            // Act
            Action act = () => _ = new Harmony_Patch();

            // Assert
            act.ShouldThrow<TypeInitializationException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void CheckForMaxMeltdown_MeltdownLevelLessThanMax_ReturnsFalse(int qliphothCounter)
        {
            // Arrange
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(qliphothCounter);

            // Act
            var result = Harmony_Patch.CheckForMaxMeltdown(creatureOverloadManager);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CheckForMaxMeltdown_MaxMeltdownLevelButNotMaxQliphothCounter_ReturnsFalse()
        {
            // Arrange
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(MaxLevel);

            // Act
            var result = Harmony_Patch.CheckForMaxMeltdown(creatureOverloadManager);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CheckForMaxMeltdown_MaxMeltdownLevelAndMaxQliphothCounter_ReturnsTrue()
        {
            // Arrange
            const int OverloadMax = 0;
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(MaxLevel, OverloadMax);

            // Act
            var result = Harmony_Patch.CheckForMaxMeltdown(creatureOverloadManager);

            // Assert
            result.Should().BeTrue();
        }
    }
}
