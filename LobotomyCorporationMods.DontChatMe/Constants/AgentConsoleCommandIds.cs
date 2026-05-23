// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>
    ///     The subset of <see cref="ConsoleCommand" /> agent operation IDs the mod issues.
    ///     Game-side indexes; only entries the mod actually uses are listed.
    /// </summary>
    internal enum AgentConsoleCommandIds
    {
        /// <summary>Deals red (HP) damage to the agent. Usage: [long agentId, float amount]</summary>
        TakePhysicalDamage = 0,

        /// <summary>Deals white (SP) damage to the agent. Usage: [long agentId, float amount]</summary>
        TakeMentalDamage = 1,
    }
}
