// SPDX-License-Identifier: MIT

#region

using System;
using Hemocode.Common.Implementations;
using Hemocode.Common.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CommonTests
{
    public sealed class LoggerTests
    {
        [Fact]
        public void Logging_exception_writes_to_log()
        {
            Mock<ILoggerTarget> mockLoggerTarget = new();
            Logger logger = new(mockLoggerTarget.Object);

            logger.WriteException(new InvalidOperationException());

            mockLoggerTarget.Verify(target => target.WriteToLoggerTarget(It.IsAny<string>()), Times.Once);
        }
    }
}
