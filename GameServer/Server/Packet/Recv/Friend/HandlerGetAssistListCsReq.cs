using HyacineCore.Server.GameServer.Server.Packet.Send.Friend;
using HyacineCore.Server.Kcp;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.GetAssistListCsReq)]
public class HandlerGetAssistListCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetAssistListScRsp());
    }
}