// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAnimationScriptAdapter
    {
        int BeautyAndTheBeastState { get; }
        int ParasiteTreeNumberOfFlowers { get; }
        TScript GetScript<TScript>(CreatureModel creature) where TScript : CreatureAnimScript;
    }
}
