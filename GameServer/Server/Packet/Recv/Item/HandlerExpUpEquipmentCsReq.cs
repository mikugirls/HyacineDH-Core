using HyacineCore.Server.GameServer.Server.Packet.Send.Item;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Item;

[Opcode(CmdIds.ExpUpEquipmentCsReq)]
public class HandlerExpUpEquipmentCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ExpUpEquipmentCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        var result = await player.InventoryManager!.LevelUpEquipment((int)req.EquipmentUniqueId, req.CostData);

        await connection.SendPacket(new PacketExpUpEquipmentScRsp(result.ReturnItems, result.Retcode));
    }
}
