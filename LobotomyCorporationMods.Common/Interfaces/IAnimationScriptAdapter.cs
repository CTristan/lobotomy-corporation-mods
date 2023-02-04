// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IAnimationScriptAdapter
    {
        int BeautyAndTheBeastState { get; }
        TScript GetScript<TScript>(CreatureModel creature) where TScript : CreatureAnimScript;
    }
}
