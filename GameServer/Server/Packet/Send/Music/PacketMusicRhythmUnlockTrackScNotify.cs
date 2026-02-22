using HyacineCore.Server.Data;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Music;

public class PacketMusicRhythmUnlockTrackScNotify : BasePacket
{
    public PacketMusicRhythmUnlockTrackScNotify() : base(CmdIds.MusicRhythmUnlockTrackScNotify)
    {
        _ = GameData.MusicRhythmTrackData.Count;
        var proto = new MusicRhythmUnlockTrackScNotify();

        SetData(proto);
    }
}
