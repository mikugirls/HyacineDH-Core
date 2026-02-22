using HyacineCore.Server.GameServer.Server.Packet.Send.Mission;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Mission;

[Opcode(CmdIds.UpdateTrackMainMissionCsReq)]
public class HandlerUpdateTrackMainMissionIdCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UpdateTrackMainMissionCsReq.Parser.ParseFrom(data);

        var prev = connection.Player!.MissionManager!.Data.TrackingMainMissionId;
        connection.Player!.MissionManager!.Data.TrackingMainMissionId = (int)req.TrackMissionId;

        await connection.SendPacket(new PacketUpdateTrackMainMissionIdScRsp(prev,
            connection.Player!.MissionManager!.Data.TrackingMainMissionId));
    }
}
