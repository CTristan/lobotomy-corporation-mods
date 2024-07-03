// SPDX-License-Identifier: MIT

#region

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class ArmorCreatureTestAdapter : Adapter<ArmorCreature>, IArmorCreatureTestAdapter
    {
        internal ArmorCreatureTestAdapter([NotNull] ArmorCreature armorCreature)
        {
            GameObject = armorCreature;
        }

        [CanBeNull]
        public IList SpecialAgentList
        {
            get
            {
                var fieldInfo = typeof(ArmorCreature).GetField("_specialAgentList", BindingFlags.NonPublic | BindingFlags.Instance);
                return fieldInfo?.GetValue(GameObject) as IList;
            }
        }

        public void OnViewInit()
        {
            GameObject.OnViewInit(GameObject.Unit);
        }
    }
}
