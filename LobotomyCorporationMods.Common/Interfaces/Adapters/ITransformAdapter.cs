// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITransformAdapter : IAdapter<Transform>
    {
        Vector3 LocalPosition { get; set; }
        Vector3 LocalScale { get; set; }
        Transform Parent { get; set; }
        ITransformAdapter ParentAdapter { get; }
        Transform GetChild(int index);
        void SetParent(Transform parent);
    }
}
