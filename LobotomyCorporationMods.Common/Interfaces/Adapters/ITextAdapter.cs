// SPDX-License-Identifier: MIT

#region

using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITextAdapter : IAdapter<Text>
    {
        string Text { get; set; }
    }
}
