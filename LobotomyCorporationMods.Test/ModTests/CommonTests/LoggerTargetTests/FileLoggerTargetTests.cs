// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Implementations.LoggerTargets;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.CommonTests.LoggerTargetTests
{
    public sealed class FileLoggerTargetTests
    {
        [Fact]
        public void Logging_message_creates_file_and_writes_to_file()
        {
            const string ExpectedLogFilename = "log.txt";
            const string ExpectedMessage = "MessageSentToLog";
            Mock<IFileManager> mockFileManager = TestExtensions.GetMockFileManager();
            FileLoggerTarget sut = new(mockFileManager.Object, ExpectedLogFilename);

            sut.WriteToLoggerTarget(ExpectedMessage);

            mockFileManager.Verify(manager => manager.GetFile(ExpectedLogFilename), Times.Once);
            mockFileManager.Verify(manager => manager.WriteAllText(It.IsAny<string>(), ExpectedMessage), Times.Once);
        }
    }
}
