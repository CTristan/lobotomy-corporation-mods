// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses
{
    public interface ITextureTestAdapter<T> : ITestAdapter<T> where T : Texture
    {
        int Width { get; }
        int Height { get; }
    }
}
