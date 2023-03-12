// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IComponentAdapter : IAdapter<Component>
    {
        string Name { get; }
    }
}
