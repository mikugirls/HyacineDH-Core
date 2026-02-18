using HyacineCore.Server.Data;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Music;

public class PacketMusicRhythmUnlockSongNotify : BasePacket
{
    public PacketMusicRhythmUnlockSongNotify() : base(CmdIds.MusicRhythmUnlockSongNotify)
    {
        var proto = new MusicRhythmUnlockSongNotify();

        foreach (var song in GameData.MusicRhythmSongData.Values) proto.ODJCMAEKOGG.Add((uint)song.GetId());

        SetData(proto);
    }
}
