using System;
using System.Security;
using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.FreeCustomization;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class FreeCustomizationTests
    {
        public FreeCustomizationTests()
        {
            var fileManager = TestExtensions.GetFileManager();
            _ = new Harmony_Patch(fileManager);
        }

        [Fact]
        public void Constructor_IsUntestable()
        {
            // Act
            Action act = () => _ = new Harmony_Patch();

            // Assert
            act.ShouldThrow<TypeInitializationException>();
        }

        [Fact]
        public void CloseWindowPrefix_CloseActionIsNull_ReturnsFalse()
        {
            // Arrange
            var appearanceUi = Substitute.For<AppearanceUI>();

            // Act
            var result = Harmony_Patch.CloseWindowPrefix(appearanceUi);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        ///     GenerateWindowPostfix is untestable because it calls a method in another window, and the original method is static
        ///     which means that we are not able to get an instance to work with.
        /// </summary>
        [Fact]
        public void GenerateWindowPostfix_IsUntestable()
        {
            // Arrange
            var agentInfoWindow = Substitute.For<AgentInfoWindow>();
            var customizingWindow = Substitute.For<CustomizingWindow>();
            agentInfoWindow.customizingWindow = customizingWindow;
            AgentInfoWindow.currentWindow = agentInfoWindow;

            // Act
            Action act = () => Harmony_Patch.GenerateWindowPostfix();

            // Assert
            act.ShouldThrow<SecurityException>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OpenAppearanceWindowPostfix_IsCustomAppearance_ReturnsFalse(bool isCustomAppearance)
        {
            // Arrange
            var customizingWindow = Substitute.For<CustomizingWindow>();
            customizingWindow.CurrentData = new AgentData { isCustomAppearance = true };

            // Act
            Harmony_Patch.OpenAppearanceWindowPostfix(customizingWindow);

            // Assert
            customizingWindow.CurrentData.isCustomAppearance.Should().BeFalse();
        }
    }
}
