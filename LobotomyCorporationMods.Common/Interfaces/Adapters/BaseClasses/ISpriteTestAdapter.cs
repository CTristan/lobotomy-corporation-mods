// SPDX-License-Identifier: MIT

using UnityEngine;

namespace Hemocode.Common.Interfaces.Adapters.BaseClasses
{
    public interface ISpriteTestAdapter : ITestAdapter<Sprite>
    {
        Sprite Create(Texture2D texture,
            Rect rect,
            Vector2 pivot);
    }
}
