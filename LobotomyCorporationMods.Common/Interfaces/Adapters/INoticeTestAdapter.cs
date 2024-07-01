// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface INoticeTestAdapter : ITestAdapter<Notice>
    {
        void Send(string notice,
            params object[] param);
    }
}
