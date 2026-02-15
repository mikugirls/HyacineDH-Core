using HyacineCore.Server.GameServer.Server.Packet.Send.Avatar;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.AvatarExpUpCsReq)]
public class HandlerAvatarExpUpCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = AvatarExpUpCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        var result = await player.InventoryManager!.LevelUpAvatar((int)req.BaseAvatarId, req.ItemCost);

        await connection.SendPacket(new PacketAvatarExpUpScRsp(result.ReturnItems, result.Retcode));
    }
}
