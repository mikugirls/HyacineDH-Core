using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Mission;

public class PacketMissionAcceptScNotify : BasePacket
{
    public PacketMissionAcceptScNotify(int missionId) : this([missionId])
    {
    }

    public PacketMissionAcceptScNotify(List<int> missionIds) : base(CmdIds.None)
    {
        _ = missionIds.Count;
        SetData([]);
    }
}
