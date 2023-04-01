// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
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
            var mockLoggerTarget = new Mock<ILoggerTarget>();
            var logger = new Logger(mockLoggerTarget.Object);

            logger.WriteException(new InvalidOperationException());

            mockLoggerTarget.Verify(static target => target.WriteToLoggerTarget(It.IsAny<string>()), Times.Once);
        }
    }
}
