using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.JukeBox;

public class PacketPlayBackGroundMusicScRsp : BasePacket
{
    public PacketPlayBackGroundMusicScRsp(uint musicId) : base(CmdIds.PlayBackGroundMusicScRsp)
    {
        var proto = new PlayBackGroundMusicScRsp
        {
            DMCGIJPHHLI = new ILJPEPMBGCI
            {
                FIMOIDDDBPN = new DJAONBPALLI
                {
                    Id = musicId
                }
            }
        };

        SetData(proto);
    }
}
