// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.LoggerTargets
{
    public sealed class DebugLoggerTarget : ILoggerTarget
    {
        private readonly IAngelaConversationUiTestAdapter _angelaConversationUiTestAdapter;

        public DebugLoggerTarget(IAngelaConversationUiTestAdapter angelaConversationUiTestAdapter)
        {
            _angelaConversationUiTestAdapter = angelaConversationUiTestAdapter;
        }

        public void WriteToLoggerTarget(string message)
        {
            Notice.instance.Send(NoticeName.AddSystemLog, message);
            _angelaConversationUiTestAdapter.AddMessage(message);
        }
    }
}
