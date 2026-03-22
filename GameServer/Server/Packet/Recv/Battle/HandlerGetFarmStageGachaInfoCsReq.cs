using HyacineCore.Server.GameServer.Server.Packet.Send.Battle;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Battle;

[Opcode(CmdIds.GetFarmStageGachaInfoCsReq)]
public class HandlerGetFarmStageGachaInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetFarmStageGachaInfoCsReq.Parser.ParseFrom(data);
        await connection.SendPacket(new PacketGetFarmStageGachaInfoScRsp(req));
    }
}