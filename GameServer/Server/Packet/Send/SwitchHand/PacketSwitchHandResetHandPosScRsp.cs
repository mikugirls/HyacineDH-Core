using HyacineCore.Server.Database.Scene;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandResetHandPosScRsp : BasePacket
{
    public PacketSwitchHandResetHandPosScRsp(SwitchHandInfo info) : base(CmdIds.GetSwitchHandResetHandPosScRsp)
    {
        var proto = new GetSwitchHandResetHandPosScRsp
        {
            BDCBCCOOLHE = info.ToSwitchHandProto()
        };

        SetData(proto);
    }

    public PacketSwitchHandResetHandPosScRsp(Retcode ret) : base(CmdIds.GetSwitchHandResetHandPosScRsp)
    {
        var proto = new GetSwitchHandResetHandPosScRsp
        {
            Retcode = (uint)ret
        };

        SetData(proto);
    }
}
