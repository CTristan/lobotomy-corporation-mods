// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace LobotomyCorporationMods.Playwright.JsonModels
{
    /// <summary>
    /// Data class for set-agent-stats command parameters.
    /// </summary>
    [Serializable]
    public sealed class SetAgentStatsParams
    {
        public long agentId;
        public float hp;
        public float mental;
        public int fortitude;
        public int prudence;
        public int temperance;
        public int justice;
    }

    /// <summary>
    /// Data class for add-gift/remove-gift command parameters.
    /// </summary>
    [Serializable]
    public sealed class GiftParams
    {
        public long agentId;
        public int giftId;
    }

    /// <summary>
    /// Data class for set-qliphoth command parameters.
    /// </summary>
    [Serializable]
    public sealed class SetQliphothParams
    {
        public long creatureId;
        public int counter;
    }

    /// <summary>
    /// Data class for set-game-speed command parameters.
    /// </summary>
    [Serializable]
    public sealed class SetGameSpeedParams
    {
        public int speed;
    }

    /// <summary>
    /// Data class for spawn-creature command parameters.
    /// </summary>
    [Serializable]
    public sealed class SpawnCreatureParams
    {
        public int metadataId;
        public string sefira;
    }

    /// <summary>
    /// Data class for trigger-ordeal command parameters.
    /// </summary>
    [Serializable]
    public sealed class TriggerOrdealParams
    {
        public string ordealType;
    }

    /// <summary>
    /// Data class for set-agent-invincible command parameters.
    /// </summary>
    [Serializable]
    public sealed class SetAgentInvincibleParams
    {
        public long agentId;
        public bool invincible;
    }

    /// <summary>
    /// Data class for assign-work command parameters.
    /// </summary>
    [Serializable]
    public sealed class AssignWorkParams
    {
        public long agentId;
        public long creatureId;
        public string workType; // instinct, insight, attachment, repression, etc.
    }

    /// <summary>
    /// Data class for deploy-agent command parameters.
    /// </summary>
    [Serializable]
    public sealed class DeployAgentParams
    {
        public long agentId;
        public string sefira;
    }

    /// <summary>
    /// Data class for recall-agent command parameters.
    /// </summary>
    [Serializable]
    public sealed class RecallAgentParams
    {
        public long agentId;
    }

    /// <summary>
    /// Data class for suppress command parameters.
    /// </summary>
    [Serializable]
    public sealed class SuppressParams
    {
        public long creatureId;
    }
}
