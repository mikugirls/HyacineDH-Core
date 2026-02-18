using HyacineCore.Server.Data;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Marble;

public class PacketMarbleGetDataScRsp : BasePacket
{
    public PacketMarbleGetDataScRsp() : base(CmdIds.MarbleGetDataScRsp)
    {
        var proto = new MarbleGetDataScRsp
        {
            FCPBIBDODPH = { GameData.MarbleSealData.Keys.Select(x => (uint)x) },
            BBMCCLMHJGM = { GameData.MarbleMatchInfoData.Keys.Select(x => (uint)x) }
        };

        SetData(proto);
    }
}
