using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandCoinUpdateScRsp : BasePacket
{
    public PacketSwitchHandCoinUpdateScRsp(Retcode ret) : base(CmdIds.PJGAKDEDHAH)
    {
        var proto = new PJGAKDEDHAH
        {
            Retcode = (uint)ret
        };
        SetData(proto);
    }

    public PacketSwitchHandCoinUpdateScRsp(uint coinNum) : base(CmdIds.PJGAKDEDHAH)
    {
        var proto = new PJGAKDEDHAH
        {
            JMMIHOEDFCG = coinNum
        };
        SetData(proto);
    }
}
