// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITransformAdapter : IAdapter<Transform>, IComponentAdapter
    {
        new Transform GameObject { get; set; }
        new IGameObjectAdapter GameObjectAdapter { get; }
        Vector3 LocalPosition { get; set; }
        Vector3 LocalScale { get; set; }
        Transform Parent { get; set; }
        ITransformAdapter ParentAdapter { get; }
        Transform GetChild(int index);
        void SetParent(Transform parent);
    }
}
