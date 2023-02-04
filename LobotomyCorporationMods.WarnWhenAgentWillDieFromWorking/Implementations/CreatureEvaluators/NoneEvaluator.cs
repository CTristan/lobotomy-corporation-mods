// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    /// <summary>
    ///     Only used if there is no creature to evaluate but we need to return something.
    /// </summary>
    public class NoneEvaluator : ICreatureEvaluator
    {
        public bool WillAgentDie()
        {
            return false;
        }
    }
}