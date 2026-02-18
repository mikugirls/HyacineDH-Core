using HyacineCore.Server.GameServer.Server.Packet.Send.Gacha;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Gacha;

[Opcode(CmdIds.SetGachaDecideItemCsReq)]
public class HandlerSetGachaDecideItemCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetGachaDecideItemCsReq.Parser.ParseFrom(data);

        connection.Player!.GachaManager!.GachaData.GachaDecideOrder = req.LODCIPDAADC.Select(x => (int)x).ToList();

        await connection.SendPacket(new PacketSetGachaDecideItemScRsp(req.GachaId, req.LODCIPDAADC.ToList()));
    }
}
