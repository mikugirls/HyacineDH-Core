using HyacineCore.Server.GameServer.Server.Packet.Send.ChallengePeak;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.ChallengePeak;

[Opcode(CmdIds.ConfirmChallengePeakSettleCsReq)]
public class HandlerConfirmChallengePeakSettleCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ConfirmChallengePeakSettleCsReq.Parser.ParseFrom(data);
        await connection.SendPacket(new PacketConfirmChallengePeakSettleScRsp(req.PeakId, req.JBJKIALGDOI));
    }
}

