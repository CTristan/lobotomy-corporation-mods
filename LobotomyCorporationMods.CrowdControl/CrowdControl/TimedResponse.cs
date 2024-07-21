// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    public class TimedResponse : CrowdControlResponse
    {
        public TimedResponse(int id,
            int duration,
            CrowdControlResponseStatus status = CrowdControlResponseStatus.STATUS_SUCCESS,
            string message = "") : base(id, status, message)
        {
            TimeRemaining = duration;
        }

        private int TimeRemaining { get; set; }
    }
}
