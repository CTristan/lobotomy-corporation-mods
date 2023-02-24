// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    /// <summary>
    ///     Only used if there is no creature to evaluate but we need to return something.
    /// </summary>
    internal sealed class NoneEvaluator : ICreatureEvaluator
    {
        public bool WillAgentDie()
        {
            return false;
        }
    }
}
