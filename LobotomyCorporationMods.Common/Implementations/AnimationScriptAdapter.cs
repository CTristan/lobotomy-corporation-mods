// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class AnimationScriptAdapter : IAnimationScriptAdapter
    {
        private CreatureAnimScript _animationScript;

        [CanBeNull]
        public TScript GetScript<TScript>([NotNull] CreatureModel creature) where TScript : CreatureAnimScript
        {
            Guard.Against.Null(creature, nameof(creature));

            _animationScript = creature.GetAnimScript() as TScript;

            return _animationScript as TScript;
        }

        public int BeautyAndTheBeastState
        {
            get
            {
                Guard.Against.Null(_animationScript, nameof(_animationScript));

                if (!(_animationScript is BeautyBeastAnim animationScript))
                {
                    throw new InvalidOperationException("Could not cast animation script as BeautyBeastAnim");
                }

                return animationScript.GetState();
            }
        }
    }
}
