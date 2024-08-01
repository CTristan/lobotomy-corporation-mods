// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses
{
    public interface ITextureTestAdapter<T> : ITestAdapter<T> where T : Texture
    {
        int Width { get; }
        int Height { get; }
    }
}
