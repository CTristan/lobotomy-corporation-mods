// SPDX-License-Identifier: MIT

#region

using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    /// <summary>Only used if there is no creature to evaluate but we need to return something.</summary>
    public sealed class NoneEvaluator : ICreatureEvaluator
    {
        public bool WillAgentDie()
        {
            return false;
        }
    }
}
