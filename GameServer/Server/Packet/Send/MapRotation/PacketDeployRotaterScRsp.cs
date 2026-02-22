using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.MapRotation;

public class PacketDeployRotaterScRsp : BasePacket
{
    public PacketDeployRotaterScRsp(RotaterData rotaterData, int curNum, int maxNum) : base(CmdIds.DeployRotatorScRsp)
    {
        var proto = new DeployRotatorScRsp
        {
            EnergyInfo = new RotaterEnergyInfo
            {
                MaxNum = (uint)maxNum,
                CurNum = (uint)curNum
            },
            RotaterData = rotaterData
        };

        SetData(proto);
    }
}
