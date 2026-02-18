using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandCoinUpdateScRsp : BasePacket
{
    public PacketSwitchHandCoinUpdateScRsp(Retcode ret) : base(CmdIds.SwitchHandCoinUpdateScRsp)
    {
        var proto = new MFFOCLIECJJ
        {
            Retcode = (uint)ret
        };
        SetData(proto);
    }

    public PacketSwitchHandCoinUpdateScRsp(uint coinNum) : base(CmdIds.SwitchHandCoinUpdateScRsp)
    {
        var proto = new MFFOCLIECJJ
        {
            CKNFABPOMBL = coinNum
        };
        SetData(proto);
    }
}
