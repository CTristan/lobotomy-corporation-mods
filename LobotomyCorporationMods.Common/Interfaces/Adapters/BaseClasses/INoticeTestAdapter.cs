// SPDX-License-Identifier: MIT

namespace Hemocode.Common.Interfaces.Adapters.BaseClasses
{
    public interface INoticeTestAdapter : ITestAdapter<Notice>
    {
        void Send(string notice,
            params object[] param);
    }
}
