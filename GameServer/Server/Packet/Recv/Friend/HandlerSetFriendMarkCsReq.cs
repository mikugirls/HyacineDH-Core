using HyacineCore.Server.GameServer.Server.Packet.Send.Friend;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.SetFriendMarkCsReq)]
public class HandlerSetFriendMarkCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetFriendMarkCsReq.Parser.ParseFrom(data);

        connection.Player!.FriendManager!.MarkFriend((int)req.Uid, req.OLCOHPHGDKK);

        await connection.SendPacket(new PacketSetFriendMarkScRsp(req.Uid, req.OLCOHPHGDKK));
    }
}
