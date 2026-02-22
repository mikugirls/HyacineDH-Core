using HyacineCore.Server.GameServer.Game.Player.Components;
using HyacineCore.Server.GameServer.Server.Packet.Send.SwitchHand;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;
using HyacineCore.Server.Util;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.SwitchHand;

[Opcode(CmdIds.SwitchHandResetTransformCsReq)]
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
            info.Item2.Pos = req.ALMMDIOABGJ?.Pos.ToPosition() ?? info.Item2.Pos;
            info.Item2.Rot = req.ALMMDIOABGJ?.Rot.ToPosition() ?? info.Item2.Rot;

            await connection.SendPacket(new PacketSwitchHandResetHandPosScRsp(info.Item2));
        }
    }
}
