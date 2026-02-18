using HyacineCore.Server.Enums.Item;
using HyacineCore.Server.GameServer.Server.Packet.Send.Item;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Item;

[Opcode(CmdIds.LockRelicCsReq)]
public class HandlerLockRelicCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = LockRelicCsReq.Parser.ParseFrom(data);
        var result =
            await connection.Player!.InventoryManager!.LockItems(req.RelicIds, req.IsLocked,
                ItemMainTypeEnum.Relic);
        await connection.SendPacket(new PacketLockRelicScRsp(result));
    }
}
