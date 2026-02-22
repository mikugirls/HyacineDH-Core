using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Chat;

[Opcode(CmdIds.SendMsgCsReq)]
public class HandlerSendMsgCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SendMsgCsReq.Parser.ParseFrom(data);
        var messageData = req.MessageDatas;
        var chatData = messageData?.ChatData;
        var text = chatData?.HasMessageText == true ? chatData.MessageText.Trim('\0').Trim() : null;
        var extraId = chatData?.HasExtraId == true ? chatData.ExtraId : 0u;
        var msgType = messageData?.MessageType ?? MsgType.None;

        if (msgType == MsgType.None)
        {
            if (!string.IsNullOrWhiteSpace(text))
                msgType = MsgType.CustomText;
            else if (extraId != 0)
                msgType = MsgType.Emoji;
        }

        if (req.TargetList.Count == 0)
        {
            await connection.SendPacket(CmdIds.SendMsgScRsp);
            return;
        }

        foreach (var targetUid in req.TargetList)
        {
            if (msgType == MsgType.Emoji && extraId != 0)
            {
                await connection.Player!.FriendManager!.SendMessage(connection.Player!.Uid, (int)targetUid, null,
                    (int)extraId);
            }
            else if (!string.IsNullOrWhiteSpace(text))
            {
                await connection.Player!.FriendManager!.SendMessage(connection.Player!.Uid, (int)targetUid, text);
            }
        }

        await connection.SendPacket(CmdIds.SendMsgScRsp);
    }
}
