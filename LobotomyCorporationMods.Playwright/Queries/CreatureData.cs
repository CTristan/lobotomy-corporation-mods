// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Playwright.Queries
{
    public sealed class CreatureData
    {
        public long InstanceId { get; set; }
        public long MetadataId { get; set; }
        public string Name { get; set; }
        public string RiskLevel { get; set; }
        public string State { get; set; }
        public int QliphothCounter { get; set; }
        public int MaxQliphothCounter { get; set; }
        public string FeelingState { get; set; }
        public string CurrentSefira { get; set; }
        public int ObservationLevel { get; set; }
        public int WorkCount { get; set; }
        public bool IsEscaping { get; set; }
        public bool IsSuppressed { get; set; }
    }
}
