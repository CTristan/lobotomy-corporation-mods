using System.Collections.Generic;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    internal sealed class Gift
    {
        public Gift(string giftName)
        {
            Name = giftName;
            Agents = new List<Agent>();
        }

        public string Name { get; }
        public List<Agent> Agents { get; }
    }
}
