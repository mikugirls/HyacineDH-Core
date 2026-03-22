using HyacineCore.Server.GameServer.Server.Packet.Send.JukeBox;
using HyacineCore.Server.Kcp;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.JukeBox;

[Opcode(CmdIds.GetJukeboxDataCsReq)]
public class HandlerGetJukeboxDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetJukeboxDataScRsp(connection.Player!));
    }
}