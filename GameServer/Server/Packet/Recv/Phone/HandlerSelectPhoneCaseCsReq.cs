using HyacineCore.Server.GameServer.Server.Packet.Send.Phone;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Phone;

[Opcode(CmdIds.SelectPhoneCaseCsReq)]
public class HandlerSelectPhoneCaseCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SelectPhoneCaseCsReq.Parser.ParseFrom(data);

        connection.Player!.Data.PhoneCase = (int)req.PhoneCaseId;

        await connection.SendPacket(new PacketSelectPhoneCaseScRsp(req.PhoneCaseId));
    }
}
