using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Marble;

public class PacketMarbleUpdateShownSealScRsp : BasePacket
{
    public PacketMarbleUpdateShownSealScRsp(ICollection<uint> sealList) : base(CmdIds.MarbleUpdateShownSealScRsp)
    {
        var proto = new MarbleUpdateShownSealScRsp
        {
            PDOOKOFDOAI = { sealList }
        };

        SetData(proto);
    }
}
