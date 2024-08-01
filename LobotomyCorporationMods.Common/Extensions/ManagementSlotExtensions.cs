// SPDX-License-Identifier: MIT

#region

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.ParameterContainers;

#endregion

// ReSharper disable UnusedParameter.Global

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class ManagementSlotExtensions
    {
        [NotNull]
        internal static IGameObjectTestAdapter CreateImageObjectTestAdapter([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParametersContainer imageParametersContainer,
            [CanBeNull] OptionalTestAdapterParametersContainer testAdapterParametersContainer = null)
        {
            testAdapterParametersContainer = testAdapterParametersContainer.EnsureNotNullWithMethod(() => new OptionalTestAdapterParametersContainer());
            testAdapterParametersContainer.ManagementSlotTestAdapter =
                testAdapterParametersContainer.ManagementSlotTestAdapter.EnsureNotNullWithMethod(() => new ManagementSlotTestAdapter(managementSlot));

            return testAdapterParametersContainer.ManagementSlotTestAdapter.CreateImageObjectTestAdapter(imageParametersContainer, testAdapterParametersContainer);
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
