// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class AgentModelExtensions
    {
        public static void KillWithoutLosingEquipment([NotNull] this AgentModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));

            // First remove all equipment
            agent.ReleaseWeaponV2();
            agent.ReleaseArmor();

            // Then kill
            var agentId = agent.instanceId;
            const float Damage = 0xFFF;
            var param = new object[] { agentId, Damage };
            ConsoleCommand.instance.AgentCommandOperation((int)AgentConsoleCommandIds.TakePhysicalDamage, param);
        }

        /// <summary>
        ///     Causes an agent to panic and drains their SP.
        /// </summary>
        /// <param name="agent"></param>
        public static void MakePanic([NotNull] this AgentModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));

            // First, make the agent panic to prevent any race conditions when we drain their SP
            agent.Panic();

            // Then take away all of their SP so it takes more than one white damage to heal them
            var agentId = agent.instanceId;
            const float Damage = 0xFFF;
            var param = new object[] { agentId, Damage };

            ConsoleCommand.instance.AgentCommandOperation((int)AgentConsoleCommandIds.TakeMentalDamage, param);
        }

        public static bool IsControllable([NotNull] this AgentModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));

            return !agent.IsDead() && !agent.IsCrazy() && !agent.invincible && agent.currentSkill == null &&
                   !agent.invincible && !agent.HasUnitBuf(UnitBufType.DEATH_ANGEL_BETRAYER);
        }
    }
}
