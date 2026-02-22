using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Player;

public class PacketPlayerKickOutScNotify : BasePacket
{
    public PacketPlayerKickOutScNotify() : base(CmdIds.FightKickOutScNotify)
    {
        var proto = new FightKickOutScNotify();
        SetData(proto);
    }
}
