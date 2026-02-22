using HyacineCore.Server.Data;
using HyacineCore.Server.GameServer.Game.Player;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.JukeBox;

public class PacketGetJukeboxDataScRsp : BasePacket
{
    public PacketGetJukeboxDataScRsp(PlayerInstance player) : base(CmdIds.GetJukeboxDataScRsp)
    {
        var proto = new GetJukeboxDataScRsp
        {
            DMCGIJPHHLI = new ILJPEPMBGCI
            {
                FIMOIDDDBPN = new DJAONBPALLI
                {
                    Id = (uint)player.Data.CurrentBgm
                }
            }
        };

        foreach (var music in GameData.BackGroundMusicData.Values)
            proto.UnlockedMusicList.Add(new MusicData
            {
                Id = (uint)music.ID,
                GroupId = (uint)music.GroupID,
                IsPlayed = true
            });

        SetData(proto);
    }
}
