// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
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

        /// <summary>
        ///     Unity overrides the equality operators to return a lifetime check rather than an actual null check. Because of
        ///     this, a simple null check won't work for determining if an object is actually active, as an object can be not null
        ///     but still be considered null by Unity if it is not active.
        ///     To determine if a non-null object is active and populated, we have to perform a boolean check of the object.
        ///     For more information:
        ///     https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
        /// </summary>
        public bool IsUnityNull()
        {
            return !GameObject;
        }

        public void SetActive(bool value)
        {
            GameObject.SetActive(value);
        }

        public ITransformAdapter Transform => new TransformAdapter { GameObject = GameObject.transform };
    }
}
