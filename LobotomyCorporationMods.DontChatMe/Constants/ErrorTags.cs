// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>
    ///     Short string tags returned on <c>effect_failed</c> replies.
    ///     Kept short and stable so chat-side projects can map them to user-facing messages.
    /// </summary>
    public static class ErrorTags
    {
        public const string UnknownSlug = "unknown_slug";
        public const string Cooldown = "cooldown";
        public const string GameNotReady = "game_not_ready";
        public const string NoAgents = "no_agents";
        public const string NoCreatures = "no_creatures";
        public const string DangerEffectsDisabled = "danger_effects_disabled";
        public const string ExecutionError = "execution_error";
        public const string Overloaded = "overloaded";
        public const string DuplicateRedemption = "duplicate_redemption";
        public const string ModDisabled = "mod_disabled";
    }
}
