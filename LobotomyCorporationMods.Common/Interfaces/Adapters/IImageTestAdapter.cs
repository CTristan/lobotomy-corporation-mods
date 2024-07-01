// SPDX-License-Identifier: MIT

#region

using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IImageTestAdapter : ITestAdapter<Image>
    {
        Color Color { get; set; }
    }
}
