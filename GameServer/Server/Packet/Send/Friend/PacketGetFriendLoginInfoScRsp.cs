using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Friend;

public class PacketGetFriendLoginInfoScRsp : BasePacket
{
    public PacketGetFriendLoginInfoScRsp(List<int> friends) : base(CmdIds.GetFriendLoginInfoScRsp)
    {
        var proto = new GetFriendLoginInfoScRsp
        {
            PKDOFLGOAOF = { friends.Select(x => (uint)x) }
        };

        SetData(proto);
    }
}
