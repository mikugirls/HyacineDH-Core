using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.ChallengePeak;

public class PacketConfirmChallengePeakSettleScRsp : BasePacket
{
    public PacketConfirmChallengePeakSettleScRsp(uint peakId, bool unk, Retcode retcode = Retcode.RetSucc)
        : base(CmdIds.ConfirmChallengePeakSettleScRsp)
    {
        var proto = new ConfirmChallengePeakSettleScRsp
        {
            Retcode = (uint)retcode,
            PeakId = peakId,
            JBJKIALGDOI = unk
        };

        SetData(proto);
    }
}

