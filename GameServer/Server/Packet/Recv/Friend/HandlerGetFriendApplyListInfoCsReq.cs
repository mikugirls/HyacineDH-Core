using HyacineCore.Server.GameServer.Server.Packet.Send.Friend;
using HyacineCore.Server.Kcp;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.GetFriendApplyListInfoCsReq)]
public class HandlerGetFriendApplyListInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetFriendApplyListInfoCsReq(connection));
    }
}