// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
{
    internal class CreatureBaseTestAdapter<T> : TestAdapter<T> where T : CreatureBase
    {
        protected CreatureBaseTestAdapter([NotNull] T gameObject) : base(gameObject)
        {
        }
    }
}
