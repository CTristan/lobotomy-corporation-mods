// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using Hemocode.Common.Interfaces.Adapters;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;

namespace Hemocode.Common.ParameterObjects
{
    /// <summary>Represents a set of optional test adapter parameters.</summary>
    public sealed class OptionalTestAdapterParameters
    {
        [CanBeNull] public IGameObjectTestAdapter GameObjectTestAdapter { get; set; }

        [CanBeNull] public IImageTestAdapter ImageTestAdapter { get; set; }

        [CanBeNull] public IManagementSlotTestAdapter ManagementSlotTestAdapter { get; set; }

        [CanBeNull] public ISpriteTestAdapter SpriteTestAdapter { get; set; }

        [CanBeNull] public ITexture2dTestAdapter Texture2DTestAdapter { get; set; }
    }
}
