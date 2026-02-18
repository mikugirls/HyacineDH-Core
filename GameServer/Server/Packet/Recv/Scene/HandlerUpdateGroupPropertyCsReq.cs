using HyacineCore.Server.GameServer.Server.Packet.Send.Scene;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.UpdateGroupPropertyCsReq)]
public class HandlerUpdateGroupPropertyCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UpdateGroupPropertyCsReq.Parser.ParseFrom(data);

        if (req.FloorId != connection.Player!.SceneInstance!.FloorId)
        {
            await connection.SendPacket(new PacketUpdateGroupPropertyScRsp(Retcode.RetReqParaInvalid));
            return;
        }

        // try to get group
        var scene = connection.Player.SceneInstance;
        if (!scene.Groups.Contains((int)req.GroupId))
        {
            await connection.SendPacket(new PacketUpdateGroupPropertyScRsp(Retcode.RetGroupNotExist));
            return;
        }

        // update group property
        var res = await scene.UpdateGroupProperty((int)req.GroupId, req.GCJKIDIBJHJ, req.KLDPELLOBDJ);
        await connection.SendPacket(new PacketUpdateGroupPropertyScRsp(res, req));
    }
}
