using HyacineCore.Server.GameServer.Server.Packet.Send.ServerPrefs;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.ServerPrefs;

[Opcode(CmdIds.UpdateServerPrefsCsReq)]
public class HandlerUpdateServerPrefsDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UpdateServerPrefsCsReq.Parser.ParseFrom(data);

        connection.Player?.ServerPrefsData?.SetData((int)req.ServerPrefs.ServerPrefsId,
            req.ServerPrefs.Data.ToBase64());
        await connection.SendPacket(new PacketUpdateServerPrefsDataScRsp(req.ServerPrefs.ServerPrefsId));
    }
}
