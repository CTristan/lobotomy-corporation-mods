// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class AgentFacade
    {
        public static long GetAgentId([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAgent().instanceId;
        }
    }
}
