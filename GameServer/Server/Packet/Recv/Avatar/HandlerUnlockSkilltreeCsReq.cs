using HyacineCore.Server.Data;
using HyacineCore.Server.Enums.Mission;
using HyacineCore.Server.GameServer.Server.Packet.Send.Avatar;
using HyacineCore.Server.GameServer.Server.Packet.Send.PlayerSync;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.UnlockSkillTreeCsReq)]
public class HandlerUnlockSkilltreeCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UnlockSkillTreeCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        GameData.AvatarSkillTreeConfigData.TryGetValue((int)(req.PointId * 100 + req.Level), out var config);
        if (config == null)
        {
            await connection.SendPacket(new PacketUnlockSkilltreeScRsp(Retcode.RetSkilltreeConfigNotExist));
            return;
        }

        var avatar = player.AvatarManager!.GetFormalAvatar(config.AvatarID);
        if (avatar == null)
        {
            await connection.SendPacket(new PacketUnlockSkilltreeScRsp(Retcode.RetAvatarNotExist));
            return;
        }

        foreach (var cost in req.ItemList)
            await connection.Player!.InventoryManager!.RemoveItem((int)cost.PileItem.ItemId,
                (int)cost.PileItem.ItemNum);

        avatar.GetCurPathInfo().GetSkillTree()[(int)req.PointId] = (int)req.Level;

        await connection.SendPacket(new PacketPlayerSyncScNotify(avatar));

        player.MissionManager?.HandleFinishType(MissionFinishTypeEnum.UnlockSkilltreeCnt, "UnlockSkillTree");
        player.MissionManager?.HandleFinishType(MissionFinishTypeEnum.UnlockSkilltree, "UnlockSkillTree");
        player.MissionManager?.HandleFinishType(MissionFinishTypeEnum.AllAvatarUnlockSkilltreeCnt, "UnlockSkillTree");

        await connection.SendPacket(new PacketUnlockSkilltreeScRsp(req.PointId, req.Level));
    }
}
