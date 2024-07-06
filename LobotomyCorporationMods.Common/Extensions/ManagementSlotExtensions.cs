// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

// ReSharper disable UnusedParameter.Global

namespace LobotomyCorporationMods.Common.Extensions
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    internal static class ManagementSlotExtensions
    {
        [NotNull]
        internal static IGameObjectTestAdapter CreateImageObjectTestAdapter([NotNull] this ManagementSlot managementSlot,
            float localScaleX,
            float localScaleY,
            float localPositionX,
            float localPositionY,
            float localPositionZ,
            [NotNull] ITexture2dTestAdapter texture2dTestAdapter,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            testAdapter = testAdapter.EnsureNotNullWithMethod(() => new ManagementSlotTestAdapter(managementSlot));

            return testAdapter.CreateImageObjectTestAdapter(localScaleX, localScaleY, localPositionX, localPositionY, localPositionZ, texture2dTestAdapter, imageGameObjectTestAdapter,
                spriteTestAdapter);
        }

        [CanBeNull]
        internal static CreatureEquipmentMakeInfo GetAbnormalityGift(this ManagementSlot managementSlot)
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;

            return commandWindow.GetAbnormalityGift();
        }

        [CanBeNull]
        internal static EquipmentTypeInfo GetAbnormalityGiftInfo(this ManagementSlot managementSlot)
        {
            return managementSlot.GetAbnormalityGift()?.equipTypeInfo;
        }
    }
}
