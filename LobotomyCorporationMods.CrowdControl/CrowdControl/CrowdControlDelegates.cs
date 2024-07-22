// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

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
                EnergyModel.instance.AddEnergy(10f);
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
