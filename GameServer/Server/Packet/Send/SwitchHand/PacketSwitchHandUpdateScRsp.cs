using HyacineCore.Server.Database.Scene;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.SwitchHand;

public class PacketSwitchHandUpdateScRsp : BasePacket
{
    public PacketSwitchHandUpdateScRsp(SwitchHandInfo info, MFJODIAILFL? operationInfo, GOPPKKEGPPG? actionInfo = null) : base(
        CmdIds.GetSwitchHandUpdateScRsp)
    {
        var proto = new GetSwitchHandUpdateScRsp
        {
            IILLGJKEOGC = operationInfo ?? info.ToSwitchHandProto(),
            FJEKJOFCGBJ = actionInfo ?? new GOPPKKEGPPG
            {
                GroupId = (uint)info.ConfigId,
                NLOPDLONLAF = info.State,
                PHOMPFCPNML = info.CoinNum < 0 ? 0u : (uint)info.CoinNum
            }
        };
        SetData(proto);
    }

    public PacketSwitchHandUpdateScRsp(Retcode ret, MFJODIAILFL? operationInfo, GOPPKKEGPPG? actionInfo = null) : base(
        CmdIds.GetSwitchHandUpdateScRsp)
    {
        var proto = new GetSwitchHandUpdateScRsp
        {
            Retcode = (uint)ret,
            IILLGJKEOGC = operationInfo ?? new MFJODIAILFL(),
            FJEKJOFCGBJ = actionInfo ?? new GOPPKKEGPPG()
        };

        SetData(proto);
    }
}
