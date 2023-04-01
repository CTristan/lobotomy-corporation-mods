// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Implementations.LoggerTargets;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.Common.LoggerTargets
{
    public sealed class FileLoggerTargetTests
    {
        [Fact]
        public void Logging_message_creates_file_and_writes_to_file()
        {
            const string ExpectedLogFilename = "log.txt";
            const string ExpectedMessage = "MessageSentToLog";
            var mockFileManager = TestExtensions.GetMockFileManager();
            var sut = new FileLoggerTarget(mockFileManager.Object, ExpectedLogFilename);

            sut.WriteToLoggerTarget(ExpectedMessage);

            mockFileManager.Verify(static manager => manager.GetOrCreateFile(ExpectedLogFilename), Times.Once);
            mockFileManager.Verify(static manager => manager.WriteAllText(It.IsAny<string>(), ExpectedMessage), Times.Once);
        }
    }
}
