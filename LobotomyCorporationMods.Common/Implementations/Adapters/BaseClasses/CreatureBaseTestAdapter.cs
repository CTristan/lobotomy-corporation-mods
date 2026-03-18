// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;

namespace Hemocode.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class CreatureBaseTestAdapter<T> : TestAdapter<T> where T : CreatureBase
    {
        protected CreatureBaseTestAdapter([NotNull] T gameObject) : base(gameObject)
        {
        }
    }
}
