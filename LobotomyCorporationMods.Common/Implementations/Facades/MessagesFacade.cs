// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class MessagesFacade
    {
        public static void SendMessage(
            this UnitModel unitModel,
            string message,
            INoticeTestAdapter noticeTestAdapter = null
        )
        {
            noticeTestAdapter = noticeTestAdapter.EnsureNotNullWithMethod(
                () => new NoticeTestAdapter(Notice.instance)
            );

            noticeTestAdapter.Send(NoticeName.AddSystemLog, message);
        }
    }
}
