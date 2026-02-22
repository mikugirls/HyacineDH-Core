using HyacineCore.Server.Database.Player;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;
using HyacineCore.Server.Util;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Friend;

public class PacketSyncApplyFriendScNotify : BasePacket
{
    public PacketSyncApplyFriendScNotify(PlayerData player) : base(CmdIds.SyncApplyFriendScNotify)
    {
        var proto = new SyncApplyFriendScNotify
        {
            FriendApplyInfo = new FriendApplyInfo
            {
                ApplyTime = Extensions.GetUnixSec(),
                PlayerInfo = player.ToSimpleProto(FriendOnlineStatus.Online)
            }
        };

        SetData(proto);
    }
}
