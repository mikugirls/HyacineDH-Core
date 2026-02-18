using HyacineCore.Server.GameServer.Server.Packet.Send.Marble;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Marble;

[Opcode(CmdIds.MarbleLevelFinishCsReq)]
public class HandlerMarbleLevelFinishCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = CJIBLEHFKHH.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketMarbleLevelFinishScRsp(req.PKAADCNKGHF));
    }
}
