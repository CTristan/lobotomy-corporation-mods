// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class TransformAdapter : ComponentAdapter, ITransformAdapter
    {
        private Transform? _transform;

        public new Transform GameObject
        {
            get
            {
                if (_transform is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _transform;
            }
            set => _transform = value;
        }

        public Transform GetChild(int index)
        {
            return GameObject.GetChild(index);
        }

        public Vector3 LocalPosition
        {
            get => GameObject.localPosition;
            set => GameObject.localPosition = value;
        }

        public Vector3 LocalScale
        {
            get => GameObject.localScale;
            set => GameObject.localScale = value;
        }

        public Transform Parent
        {
            get => GameObject.parent;
            set => GameObject.parent = value;
        }

        public ITransformAdapter ParentAdapter
        {
            get
            {
                var transformParent = Parent;

                return new TransformAdapter { GameObject = transformParent };
            }
        }

        public void SetParent(Transform parent)
        {
            GameObject.SetParent(parent);
        }
    }
}
