using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Gacha;

public class PacketSetGachaDecideItemScRsp : BasePacket
{
    public PacketSetGachaDecideItemScRsp(uint gachaId, List<uint> order) : base(CmdIds.SetGachaDecideItemScRsp)
    {
        var proto = new SetGachaDecideItemScRsp
        {
            BNOGOLNMOOA = new JMDPCOPDNNH
            {
                HHKJCOLOKFF = gachaId,
                CCLAKJOGOGB = { order },
                EJHKEFAIEBG = 1,
                MPOFFMIELAF = { 11 }
            }
        };

        SetData(proto);
    }
}
