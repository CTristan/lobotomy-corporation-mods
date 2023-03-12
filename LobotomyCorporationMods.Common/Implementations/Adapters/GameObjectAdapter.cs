// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class GameObjectAdapter : Adapter<GameObject>, IGameObjectAdapter
    {
        public bool ActiveSelf => GameObject.activeSelf;

        public T AddComponent<T>() where T : Component
        {
            return GameObject.AddComponent<T>();
        }

        public IComponentAdapter AddComponentAdapter<T>() where T : Component
        {
            return new ComponentAdapter { GameObject = GameObject.AddComponent<T>() };
        }

        public T GetComponent<T>()
        {
            return GameObject.GetComponent<T>();
        }

        public bool IsUnityNull()
        {
            return GameObject.IsUnityNull();
        }

        public void SetActive(bool value)
        {
            GameObject.SetActive(value);
        }

        public ITransformAdapter Transform => new TransformAdapter { GameObject = GameObject.transform };
    }
}
