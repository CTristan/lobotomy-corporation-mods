// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class GameObjectTestAdapter : Adapter<GameObject>, IGameObjectTestAdapter
    {
        internal GameObjectTestAdapter([NotNull] GameObject gameObject) : base(gameObject)
        {
        }

        public bool ActiveSelf => GameObject.activeSelf;

        public void SetActive(bool value)
        {
            GameObject.SetActive(value);
        }
    }
}
