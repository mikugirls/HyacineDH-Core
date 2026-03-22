using HyacineCore.Server.GameServer.Server.Packet.Send.Quest;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Quest;

[Opcode(CmdIds.FinishQuestCsReq)]
public class HandlerFinishQuestCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = FinishQuestCsReq.Parser.ParseFrom(data);
        var retCode = await connection.Player!.QuestManager!.FinishQuestByClient((int)req.QuestId);
        await connection.SendPacket(new PacketFinishQuestScRsp(retCode));
    }
}