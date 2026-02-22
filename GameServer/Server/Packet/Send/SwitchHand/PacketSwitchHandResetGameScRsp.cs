using HyacineCore.Server.Database.Scene;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandResetGameScRsp : BasePacket
{
    public PacketSwitchHandResetGameScRsp(SwitchHandInfo info) : base(CmdIds.GetSwitchHandResetGameScRsp)
    {
        var proto = new GetSwitchHandResetGameScRsp
        {
            BDCBCCOOLHE = info.ToSwitchHandProto()
        };

        SetData(proto);
    }

    public PacketSwitchHandResetGameScRsp(Retcode ret) : base(CmdIds.GetSwitchHandResetGameScRsp)
    {
        var proto = new GetSwitchHandResetGameScRsp
        {
            Retcode = (uint)ret
        };

        SetData(proto);
    }
}
