// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.CommonTests
{
    public sealed class LoggerTests
    {
        [Fact]
        public void Logging_exception_throws_Unity_exception_due_to_Debug_mode()
        {
            // Needs an existing AngelaConversationUI instance because we're also logging to debug
            _ = TestExtensions.CreateAngelaConversationUI();
            var mockFileManager = new Mock<IFileManager>();
            var logger = new Logger(mockFileManager.Object);

            Action action = () => logger.WriteToLog(new Exception());

            action.ShouldThrowUnityException();
        }
    }
}
