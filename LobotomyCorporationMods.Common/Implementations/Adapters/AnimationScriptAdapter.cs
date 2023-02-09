// SPDX-License-Identifier: MIT

using System;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    public sealed class AnimationScriptAdapter : IAnimationScriptAdapter
    {
        private CreatureAnimScript _animationScript;

        public int BeautyAndTheBeastState
        {
            get
            {
                Guard.Against.Null(_animationScript, nameof(_animationScript));

                if (_animationScript is BeautyBeastAnim animationScript)
                {
                    return animationScript.GetState();
                }

                throw new InvalidOperationException("Could not cast animation script as BeautyBeastAnim");
            }
        }

        public int ParasiteTreeNumberOfFlowers
        {
            get
            {
                Guard.Against.Null(_animationScript, nameof(_animationScript));

                if (_animationScript is YggdrasilAnim animationScript)
                {
                    return animationScript.flowers.Count(flower => flower.activeSelf);
                }

                throw new InvalidOperationException("Could not cast animation script as YggdrasilAnim");
            }
        }

        [CanBeNull]
        public TScript GetScript<TScript>([NotNull] CreatureModel creature) where TScript : CreatureAnimScript
        {
            Guard.Against.Null(creature, nameof(creature));

            _animationScript = creature.GetAnimScript() as TScript;

            return _animationScript as TScript;
        }
    }
}
