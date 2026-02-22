using HyacineCore.Server.GameServer.Server.Packet.Send.MapRotation;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.MapRotation;

[Opcode(CmdIds.DeployRotatorCsReq)]
public class HandlerDeployRotaterCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = DeployRotatorCsReq.Parser.ParseFrom(data);

        connection.Player!.ChargerNum--;
        await connection.SendPacket(new PacketDeployRotaterScRsp(req.RotaterData, connection.Player!.ChargerNum, 5));
    }
}
