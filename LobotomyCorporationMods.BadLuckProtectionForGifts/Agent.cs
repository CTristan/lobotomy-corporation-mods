namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    internal sealed class Agent
    {
        public Agent(long id)
        {
            Id = id;
            WorkCount = 0f;
        }

        public long Id { get; }
        public float WorkCount { get; set; }
    }
}
