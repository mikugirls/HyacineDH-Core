using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Mission;

public class PacketUpdateTrackMainMissionIdScRsp : BasePacket
{
    public PacketUpdateTrackMainMissionIdScRsp(int prev, int cur) : base(CmdIds.UpdateTrackMainMissionScRsp)
    {
        var proto = new UpdateTrackMainMissionScRsp
        {
            PrevTrackMissionId = (uint)prev,
            TrackMissionId = (uint)cur
        };

        SetData(proto);
    }
}
