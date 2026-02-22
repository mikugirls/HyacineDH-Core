using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Friend;

public class PacketSetFriendMarkScRsp : BasePacket
{
    public PacketSetFriendMarkScRsp(uint uid, bool isMark) : base(CmdIds.SetFriendMarkScRsp)
    {
        var proto = new SetFriendMarkScRsp
        {
            Uid = uid,
            OLCOHPHGDKK = isMark
        };

        SetData(proto);
    }
}
