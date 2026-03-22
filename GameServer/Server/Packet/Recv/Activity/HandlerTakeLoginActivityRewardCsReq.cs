// 文件路径: GameServer/Server/Packet/Recv/Activity/HandlerTakeLoginActivityRewardCsReq.cs
using HyacineCore.Server.GameServer.Server;
using HyacineCore.Server.GameServer.Server.Packet.Send.Activity;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Activity;

[Opcode(CmdIds.TakeLoginActivityRewardCsReq)]
public class HandlerTakeLoginActivityRewardCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = TakeLoginActivityRewardCsReq.Parser.ParseFrom(data);
        var player = connection.Player;

        if (player?.ActivityManager == null) return;

        // 解包 ActivityManager 返回的 (items, panelId, retcode)
        var (rewardProto, panelId, retcode) = await player.ActivityManager.TakeLoginReward(req.Id, req.TakeDays);

        // 发送 Packet，带入动态 panelId
        await connection.SendPacket(new PacketTakeLoginActivityRewardScRsp(req.Id, req.TakeDays, retcode, rewardProto, panelId));
    }
}
