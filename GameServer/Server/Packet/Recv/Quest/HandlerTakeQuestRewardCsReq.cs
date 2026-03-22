using HyacineCore.Server.Database.Inventory;
using HyacineCore.Server.GameServer.Server.Packet.Send.Quest;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Quest;

[Opcode(CmdIds.TakeQuestRewardCsReq)]
public class HandlerTakeQuestRewardCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = TakeQuestRewardCsReq.Parser.ParseFrom(data);
        List<ItemData> rewardItems = [];
        var ret = Retcode.RetSucc;
        List<int> succQuestIds = [];

        foreach (var quest in req.SuccQuestIdList)
        {
            var (retCode, items) = await connection.Player!.QuestManager!.TakeQuestReward((int)quest);
            if (retCode != Retcode.RetSucc)
                ret = retCode;
            else
                succQuestIds.Add((int)quest);

            if (items != null) rewardItems.AddRange(items);
        }

        await connection.SendPacket(new PacketTakeQuestRewardScRsp(ret, rewardItems, succQuestIds));
    }
}