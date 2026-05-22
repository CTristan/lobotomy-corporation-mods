// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Interfaces
{
    /// <summary>
    ///     Narrow adapter over the Unity-singleton game APIs each effect touches.
    ///     The production implementation lives in <c>UnityGameAdapter</c> and is excluded from
    ///     coverage; tests pass a fake to exercise effect logic in isolation.
    /// </summary>
    public interface IGameAdapter
    {
        /// <summary>True when <c>GameManager.currentGameManager</c> exists and is in the PLAYING state.</summary>
        bool IsGameReady { get; }

        int LivingAgentCount { get; }
        int ControllableAgentCount { get; }
        int CreatureCount { get; }

        /// <summary>Kills a random living agent without dropping their equipment.</summary>
        /// <returns><c>true</c> on success; <c>false</c> if no living agents.</returns>
        bool KillRandomLivingAgent();

        /// <summary>Forces a random controllable agent into panic and drains their SP.</summary>
        /// <returns><c>true</c> on success; <c>false</c> if no controllable agents.</returns>
        bool ForcePanicOnRandomControllableAgent();

        /// <summary>Triggers an overload (meltdown) on one random abnormality.</summary>
        /// <returns><c>true</c> when at least one overload was activated.</returns>
        bool ActivateRandomMeltdown();

        /// <summary>Releases a random creature from containment.</summary>
        /// <returns><c>true</c> on success; <c>false</c> if no creatures to release.</returns>
        bool EscapeRandomCreature();

        void AddEnergy(float amount);
        void SubtractEnergy(float amount);
        void AddMoney(int amount);
        void ShowSystemMessage(string message);
        void SetGameSpeed(float speed);

        /// <summary>
        ///     Returns the current game phase as one of the
        ///     <see cref="Constants.GamePhases" /> string constants. Used by
        ///     <c>GamePhaseProbe</c> to push <c>game_state</c> frames to the chat-side server.
        /// </summary>
        string ReadPhase();
    }
}
