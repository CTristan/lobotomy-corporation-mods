// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.LoggerTargets
{
    public sealed class DebugLoggerTarget : ILoggerTarget
    {
        private readonly IAngelaConversationUiAdapter _angelaConversationUiAdapter;

        public DebugLoggerTarget(IAngelaConversationUiAdapter angelaConversationUiAdapter)
        {
            _angelaConversationUiAdapter = angelaConversationUiAdapter;
        }

        public void WriteToLoggerTarget(string message)
        {
            Notice.instance.Send(NoticeName.AddSystemLog, message);
            _angelaConversationUiAdapter.AddMessage(message);
        }
    }
}
