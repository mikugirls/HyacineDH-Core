using HyacineCore.Server.GameServer.Game.Player;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Scene;

public class PacketActivateFarmElementScRsp : BasePacket
{
    public PacketActivateFarmElementScRsp(uint entityId, PlayerInstance player) : base(CmdIds.ActiveFarmElementScRsp)
    {
        var proto = new ActiveFarmElementScRsp
        {
            EntityId = entityId,
            WorldLevel = (uint)player.Data.WorldLevel
        };

        SetData(proto);
    }
}
