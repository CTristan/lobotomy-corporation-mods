// // SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Enums
{
    internal enum AgentConsoleCommandIds
    {
        /// <summary>Deals red damage to the agent. Usage: [long Agent ID] [float amount]</summary>
        TakePhysicalDamage = 0,

        /// <summary>Deals white damage to the agent. Usage: [long Agent ID] [float amount]</summary>
        TakeMentalDamage = 1,

        /// <summary>Sets the agent's HP and SP to a specific value. Usage: [long Agent ID] [float value]</summary>
        SuperSoldier = 2,

        /// <summary>
        ///     Causes fear damage to an agent, which can cause an employee to lose SP in relation to their employee level.
        ///     Usage: [long Agent ID] [int fear level (0-4)
        /// </summary>
        /// <remarks>
        ///     Does nothing to agents that are level 5, mostly interesting if used on lower-level agents.
        ///     https://lobotomycorp.fandom.com/wiki/Employees#Fear_Level
        /// </remarks>
        Encounter = 3,

        /// <summary>Equips the specified gift to the agent. Usage: [long Agent ID] [long Gift ID]</summary>
        /// <remarks>If there is a gift already equipped in the same slot, it will overwrite it.</remarks>
        GiftAdd = 4,

        /// <summary>Removes the specified gift from the agent. Usage: [long Agent ID] [long Gift ID]</summary>
        GiftRemove = 5,

        /// <summary>Deals the specified type of damage to the agent. Usage: [damage type (R/W/B/P)] [long Agent ID] [float amount]</summary>
        DamageForcely = 6
    }
}
