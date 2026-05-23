// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>
    ///     Canonical <c>effect_slug</c> values that v1 ships with.
    ///     Chat-side projects must match these exactly when registering effects.
    /// </summary>
    public static class EffectSlugs
    {
        // Ported from the CC repo prior art
        public const string RandomMeltdown = "random_meltdown";
        public const string KillRandomAgent = "kill_random_agent";
        public const string RandomAgentPanic = "random_agent_panic";
        public const string AddEnergy = "add_energy";
        public const string RemoveEnergy = "remove_energy";

        // New for v1
        public const string AddMoney = "add_money";
        public const string ShowSystemMessage = "show_system_message";
        public const string SetGameSpeed = "set_game_speed";
        public const string EscapeRandomCreature = "escape_random_creature";
    }
}
