// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Implementations.LoggerTargets;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.Mods.Common.LoggerTargets
{
    public sealed class DebugLoggerTests
    {
        [Fact]
        public void Logging_message_sends_to_Angela()
        {
            const string ExpectedMessage = "MessageSentToLog";
            var mockAngelaConversationUiAdapter = new Mock<IAngelaConversationUiAdapter>();
            var sut = new DebugLoggerTarget(mockAngelaConversationUiAdapter.Object);

            sut.WriteToLoggerTarget(ExpectedMessage);

            mockAngelaConversationUiAdapter.Verify(adapter => adapter.AddMessage(ExpectedMessage), Times.Once);
        }
    }
}
