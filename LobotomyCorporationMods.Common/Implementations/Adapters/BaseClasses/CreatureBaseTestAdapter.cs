// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal class CreatureBaseTestAdapter<T> : TestAdapter<T> where T : CreatureBase
    {
        protected CreatureBaseTestAdapter([NotNull] T gameObject) : base(gameObject)
        {
        }
    }
}
