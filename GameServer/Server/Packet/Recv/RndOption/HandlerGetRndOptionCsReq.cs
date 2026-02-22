using HyacineCore.Server.Kcp;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.RndOption;

[Opcode(CmdIds.GetRndOptionCsReq)]
public class HandlerGetRndOptionCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(CmdIds.None);
    }
}
