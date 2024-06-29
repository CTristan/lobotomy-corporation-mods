// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class NoticeAdapter : Adapter<Notice>, INoticeAdapter
    {
        public void Send(string notice,
            params object[] param)
        {
            GameObject.Send(notice, param);
        }
    }
}
