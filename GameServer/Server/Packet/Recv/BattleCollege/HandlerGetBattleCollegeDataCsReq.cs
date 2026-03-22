using HyacineCore.Server.GameServer.Server.Packet.Send.BattleCollege;
using HyacineCore.Server.Kcp;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.BattleCollege;

[Opcode(CmdIds.GetBattleCollegeDataCsReq)]
public class HandlerGetBattleCollegeDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetBattleCollegeDataScRsp(connection.Player!));
    }
}