// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;

#endregion

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class AngelaConversationUiTestAdapter : ComponentTestAdapter<AngelaConversationUI>, IAngelaConversationUiTestAdapter
    {
        internal AngelaConversationUiTestAdapter([NotNull] AngelaConversationUI gameObject) : base(gameObject)
        {
        }

        public void AddMessage(string message)
        {
            GameObjectInternal.AddAngelaMessage(message);
        }
    }
}
