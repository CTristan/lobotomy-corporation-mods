// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class NoticeTestAdapter : TestAdapter<Notice>, INoticeTestAdapter
    {
        internal NoticeTestAdapter([NotNull] Notice gameObject)
            : base(gameObject) { }

        public void Send(string notice, params object[] param)
        {
            _gameObject.Send(notice, param);
        }
    }
}
