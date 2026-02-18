using HyacineCore.Server.Database.Scene;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandUpdateScRsp : BasePacket
{
    public PacketSwitchHandUpdateScRsp(SwitchHandInfo info, CNIADJCLKKF? operationInfo) : base(
        CmdIds.SwitchHandUpdateScRsp)
    {
        var proto = new GetSwitchHandUpdateScRsp
        {
            GNHGNIGGOBF = info.ToProto(),
            ALOHEJACLLN = operationInfo ?? new CNIADJCLKKF()
        };
        SetData(proto);
    }

    public PacketSwitchHandUpdateScRsp(Retcode ret, CNIADJCLKKF? operationInfo) : base(
        CmdIds.SwitchHandUpdateScRsp)
    {
        var proto = new GetSwitchHandUpdateScRsp
        {
            Retcode = (uint)ret,
            ALOHEJACLLN = operationInfo ?? new CNIADJCLKKF()
        };

        SetData(proto);
    }
}
