// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface INoticeAdapter : IAdapter<Notice>
    {
        void Send(string notice, params object[] param);
    }
}
