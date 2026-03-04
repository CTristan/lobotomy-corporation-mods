// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.ParameterObjects;

// ReSharper disable UnusedParameter.Global

namespace LobotomyCorporationMods.Common.Extensions
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    internal static class ManagementSlotExtensions
    {
        [NotNull]
        internal static IGameObjectTestAdapter CreateImageObjectTestAdapter([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters = null)
        {
            testAdapterParameters = testAdapterParameters.EnsureNotNullWithMethod(() => new OptionalTestAdapterParameters());
            testAdapterParameters.ManagementSlotTestAdapter = testAdapterParameters.ManagementSlotTestAdapter.EnsureNotNullWithMethod(() => new ManagementSlotTestAdapter(managementSlot));

            return testAdapterParameters.ManagementSlotTestAdapter.CreateImageObjectTestAdapter(imageParameters, testAdapterParameters);
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
