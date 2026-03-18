// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations;
using Hemocode.Common.Implementations.Facades;
using Hemocode.Common.Interfaces.Adapters;
using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.Adapters
{
    [AdapterClass]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class CreatureDataAdapter : ICreatureData
    {
        private readonly CreatureModel _creature;

        public CreatureDataAdapter([NotNull] CreatureModel creature)
        {
            ThrowHelper.ThrowIfNull(creature, nameof(creature));
            _creature = creature;
        }

        public long metadataId => _creature.metadataId;
        public int qliphothCounter => _creature.qliphothCounter;
        public object script => _creature.script;

        public bool IsMaxObserved()
        {
            return _creature.observeInfo.IsMaxObserved();
        }

        public bool IsBeautyAndTheBeastWeakened(IBeautyBeastAnimTestAdapter beautyBeastAnimTestAdapter)
        {
            return _creature.IsBeautyAndTheBeastWeakened(beautyBeastAnimTestAdapter);
        }

        public int GetParasiteTreeNumberOfFlowers(IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter)
        {
            return _creature.GetParasiteTreeNumberOfFlowers(yggdrasilAnimTestAdapter);
        }
    }
}
