using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking
{
    internal static class AgentModelExtensions
    {
        public static bool HasBuffOfType<TBuff>([NotNull] this AgentModel agent) where TBuff : UnitBuf
        {
            var buffs = agent.GetUnitBufList();

            return buffs.OfType<TBuff>().Any();
        }

        public static bool HasCrumblingArmor([NotNull] this AgentModel agent)
        {
            return agent.HasEquipment(4000371) || agent.HasEquipment(4000372) || agent.HasEquipment(4000373) ||
                   agent.HasEquipment(4000374);
        }
    }
}
