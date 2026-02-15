using HyacineCore.Server.GameServer.Game.Challenge.Instances;
using HyacineCore.Server.GameServer.Server.Packet.Send.Challenge;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Challenge;

[Opcode(CmdIds.EnterChallengeNextPhaseCsReq)]
public class HandlerEnterChallengeNextPhaseCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var challenge = connection.Player!.ChallengeManager?.ChallengeInstance;
        if (challenge == null)
        {
            await connection.SendPacket(new PacketEnterChallengeNextPhaseScRsp(Retcode.RetChallengeNotDoing));
            return;
        }

        if (challenge is ChallengeBossInstance boss)
        {
            var ok = await boss.NextPhase();
            if (!ok)
            {
                await connection.SendPacket(new PacketEnterChallengeNextPhaseScRsp(Retcode.RetChallengeNotDoing));
                return;
            }

            await connection.SendPacket(new PacketEnterChallengeNextPhaseScRsp(connection.Player));
            return;
        }

        if (challenge is ChallengeMemoryInstance or ChallengeStoryInstance)
        {
            // Memory/Story already switch stage server-side when stage 1 battle settles.
            // Reply with current scene so client can continue to stage 2 flow.
            await connection.SendPacket(new PacketEnterChallengeNextPhaseScRsp(connection.Player));
            return;
        }

        await connection.SendPacket(new PacketEnterChallengeNextPhaseScRsp(Retcode.RetChallengeNotDoing));
    }
}
