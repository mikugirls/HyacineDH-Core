using HyacineCore.Server.GameServer.Server.Packet.Send.Quest; 
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Quest;

[Opcode(CmdIds.GetDailyActiveInfoCsReq)]
public class HandlerGetDailyActiveInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        // 1. 准备硬编码数据 (调用 QuestManager 里的函数)
        var rsp = connection.Player!.DailyActiveManager!.GetDailyActiveInfo();
        
        // 2. 构造你刚写的发送包
        var packet = new PacketGetDailyActiveInfoScRsp(rsp);
        
        // 3. 发送
        await connection.SendPacket(packet);
        
        // 4. 为了让界面刷出来，强行推一下任务状态 (QuestStatus.Doing)
        await connection.Player.DailyActiveManager.SyncDailyQuestsStatus();    }
}
