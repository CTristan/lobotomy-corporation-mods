// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;
using UnityEngine.UI;

#endregion

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class TextTestAdapter : ComponentTestAdapter<Text>, ITextTestAdapter
    {
        internal TextTestAdapter([NotNull] Text gameObject) : base(gameObject)
        {
        }

        public string Text
        {
            get =>
                GameObjectInternal.text;
            set =>
                GameObjectInternal.text = value;
        }
    }
}
