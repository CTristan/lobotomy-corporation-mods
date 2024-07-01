// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

// ReSharper disable MemberCanBePrivate.Global
namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class AgentFacade
    {
        public static AgentModel GetAgent([NotNull] this UseSkill useSkill)
        {
            Guard.Against.Null(useSkill, nameof(useSkill));

            return useSkill.agent;
        }

        public static long GetAgentId([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAgent().instanceId;
        }
    }
}
