// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.CrowdControl.Constants;

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    internal delegate CrowdControlResponse CrowdControlAction(CrowdControlClient client,
        CrowdControlRequest req);

    internal static class CrowdControlDelegates
    {
        [NotNull]
        internal static CrowdControlResponse AddEnergy(CrowdControlClient client,
            [NotNull] CrowdControlRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            const CrowdControlResponseStatus Status = CrowdControlResponseStatus.Success;

            try
            {
                EnergyModel.instance.AddEnergy(Settings.EnergyChange);
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogException(e);
                throw;
            }

            return new CrowdControlResponse(request.Id, Status);
        }

        [NotNull]
        internal static CrowdControlResponse RandomMeltdown(CrowdControlClient client,
            [NotNull] CrowdControlRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            var status = CrowdControlResponseStatus.Success;
            var message = string.Empty;

            try
            {
                var overloadedRooms = CreatureOverloadManager.instance.ActivateOverload(1, OverloadType.DEFAULT, 60f, false, true);
                if (overloadedRooms.Count == 0)
                {
                    status = CrowdControlResponseStatus.Failure;
                    message = "No abnormalities are available for meltdown.";
                }
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogException(e);
                throw;
            }

            return new CrowdControlResponse(request.Id, status, message);
        }

        [NotNull]
        internal static CrowdControlResponse RemoveEnergy(CrowdControlClient client,
            [NotNull] CrowdControlRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            const CrowdControlResponseStatus Status = CrowdControlResponseStatus.Success;

            try
            {
                EnergyModel.instance.AddEnergy(Settings.EnergyChange * -1);
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogException(e);
                throw;
            }

            return new CrowdControlResponse(request.Id, Status);
        }
    }
}
