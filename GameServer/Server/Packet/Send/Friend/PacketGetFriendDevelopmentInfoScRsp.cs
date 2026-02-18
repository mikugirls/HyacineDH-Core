using HyacineCore.Server.Database.Friend;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;
using HyacineCore.Server.Util;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Friend;

public class PacketGetFriendDevelopmentInfoScRsp : BasePacket
{
    public PacketGetFriendDevelopmentInfoScRsp(Retcode code) : base(CmdIds.GetFriendDevelopmentInfoScRsp)
    {
        var proto = new GetFriendDevelopmentInfoScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }

    public PacketGetFriendDevelopmentInfoScRsp(FriendRecordData data) : base(CmdIds.GetFriendDevelopmentInfoScRsp)
    {
        foreach (var friendDevelopmentInfoPb in data.DevelopmentInfos.ToArray())
            if (Extensions.GetUnixSec() - friendDevelopmentInfoPb.Time >=
                TimeSpan.TicksPerDay * 7 / TimeSpan.TicksPerSecond)
                data.DevelopmentInfos.Remove(friendDevelopmentInfoPb);

        var proto = new GetFriendDevelopmentInfoScRsp
        {
            KDPNPGFBGNB = { data.DevelopmentInfos.Select(x => x.ToProto()) },
            Uid = (uint)data.Uid
        };

        SetData(proto);
    }
}
