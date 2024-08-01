// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.Common.ParameterContainers
{
    /// <summary>Represents a set of optional test adapter parameters.</summary>
    public sealed class OptionalTestAdapterParametersContainer
    {
        [CanBeNull]
        public IGameObjectTestAdapter GameObjectTestAdapter { get; set; }

        [CanBeNull]
        public IImageTestAdapter ImageTestAdapter { get; set; }

        [CanBeNull]
        public IManagementSlotTestAdapter ManagementSlotTestAdapter { get; set; }

        [CanBeNull]
        public ISpriteTestAdapter SpriteTestAdapter { get; set; }

        [CanBeNull]
        public ITexture2dTestAdapter Texture2DTestAdapter { get; set; }
    }
}
