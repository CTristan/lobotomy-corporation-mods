// SPDX-License-Identifier: MIT

using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.FreeCustomization.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.FreeCustomizationTests.PatchTests
{
    public sealed class AgentDataPatchAppearCopy : FreeCustomizationModTests
    {
        [Theory]
        [InlineData("oldMouthPanic", "newMouthPanic")]
        public void Panic_mouth_is_correctly_copied(string oldPanicMouthName,
            string newPanicMouthName)
        {
            // Arrange
            var sutAppearance = new Appearance
            {
                Mouth_Panic = UnityTestExtensions.CreateSprite(oldPanicMouthName),
            };

            var originalHashCode = sutAppearance.Mouth_Panic.GetHashCode();
            var sut = UnityTestExtensions.CreateAgentData(appearance: sutAppearance);

            var newAgentAppearance = new Appearance
            {
                Mouth_Panic = UnityTestExtensions.CreateSprite(newPanicMouthName),
            };

            var newAgentData = UnityTestExtensions.CreateAgentData(appearance: newAgentAppearance);

            // Act
            sut.PatchAfterAppearCopy(newAgentData);

            // Assert
            // Have to use HashCodes because all the properties are ECall and cause SecurityExceptions.
            sut.appearance.Mouth_Panic.GetHashCode().Should().Be(newAgentAppearance.Mouth_Panic.GetHashCode()).And.NotBe(originalHashCode);
        }
    }
}
