using HyacineCore.Server.GameServer.Server.Packet.Send.Scene;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.ActiveFarmElementCsReq)]
public class HandlerActivateFarmElementCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ActiveFarmElementCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketActivateFarmElementScRsp(req.EntityId, connection.Player!));
    }
}
