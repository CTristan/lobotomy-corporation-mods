// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.Common
{
    public sealed class LoggerTests
    {
        [Fact]
        public void Logging_exception_writes_to_log()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockAngelaConversationUiAdapter = new Mock<IAngelaConversationUiAdapter>();
            var logger = new Logger(mockFileManager.Object, mockAngelaConversationUiAdapter.Object);

            logger.WriteToLog(new InvalidOperationException());

            mockFileManager.Verify(static manager => manager.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
