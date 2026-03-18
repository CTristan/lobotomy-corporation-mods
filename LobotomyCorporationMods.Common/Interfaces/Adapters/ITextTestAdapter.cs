// SPDX-License-Identifier: MIT

#region

using Hemocode.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine.UI;

#endregion

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface ITextTestAdapter : IComponentTestAdapter<Text>
    {
        string Text { get; set; }
    }
}
