// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    public enum CrowdControlResponseStatus
    {
        //== Effect Instance Messages
        /// <summary>The effect executed successfully.</summary>
        Success = 0x00,

        /// <summary>The effect failed to trigger, but is still available for use. Viewer(s) will be refunded. You probably don't want this.</summary>
        Failure = 0x01,

        /// <summary>Same as <see cref="Failure" /> but the effect is no longer available for use for the remainder of the game. You probably don't want this.</summary>
        Unavailable = 0x02,

        /// <summary>The effect can’t be triggered right now, try again in a few seconds. This is the "normal" failure response.</summary>
        Retry = 0x03,

        /// <summary>INTERNAL USE ONLY. The effect has been queued for execution after the current one ends.</summary>
        Queue = 0x04,

        /// <summary>INTERNAL USE ONLY. The effect triggered successfully and is now active until it ends.</summary>
        Running = 0x05,

        /// <summary>The timed effect has been paused and is now waiting.</summary>
        Paused = 0x06,

        /// <summary>The timed effect has been resumed and is counting down again.</summary>
        Resumed = 0x07,

        /// <summary>The timed effect has finished.</summary>
        Finished = 0x08,

        //== Effect Class Messages
        /// <summary>The effect should be shown in the menu.</summary>
        Visible = 0x80,

        /// <summary>The effect should be hidden in the menu.</summary>
        NotVisible = 0x81,

        /// <summary>The effect should be selectable in the menu.</summary>
        Selectable = 0x82,

        /// <summary>The effect should be unselectable in the menu.</summary>
        NotSelectable = 0x83,

        //== System Status Messages
        /// <summary>The processor isn't ready to start or has shut down.</summary>
        NotReady = 0xFF,
    }
}
