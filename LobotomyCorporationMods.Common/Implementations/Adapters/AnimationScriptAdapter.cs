// SPDX-License-Identifier: MIT

#region

using System;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    public sealed class AnimationScriptAdapter : IAnimationScriptAdapter
    {
        private readonly CreatureAnimScript _animationScript;
        private readonly IGameObjectAdapter _gameObjectAdapter;

        public AnimationScriptAdapter(CreatureAnimScript animationScript) : this(animationScript, new GameObjectAdapter()) { }

        public AnimationScriptAdapter(CreatureAnimScript animationScript, IGameObjectAdapter gameObjectAdapter)
        {
            _animationScript = animationScript;
            _gameObjectAdapter = gameObjectAdapter;
        }

        public int BeautyAndTheBeastState
        {
            get
            {
                _animationScript.NotNull(nameof(_animationScript));

                if (_animationScript is BeautyBeastAnim animationScript)
                {
                    try
                    {
                        return animationScript.GetState();
                    }
                    catch (Exception exception) when (exception.IsUnityException())
                    {
                        return 0;
                    }
                }

                throw new InvalidOperationException("Could not cast animation script as BeautyBeastAnim");
            }
        }

        public int ParasiteTreeNumberOfFlowers
        {
            get
            {
                _animationScript.NotNull(nameof(_animationScript));

                if (_animationScript is YggdrasilAnim animationScript)
                {
                    var flowers = animationScript.flowers ?? new GameObject[0];

                    return flowers.Count(flower => _gameObjectAdapter.GameObjectIsActive(flower));
                }

                throw new InvalidOperationException("Could not cast animation script as YggdrasilAnim");
            }
        }

        [CanBeNull]
        public TScript UnpackScriptAsType<TScript>() where TScript : CreatureAnimScript
        {
            return _animationScript as TScript;
        }
    }
}
