using CrowdControl.Common;
using JetBrains.Annotations;
using ConnectorType = CrowdControl.Common.ConnectorType;

namespace CrowdControl.Games.Packs.LobotomyCorporation;

[UsedImplicitly]
public class LobotomyCorporation : SimpleTCPPack
{
    protected override string ProcessName => "LobotomyCorporation";

    public override string Host => "127.0.0.1";

    public override ushort Port => 3330;

    public LobotomyCorporation(UserRecord player, Func<CrowdControlBlock, bool> responseHandler, Action<object> statusUpdateHandler) : base(player, responseHandler, statusUpdateHandler) { }

    public override Game Game { get; } = new("LobotomyCorporation", "LobotomyCorporation", "PC", ConnectorType.SimpleTCPServerConnector);

    public override EffectList Effects { get; } = new Effect[]
    {
        new("Add Energy", "AddEnergy"),
        new("Remove Energy", "RemoveEnergy"),
        new("Meltdown One Abnormality", "RandomMeltdown"),
        new("Flip Facility", "flip") { Duration = 15 },
    };
}
