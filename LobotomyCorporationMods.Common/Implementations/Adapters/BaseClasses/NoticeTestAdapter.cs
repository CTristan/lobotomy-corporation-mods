// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace Hemocode.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class NoticeTestAdapter : TestAdapter<Notice>, INoticeTestAdapter
    {
        internal NoticeTestAdapter([NotNull] Notice gameObject) : base(gameObject)
        {
        }

        public void Send(string notice,
            params object[] param)
        {
            GameObjectInternal.Send(notice, param);
        }
    }
}
