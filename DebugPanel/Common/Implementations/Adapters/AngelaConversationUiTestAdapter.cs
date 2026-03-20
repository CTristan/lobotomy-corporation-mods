// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Common.Implementations.Adapters.BaseClasses;
using DebugPanel.Common.Interfaces.Adapters;

#endregion

namespace DebugPanel.Common.Implementations.Adapters
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
