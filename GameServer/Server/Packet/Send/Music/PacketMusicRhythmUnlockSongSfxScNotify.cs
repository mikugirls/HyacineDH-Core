using HyacineCore.Server.Data;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Music;

public class PacketMusicRhythmUnlockSongSfxScNotify : BasePacket
{
    public PacketMusicRhythmUnlockSongSfxScNotify() : base(CmdIds.MusicRhythmUnlockSongSfxScNotify)
    {
        var proto = new MusicRhythmUnlockSongSfxScNotify();

        foreach (var sfx in GameData.MusicRhythmSoundEffectData.Values) proto.ODJCMAEKOGG.Add((uint)sfx.GetId());

        SetData(proto);
    }
}
