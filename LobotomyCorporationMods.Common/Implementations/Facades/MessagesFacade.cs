// SPDX-License-Identifier: MIT

using Hemocode.Common.Extensions;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;

namespace Hemocode.Common.Implementations.Facades
{
    public static class MessagesFacade
    {
        public static void SendMessage(this UnitModel unitModel,
            string message,
            INoticeTestAdapter noticeTestAdapter = null)
        {
            noticeTestAdapter = noticeTestAdapter.EnsureNotNullWithMethod(() => new NoticeTestAdapter(Notice.instance));

            noticeTestAdapter.Send(NoticeName.AddSystemLog, message);
        }
    }
}
