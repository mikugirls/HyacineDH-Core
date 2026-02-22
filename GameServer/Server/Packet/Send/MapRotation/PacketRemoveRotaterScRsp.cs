using HyacineCore.Server.GameServer.Game.Player;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.MapRotation;

public class PacketRemoveRotaterScRsp : BasePacket
{
    public PacketRemoveRotaterScRsp(PlayerInstance player, RemoveRotatorCsReq req) : base(CmdIds.RemoveRotaterScRsp)
    {
        var proto = new RemoveRotaterScRsp
        {
            EnergyInfo = new RotaterEnergyInfo
            {
                CurNum = (uint)player.ChargerNum,
                MaxNum = 5
            },
            RotaterData = req.RotaterData
        };

        SetData(proto);
    }
}
