using HyacineCore.Server.Database.Inventory;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Item;

public class PacketExpUpEquipmentScRsp : BasePacket
{
    public PacketExpUpEquipmentScRsp(List<ItemData> returnItem, Retcode code = Retcode.RetSucc) : base(CmdIds.ExpUpEquipmentScRsp)
    {
        var proto = new ExpUpEquipmentScRsp();
        proto.ReturnItemList.AddRange(returnItem.Select(item => item.ToPileProto()));
        proto.Retcode = (uint)code;

        SetData(proto);
    }
}
