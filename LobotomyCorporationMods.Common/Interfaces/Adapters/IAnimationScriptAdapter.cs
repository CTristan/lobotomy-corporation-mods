// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAnimationScriptAdapter
    {
        int BeautyAndTheBeastState { get; }
        int ParasiteTreeNumberOfFlowers { get; }
        TScript UnpackScriptAsType<TScript>() where TScript : CreatureAnimScript;
    }
}
