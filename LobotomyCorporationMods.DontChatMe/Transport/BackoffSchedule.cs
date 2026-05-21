// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Transport
{
    /// <summary>
    ///     Pure-function reconnect-backoff schedule. Doubles the delay each attempt, capped at
    ///     <see cref="MaxDelaySeconds" />, with up to 25% positive jitter so multiple clients don't
    ///     thunder back together.
    /// </summary>
    public static class BackoffSchedule
    {
        public const float InitialDelaySeconds = 1f;
        public const float MaxDelaySeconds = 30f;
        public const float JitterFraction = 0.25f;

        /// <summary>
        ///     Returns the delay (in seconds) for <paramref name="attempt" /> (1-based).
        ///     <paramref name="randomFraction" /> is in [0,1); the caller supplies it so this remains
        ///     deterministic in tests.
        /// </summary>
        public static float DelayFor(int attempt, double randomFraction)
        {
            if (attempt < 1)
            {
                attempt = 1;
            }

            var clampedAttempt = attempt > 10 ? 10 : attempt;
            var doublings = clampedAttempt - 1;
            var raw = InitialDelaySeconds * (float)Math.Pow(2.0, doublings);
            var capped = raw > MaxDelaySeconds ? MaxDelaySeconds : raw;
            var jitter = capped * JitterFraction * (float)randomFraction;
            return capped + jitter;
        }
    }
}
