// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.LoggerTargets
{
    public sealed class AngelaLoggerTarget : ILoggerTarget
    {
        private readonly bool _logToAngela;
        private IAngelaConversationUiTestAdapter _angelaConversationUiTestAdapter;

        /// <summary>
        ///     Represents a logger target that writes log messages to Angela. If we don't want to log messages to Angela then
        ///     just set the adapter to null.
        /// </summary>
        public AngelaLoggerTarget(bool logToAngela,
            [CanBeNull] IAngelaConversationUiTestAdapter angelaConversationUiTestAdapter = null)
        {
            _logToAngela = logToAngela;
            _angelaConversationUiTestAdapter = angelaConversationUiTestAdapter;
        }

        public void WriteToLoggerTarget(string message)
        {
            if (!_logToAngela || AngelaConversationUI.instance is null)
            {
                return;
            }

            _angelaConversationUiTestAdapter = _angelaConversationUiTestAdapter.EnsureNotNullWithMethod(() =>
                new AngelaConversationUiTestAdapter(AngelaConversationUI.instance));

            Notice.instance.Send(NoticeName.AddSystemLog, message);
            _angelaConversationUiTestAdapter.AddMessage(message);
        }
    }
}
