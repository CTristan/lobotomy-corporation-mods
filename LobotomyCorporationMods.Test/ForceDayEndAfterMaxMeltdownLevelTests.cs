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

            // Static instances that need to be created before some of the tests will work
            _ = TestExtensions.CreateGlobalGameManager();
            _ = TestExtensions.CreateEnergyModel(999f);
        }

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
        public void AddOverloadGaguePrefix_MeltdownLevelLessThanMax_ReturnsTrue(int qliphothCounter)
        {
            // Arrange
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(qliphothCounter);

            // Act
            var result = Harmony_Patch.AddOverloadGaguePrefix(creatureOverloadManager);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void AddOverloadGaguePrefix_MaxMeltdownLevelButNotMaxQliphothCounter_ReturnsTrue()
        {
            // Arrange
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(MaxLevel);

            // Act
            var result = Harmony_Patch.AddOverloadGaguePrefix(creatureOverloadManager);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void AddOverloadGaguePrefix_MaxMeltdownLevelAndMaxQliphothCounterButInEmergency_StageEndedIsFalse()
        {
            // Arrange
            const int OverloadMax = 0;
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(MaxLevel, OverloadMax);
            var gameManager = TestExtensions.CreateGameManager(true);

            // Act
            var result = Harmony_Patch.AddOverloadGaguePrefix(creatureOverloadManager);

            // Assert
            result.Should().BeFalse();
            gameManager.StageEnded.Should().BeFalse();
        }

        /// <summary>
        ///     This is untestable because the actual method to end the day calls another game method that Unity won't allow.
        /// </summary>
        [Fact]
        public void AddOverloadGaguePrefix_MaxMeltdownLevelAndMaxQliphothCounterAndNotInEmergency_InUntestable()
        {
            // Arrange
            const int OverloadMax = 0;
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(MaxLevel, OverloadMax);
            var gameManager = TestExtensions.CreateGameManager();

            // Act
            var result = Record.Exception(() => Harmony_Patch.AddOverloadGaguePrefix(creatureOverloadManager));

            // Assert
            TestExtensions.AssertIsUnityException(result).Should().BeTrue();
        }
    }
}
