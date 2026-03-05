// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright.Queries
{
    public class AgentData
    {
        public long InstanceId { get; set; }
        public string Name { get; set; }
        public float Hp { get; set; }
        public float MaxHp { get; set; }
        public float Mental { get; set; }
        public float MaxMental { get; set; }
        public int Fortitude { get; set; }
        public int Prudence { get; set; }
        public int Temperance { get; set; }
        public int Justice { get; set; }
        public string CurrentSefira { get; set; }
        public string State { get; set; }
        public string[] GiftIds { get; set; }
        public string WeaponId { get; set; }
        public string ArmorId { get; set; }
        public bool IsDead { get; set; }
        public bool IsPanicking { get; set; }
    }
}
