// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    public enum CrowdControlResponseStatus
    {
        NONE = 0,
        STATUS_SUCCESS = 1,
        STATUS_FAILURE = 2,
        STATUS_UNAVAIL = 3,
        STATUS_RETRY = 4,
        STATUS_START = 5,
        STATUS_PAUSE = 6,
        STATUS_RESUME = 7,
        STATUS_STOP = 8,

        STATUS_VISIBLE = 0x80,
        STATUS_NOTVISIBLE = 0x81,
        STATUS_SELECTABLE = 0x82,
        STATUS_NOTSELECTABLE = 0x83,

        STATUS_KEEPALIVE = 255,
    }
}
