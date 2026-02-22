using HyacineCore.Server.Kcp;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.PlayerLogoutCsReq)]
public class HandlerPlayerLogoutCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(CmdIds.None);
        connection.Stop();
    }
}
