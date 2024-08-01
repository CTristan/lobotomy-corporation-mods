// SPDX-License-Identifier: MIT

using System;
using FluentAssertions;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;
using Moq;

namespace LobotomyCorporationMods.Test.ModTests
{
    public abstract class HarmonyPatchTestBase
    {
        protected Mock<ILogger> MockLogger { get; } = new Mock<ILogger>();

        protected static void ValidatePatch([NotNull] Type patchType,
            Type originalClass,
            string methodName)
        {
            var attribute = Attribute.GetCustomAttribute(patchType, typeof(HarmonyPatch)) as HarmonyPatch;

            attribute.Should().NotBeNull();
            attribute?.info.originalType.Should().Be(originalClass);
            attribute?.info.methodName.Should().Be(methodName);
        }

        protected void VerifyArgumentNullExceptionLogging(Action action,
            Times? numberOfTimes = null)
        {
            action.Should().Throw<ArgumentNullException>();
            MockLogger.Verify(logger => logger.LogError(It.IsAny<ArgumentNullException>()), numberOfTimes ?? Times.Once());
        }

        protected void VerifyNullReferenceExceptionLogging(Action action,
            Times? numberOfTimes = null)
        {
            action.Should().Throw<NullReferenceException>();
            MockLogger.Verify(logger => logger.LogError(It.IsAny<NullReferenceException>()), numberOfTimes ?? Times.Once());
        }
    }
}
