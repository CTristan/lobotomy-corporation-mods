// SPDX-License-Identifier: MIT

#region

using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IImageAdapter : IAdapter<Image>
    {
        Color Color { get; set; }
    }
}
