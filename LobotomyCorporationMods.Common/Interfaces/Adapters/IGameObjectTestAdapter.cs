// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IGameObjectTestAdapter : ITestAdapter<GameObject>
    {
        bool ActiveSelf { get; }
        ITransformAdapter Transform { get; }
        T AddComponent<T>() where T : Component;
        IComponentAdapter AddComponentAdapter<T>() where T : Component;
        T GetComponent<T>();
        bool IsUnityNull();
        void SetActive(bool value);
    }
}
