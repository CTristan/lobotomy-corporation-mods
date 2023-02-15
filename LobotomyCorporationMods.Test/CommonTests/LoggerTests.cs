// SPDX-License-Identifier: MIT

#region

using System;
using FluentAssertions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.CommonTests
{
    public sealed class LoggerTests
    {
        [Fact]
        public void Logging_exception_writes_to_log()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockAngelaConversationUiAdapter = new Mock<IAngelaConversationUiAdapter>();
            var logger = new Logger(mockFileManager.Object, mockAngelaConversationUiAdapter.Object);

            logger.WriteToLog(new Exception());

            mockFileManager.Verify(static manager => manager.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Debug_logging_throws_Unity_exception()
        {
            // Needs existing game instances
            _ = TestExtensions.CreateAngelaConversationUI();
            _ = TestExtensions.CreateGlobalGameManager();
            _ = TestExtensions.CreateSefiraBossManager(SefiraEnum.DUMMY);
            var mockFileManager = new Mock<IFileManager>();
            var logger = new Logger(mockFileManager.Object) { DebugLoggingEnabled = true };

            Action action = () => logger.WriteToLog(new Exception());

            action.ShouldNotThrow();
        }
    }
}
