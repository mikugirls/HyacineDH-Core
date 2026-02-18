using HyacineCore.Server.GameServer.Game.Player.Components;
using HyacineCore.Server.GameServer.Server.Packet.Send.SwitchHand;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;
using HyacineCore.Server.Util;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.SwitchHand;

[Opcode(CmdIds.SwitchHandResetHandPosCsReq)]
public class HandlerSwitchHandResetHandPosCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SwitchHandResetTransformCsReq.Parser.ParseFrom(data);

        var component = connection.Player!.GetComponent<SwitchHandComponent>();

        var info = component.GetHandInfo((int)req.ConfigId);
        if (info.Item2 == null)
        {
            await connection.SendPacket(new PacketSwitchHandResetHandPosScRsp(info.Item1));
        }
        else
        {
            info.Item2.Pos = req.PKGLJDIHGCC.Pos.ToPosition();
            info.Item2.Rot = req.PKGLJDIHGCC.Rot.ToPosition();

            await connection.SendPacket(new PacketSwitchHandResetHandPosScRsp(info.Item2));
        }
    }
}
