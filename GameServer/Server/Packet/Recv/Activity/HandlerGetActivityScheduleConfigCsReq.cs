using HyacineCore.Server.GameServer.Server.Packet.Send.Activity;
using HyacineCore.Server.Kcp;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Activity;

[Opcode(CmdIds.GetActivityScheduleConfigCsReq)]
public class HandlerGetActivityScheduleConfigCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetActivityScheduleConfigScRsp(connection.Player!));
    }
}