// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Extensions
{
    /// <summary>Tight wrappers over <see cref="AgentModel" /> for use by the Unity game adapter.</summary>
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal static class AgentModelExtensions
    {
        // The damage values are picked to be larger than any agent's max HP/SP so the operation
        // reduces the stat to zero regardless of upgrades.
        private const float OneShotDamage = 0xFFF;

        /// <summary>
        ///     An agent is controllable when it is alive, not panicking, not invincible, not currently
        ///     mid-skill, and not under the Death-Angel-Betrayer buff.
        /// </summary>
        internal static bool IsControllable(this AgentModel agent)
        {
            ThrowHelper.ThrowIfNull(agent, nameof(agent));

            return !agent.IsDead()
                && !agent.IsCrazy()
                && !agent.invincible
                && agent.currentSkill == null
                && !agent.HasUnitBuf(UnitBufType.DEATH_ANGEL_BETRAYER);
        }

        /// <summary>Removes equipment, then deals enough physical damage to kill the agent outright.</summary>
        internal static void KillWithoutLosingEquipment(this AgentModel agent)
        {
            ThrowHelper.ThrowIfNull(agent, nameof(agent));

            agent.ReleaseWeaponV2();
            agent.ReleaseArmor();

            ConsoleCommand.instance.AgentCommandOperation(
                (int)AgentConsoleCommandIds.TakePhysicalDamage,
                new object[] { agent.instanceId, OneShotDamage }
            );
        }

        /// <summary>Forces the agent to panic and drains their sanity to zero.</summary>
        internal static void ForcePanicAndDrainSanity(this AgentModel agent)
        {
            ThrowHelper.ThrowIfNull(agent, nameof(agent));

            agent.Panic();
            ConsoleCommand.instance.AgentCommandOperation(
                (int)AgentConsoleCommandIds.TakeMentalDamage,
                new object[] { agent.instanceId, OneShotDamage }
            );
        }
    }
}
