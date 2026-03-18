// SPDX-License-Identifier: MIT

using Hemocode.Common.Implementations.LoggerTargets;
using Hemocode.Common.Interfaces.Adapters;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.CommonTests.LoggerTargetTests
{
    public sealed class AngelaLoggerTests
    {
        [Fact]
        public void Logging_message_sends_to_Angela()
        {
            const string ExpectedMessage = "MessageSentToLog";
            Mock<IAngelaConversationUiTestAdapter> mockAngelaConversationUiTestAdapter = new();
            AngelaLoggerTarget sut = new(true, mockAngelaConversationUiTestAdapter.Object);

            sut.WriteToLoggerTarget(ExpectedMessage);

            mockAngelaConversationUiTestAdapter.Verify(adapter => adapter.AddMessage(ExpectedMessage), Times.Once);
        }

        [Fact]
        public void Disabling_Angela_logging_does_not_send_messages_to_Angela()
        {
            const string ExpectedMessage = "MessageSentToLog";
            Mock<IAngelaConversationUiTestAdapter> mockAngelaConversationUiTestAdapter = new();
            AngelaLoggerTarget sut = new(false, mockAngelaConversationUiTestAdapter.Object);

            sut.WriteToLoggerTarget(ExpectedMessage);

            mockAngelaConversationUiTestAdapter.Verify(adapter => adapter.AddMessage(ExpectedMessage), Times.Never);
        }
    }
}
