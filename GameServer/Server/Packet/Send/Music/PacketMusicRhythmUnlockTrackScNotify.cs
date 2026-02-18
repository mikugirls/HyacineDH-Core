using HyacineCore.Server.Data;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Music;

public class PacketMusicRhythmUnlockTrackScNotify : BasePacket
{
    public PacketMusicRhythmUnlockTrackScNotify() : base(CmdIds.MusicRhythmUnlockTrackScNotify)
    {
        var proto = new MusicRhythmUnlockTrackScNotify();

        foreach (var sfx in GameData.MusicRhythmTrackData.Values) proto.IELLAKHIONO.Add((uint)sfx.GetId());

        SetData(proto);
    }
}
