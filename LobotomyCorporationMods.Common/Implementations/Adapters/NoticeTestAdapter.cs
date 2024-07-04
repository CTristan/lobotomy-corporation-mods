// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class NoticeTestAdapter : Adapter<Notice>, INoticeTestAdapter
    {
        internal NoticeTestAdapter([NotNull] Notice notice)
        {
            GameObject = notice;
        }

        public void Send(string notice,
            params object[] param)
        {
            GameObject.Send(notice, param);
        }
    }
}
