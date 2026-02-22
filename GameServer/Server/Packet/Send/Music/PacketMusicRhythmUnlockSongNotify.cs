using HyacineCore.Server.Data;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Music;

public class PacketMusicRhythmUnlockSongNotify : BasePacket
{
    public PacketMusicRhythmUnlockSongNotify() : base(CmdIds.MusicRhythmUnlockSongNotify)
    {
        _ = GameData.MusicRhythmSongData.Count;
        var proto = new MusicRhythmUnlockSongNotify();

        SetData(proto);
    }
}
