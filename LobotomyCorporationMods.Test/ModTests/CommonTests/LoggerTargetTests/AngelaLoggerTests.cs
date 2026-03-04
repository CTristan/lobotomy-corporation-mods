// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Implementations.LoggerTargets;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
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
            var mockAngelaConversationUiTestAdapter = new Mock<IAngelaConversationUiTestAdapter>();
            var sut = new AngelaLoggerTarget(true, mockAngelaConversationUiTestAdapter.Object);

            sut.WriteToLoggerTarget(ExpectedMessage);

            mockAngelaConversationUiTestAdapter.Verify(adapter => adapter.AddMessage(ExpectedMessage), Times.Once);
        }

        [Fact]
        public void Disabling_Angela_logging_does_not_send_messages_to_Angela()
        {
            const string ExpectedMessage = "MessageSentToLog";
            var mockAngelaConversationUiTestAdapter = new Mock<IAngelaConversationUiTestAdapter>();
            var sut = new AngelaLoggerTarget(false, mockAngelaConversationUiTestAdapter.Object);

            sut.WriteToLoggerTarget(ExpectedMessage);

            mockAngelaConversationUiTestAdapter.Verify(adapter => adapter.AddMessage(ExpectedMessage), Times.Never);
        }
    }
}
