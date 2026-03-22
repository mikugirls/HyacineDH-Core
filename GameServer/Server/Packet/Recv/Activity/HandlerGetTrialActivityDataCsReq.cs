using HyacineCore.Server.GameServer.Server.Packet.Send.Activity;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Activity;

[Opcode(CmdIds.GetTrialActivityDataCsReq)]
public class HandlerGetTrialActivityDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetTrialActivityDataCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketGetTrialActivityDataScRsp(connection.Player!));
    }
}