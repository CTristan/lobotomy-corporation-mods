// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Implementations.LoggerTargets;
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
            const string ExpectedLogFilename = "errors.log";
            const string ExpectedMessage = "MessageSentToLog";
            var mockFileManager = TestExtensions.GetMockFileManager();
            var sut = new FileLoggerTarget(mockFileManager.Object, ExpectedLogFilename);

            sut.WriteToLoggerTarget(ExpectedMessage);

            mockFileManager.Verify(manager => manager.GetFile(ExpectedLogFilename), Times.Once);
            mockFileManager.Verify(manager => manager.WriteAllText(It.IsAny<string>(), ExpectedMessage, It.IsAny<bool>()), Times.Once);
        }
    }
}
