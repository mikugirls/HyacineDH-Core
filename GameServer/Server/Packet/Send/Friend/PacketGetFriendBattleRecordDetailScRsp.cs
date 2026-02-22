using HyacineCore.Server.Data;
using HyacineCore.Server.Database.Avatar;
using HyacineCore.Server.Database.Challenge;
using HyacineCore.Server.Database.Friend;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Friend;

public class PacketGetFriendBattleRecordDetailScRsp : BasePacket
{
    public PacketGetFriendBattleRecordDetailScRsp(FriendRecordData recordData, ChallengeData challengeData,
        AvatarData avatarData) : base(
        CmdIds.GetFriendBattleRecordDetailScRsp)
    {
        var proto = new GetFriendBattleRecordDetailScRsp
        {
            Uid = (uint)recordData.Uid
        };

        SetData(proto);
    }

    public PacketGetFriendBattleRecordDetailScRsp(Retcode code) : base(CmdIds.GetFriendBattleRecordDetailScRsp)
    {
        var proto = new GetFriendBattleRecordDetailScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }
}
