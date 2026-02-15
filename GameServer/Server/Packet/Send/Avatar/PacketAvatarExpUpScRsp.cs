using HyacineCore.Server.Database.Inventory;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Avatar;

public class PacketAvatarExpUpScRsp : BasePacket
{
    public PacketAvatarExpUpScRsp(List<ItemData> returnItem, Retcode code = Retcode.RetSucc) : base(CmdIds.AvatarExpUpScRsp)
    {
        var proto = new AvatarExpUpScRsp();
        proto.ReturnItemList.AddRange(returnItem.Select(item => item.ToPileProto()));
        proto.Retcode = (uint)code;

        SetData(proto);
    }
}
