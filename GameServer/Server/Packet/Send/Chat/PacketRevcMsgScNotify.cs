using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Chat;

public class PacketRevcMsgScNotify : BasePacket
{
    public PacketRevcMsgScNotify(uint toUid, uint fromUid, string msg) : base(CmdIds.RevcMsgScNotify)
    {
        var chat = ChatMessageBuilder.BuildText(fromUid, msg);

        var proto = new RevcMsgScNotify
        {
            ChatType = ChatType.Private,
            SourceUid = fromUid,
            RecvMessageData = chat
        };

        SetData(proto);
    }

    public PacketRevcMsgScNotify(uint toUid, uint fromUid, uint extraId) : base(CmdIds.RevcMsgScNotify)
    {
        var chat = ChatMessageBuilder.BuildEmoji(fromUid, extraId);

        var proto = new RevcMsgScNotify
        {
            ChatType = ChatType.Private,
            SourceUid = fromUid,
            RecvMessageData = chat
        };

        SetData(proto);
    }
}
