using HyacineCore.Server.GameServer.Game.Player;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.BattleCollege;

public class PacketBattleCollegeDataChangeScNotify : BasePacket
{
    public PacketBattleCollegeDataChangeScNotify(PlayerInstance player) : base(CmdIds.None)
    {
        _ = player.BattleCollegeData?.FinishedCollegeIdList.Count ?? 0;
        SetData([]);
    }
}
