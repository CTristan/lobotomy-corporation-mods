// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    public delegate CrowdControlResponse CrowdControlAction(CrowdControlClient client,
        CrowdControlRequest req);

    public static class CrowdControlDelegates
    {
        [NotNull]
        public static CrowdControlResponse Example(CrowdControlClient client,
            [NotNull] CrowdControlRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            // Example of a non-timed effect
            var status = CrowdControlResponseStatus.STATUS_SUCCESS;
            var message = "";

            try
            {
                // Do something here
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogException(e);
                throw;
            }

            return new CrowdControlResponse(request.Id, status, message);
        }
    }
}
