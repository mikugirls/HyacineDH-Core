using HyacineCore.Server.Kcp;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.GetCurAssistCsReq)]
public class HandlerGetCurAssistCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(CmdIds.GetCurAssistScRsp);
    }
}