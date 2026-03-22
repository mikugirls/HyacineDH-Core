using HyacineCore.Server.Enums.Mission;
using HyacineCore.Server.GameServer.Server.Packet.Send.HeartDial;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.HeartDial;

[Opcode(CmdIds.FinishEmotionDialoguePerformanceCsReq)]
public class HandlerFinishEmotionDialoguePerformanceCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = FinishEmotionDialoguePerformanceCsReq.Parser.ParseFrom(data);

        var player = connection.Player!;
        await player.MissionManager!.HandleFinishType(MissionFinishTypeEnum.HeartDialDialoguePerformanceFinish,
            $"HeartDial_{req.DialogueId}");

        await connection.SendPacket(new PacketFinishEmotionDialoguePerformanceScRsp(req.ScriptId, req.DialogueId));
    }
}