// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>
    ///     Allowed values of the <c>status</c> field on an outbound <c>effect_response</c>
    ///     frame. The server's <c>Hemograce.Dispatch.GameQueue</c> uses these to decide how
    ///     to advance (or hold) the per-game queue head.
    /// </summary>
    public static class ResponseStatuses
    {
        /// <summary>Effect ran. Server marks the row succeeded and advances the queue.</summary>
        public const string Success = "success";

        /// <summary>Effect could not run and should NOT be retried. Server refunds and advances.</summary>
        public const string Failure = "failure";

        /// <summary>Transient block; server should hold and resend after <c>retry_after_ms</c>.</summary>
        public const string Retry = "retry";

        /// <summary>
        ///     The mod's idempotency cache says this redemption already terminated in a prior
        ///     session. Server treats this as <see cref="Success" /> for queue-advance purposes.
        /// </summary>
        public const string Duplicate = "duplicate";
    }
}
