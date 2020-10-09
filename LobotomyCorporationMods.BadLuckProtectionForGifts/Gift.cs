using System.Collections.Generic;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    internal sealed class Gift
    {
        internal Gift(string giftName)
        {
            Agents = new List<Agent>();
            Name = giftName;
        }

        internal List<Agent> Agents { get; }
        internal string Name { get; }
    }
}
