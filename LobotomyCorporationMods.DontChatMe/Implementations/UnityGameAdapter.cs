// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Extensions;
using LobotomyCorporationMods.DontChatMe.Interfaces;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations
{
    /// <summary>
    ///     Production <see cref="IGameAdapter" /> that reaches into the Unity game's singletons.
    ///     Marked <see cref="ExcludeFromCodeCoverageAttribute" /> because every method is a thin
    ///     wrapper over a Unity-only API and is exercised at runtime, not by unit tests.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class UnityGameAdapter : IGameAdapter
    {
        public bool IsGameReady =>
            GameManager.currentGameManager != null
            && GameManager.currentGameManager.state == GameState.PLAYING;

        public int LivingAgentCount => GetLivingAgents().Count;

        public int ControllableAgentCount => GetControllableAgents().Count;

        public int CreatureCount
        {
            get
            {
                var list = CreatureManager.instance.GetCreatureList();
                return list?.Length ?? 0;
            }
        }

        public bool KillRandomLivingAgent()
        {
            var living = GetLivingAgents();
            if (living.Count == 0)
            {
                return false;
            }

            living.GetRandom().KillWithoutLosingEquipment();
            return true;
        }

        public bool ForcePanicOnRandomControllableAgent()
        {
            var controllable = GetControllableAgents();
            if (controllable.Count == 0)
            {
                return false;
            }

            controllable.GetRandom().ForcePanicAndDrainSanity();
            return true;
        }

        public bool ActivateRandomMeltdown()
        {
            var overloads = CreatureOverloadManager.instance.ActivateOverload(
                overloadCount: 1,
                type: OverloadType.DEFAULT,
                overloadTime: 60f,
                ignoreBossReward: true
            );

            if (overloads == null || overloads.Count == 0)
            {
                return false;
            }

            PlayMeltdownSound();
            return true;
        }

        public bool EscapeRandomCreature()
        {
            var creatures = CreatureManager.instance.GetCreatureList();
            if (creatures == null || creatures.Length == 0)
            {
                return false;
            }

            var candidates = new List<CreatureModel>(creatures.Length);
            foreach (var creature in creatures)
            {
                if (creature != null && !creature.IsEscaped())
                {
                    candidates.Add(creature);
                }
            }

            if (candidates.Count == 0)
            {
                return false;
            }

            candidates.GetRandom().Escape();
            return true;
        }

        public void AddEnergy(float amount) => EnergyModel.instance.AddEnergy(amount);

        public void SubtractEnergy(float amount) => EnergyModel.instance.SubEnergy(amount);

        public void AddMoney(int amount) => MoneyModel.instance.Add(amount);

        public void ShowSystemMessage(string message) =>
            AngelaConversation.instance.SendSystemLogMessage(cm: null, message);

        public void SetGameSpeed(float speed) =>
            GameManager.currentGameManager.SetPlaySpeedForcely(speed);

        public string ReadPhase()
        {
            var gm = GameManager.currentGameManager;
            if (gm == null)
            {
                return GamePhases.NoDay;
            }

            switch (gm.state)
            {
                case GameState.STOP:
                    return GamePhases.NoDay;
                case GameState.PAUSE:
                    return GamePhases.Paused;
            }

            // PLAYING — narrow further by whichever in-game special states are active.
            var ordeals = OrdealManager.instance.GetActivatedOrdeals();
            if (ordeals != null && ordeals.Count > 0)
            {
                return GamePhases.OrdealActive;
            }

            if (HasOverloadedCreature())
            {
                return GamePhases.MeltdownActive;
            }

            return GamePhases.Ready;
        }

        private static bool HasOverloadedCreature()
        {
            var creatures = CreatureManager.instance.GetCreatureList();
            if (creatures == null)
            {
                return false;
            }

            foreach (var creature in creatures)
            {
                if (creature != null && creature.isOverloaded)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<AgentModel> GetLivingAgents()
        {
            return AgentManager
                .instance.GetAgentList()
                .Where(a => a != null && !a.IsDead())
                .ToList();
        }

        private static List<AgentModel> GetControllableAgents()
        {
            return AgentManager
                .instance.GetAgentList()
                .Where(a => a != null && a.IsControllable())
                .ToList();
        }

        private static void PlayMeltdownSound()
        {
            var energyController = GameStatusUI.GameStatusUI.Window?.energyContorller;
            if (energyController == null)
            {
                return;
            }

            GlobalAudioManager.instance.PlayLocalClip(energyController.OverloadClip);
        }
    }
}
