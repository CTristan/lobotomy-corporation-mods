// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Interfaces;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes
{
    /// <summary>
    ///     Hand-rolled fake for <see cref="IGameAdapter" />. Tracks every method call by name so tests
    ///     can assert on side-effect ordering without standing up Unity singletons.
    /// </summary>
    public sealed class FakeGameAdapter : IGameAdapter
    {
        private readonly List<string> _calls = new List<string>();

        public Collection<string> Calls => new Collection<string>(_calls);

        public bool IsGameReady { get; set; } = true;
        public int LivingAgentCount { get; set; }
        public int ControllableAgentCount { get; set; }
        public int CreatureCount { get; set; }

        public bool KillRandomLivingAgentResult { get; set; } = true;
        public bool ForcePanicResult { get; set; } = true;
        public bool ActivateRandomMeltdownResult { get; set; } = true;
        public bool EscapeRandomCreatureResult { get; set; } = true;

        public float LastEnergyAdded { get; private set; }
        public float LastEnergySubtracted { get; private set; }
        public int LastMoneyAdded { get; private set; }
        public string LastSystemMessage { get; private set; }
        public float LastGameSpeed { get; private set; } = float.NaN;

        public string Phase { get; set; } = GamePhases.Ready;

        public bool KillRandomLivingAgent()
        {
            _calls.Add(nameof(KillRandomLivingAgent));
            return KillRandomLivingAgentResult;
        }

        public bool ForcePanicOnRandomControllableAgent()
        {
            _calls.Add(nameof(ForcePanicOnRandomControllableAgent));
            return ForcePanicResult;
        }

        public bool ActivateRandomMeltdown()
        {
            _calls.Add(nameof(ActivateRandomMeltdown));
            return ActivateRandomMeltdownResult;
        }

        public bool EscapeRandomCreature()
        {
            _calls.Add(nameof(EscapeRandomCreature));
            return EscapeRandomCreatureResult;
        }

        public void AddEnergy(float amount)
        {
            _calls.Add(nameof(AddEnergy));
            LastEnergyAdded = amount;
        }

        public void SubtractEnergy(float amount)
        {
            _calls.Add(nameof(SubtractEnergy));
            LastEnergySubtracted = amount;
        }

        public void AddMoney(int amount)
        {
            _calls.Add(nameof(AddMoney));
            LastMoneyAdded = amount;
        }

        public void ShowSystemMessage(string message)
        {
            _calls.Add(nameof(ShowSystemMessage));
            LastSystemMessage = message;
        }

        public void SetGameSpeed(float speed)
        {
            _calls.Add(nameof(SetGameSpeed));
            LastGameSpeed = speed;
        }

        public string ReadPhase()
        {
            _calls.Add(nameof(ReadPhase));
            return Phase;
        }
    }
}
