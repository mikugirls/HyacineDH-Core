using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Marble;

public class PacketMarbleLevelFinishScRsp : BasePacket
{
    public PacketMarbleLevelFinishScRsp(uint levelId) : base(CmdIds.None)
    {
        SetData([]);
    }
}
