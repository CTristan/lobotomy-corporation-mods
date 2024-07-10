// SPDX-License-Identifier: MIT

using Customizing;
using WorkerSprite;

namespace LobotomyCorporationMods.FreeCustomization.Objects
{
    /// <summary>The original ResourceLib doesn't contain the Mouth_Panic sprite, so we need to replace everything that calls it to instead use our new class.</summary>
    public sealed class FixedResourceLib : Appearance.ResourceLib
    {
        public WorkerBasicSprite.ResourceData Mouth_Panic { get; set; }
    }
}
