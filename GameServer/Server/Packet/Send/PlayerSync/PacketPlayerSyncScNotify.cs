using HyacineCore.Server.Data;
using HyacineCore.Server.Database.Avatar;
using HyacineCore.Server.Database.Inventory;
using HyacineCore.Server.Database.Message;
using HyacineCore.Server.Database.Quests;
using HyacineCore.Server.Enums.Item;
using HyacineCore.Server.GameServer.Game.Sync;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.PlayerSync;

public class PacketPlayerSyncScNotify : BasePacket
{
    public PacketPlayerSyncScNotify(List<BaseSyncData> datas) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify();

        foreach (var data in datas) data.SyncData(proto);

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(ItemData item) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify();
        AddItemToProto(item, proto);
        SetData(proto);
    }

    public PacketPlayerSyncScNotify(List<ItemData> item) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify();
        foreach (var i in item) AddItemToProto(i, proto);
        SetData(proto);
    }

    public PacketPlayerSyncScNotify(BaseAvatarInfo avatar) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify
        {
            AvatarSync = new AvatarSync()
        };
        AddAvatarToProto(avatar, proto.AvatarSync);

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(List<BaseAvatarInfo> avatars) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify
        {
            AvatarSync = new AvatarSync()
        };

        foreach (var avatar in avatars)
        {
            AddAvatarToProto(avatar, proto.AvatarSync);
        }

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(List<FormalAvatarInfo> avatars) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify
        {
            AvatarSync = new AvatarSync()
        };

        foreach (var avatar in avatars)
            AddAvatarToProto(avatar, proto.AvatarSync);

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(BaseAvatarInfo avatar, ItemData item) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify();
        AddItemToProto(item, proto);
        proto.AvatarSync = new AvatarSync();
        AddAvatarToProto(avatar, proto.AvatarSync);

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(MissionSync mission) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify
        {
            MissionSync = mission
        };

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(PlayerBasicInfo info) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify
        {
            BasicInfo = info
        };

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(PlayerBasicInfo info, List<ItemData> item) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify
        {
            BasicInfo = info
        };

        foreach (var i in item) AddItemToProto(i, proto);

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(MessageGroupData? groupData, MessageSectionData? sectionData) : base(
        CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify
        {
            SyncStatus = new SyncStatus()
        };

        if (groupData != null)
            proto.SyncStatus.MessageGroupStatus.Add(new GroupStatus
            {
                GroupId = (uint)groupData.GroupId,
                GroupStatus_ = groupData.Status,
                RefreshTime = groupData.RefreshTime
            });

        if (sectionData != null)
            proto.SyncStatus.SectionStatus.Add(new SectionStatus
            {
                SectionId = (uint)sectionData.SectionId,
                SectionStatus_ = sectionData.Status
            });

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(QuestInfo quest) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify();
        proto.QuestList.Add(quest.ToProto());

        SetData(proto);
    }

    public PacketPlayerSyncScNotify(List<QuestInfo> quest) : base(CmdIds.PlayerSyncScNotify)
    {
        var proto = new PlayerSyncScNotify();
        proto.QuestList.Add(quest.Select(x => x.ToProto()));

        SetData(proto);
    }

    private void AddItemToProto(ItemData item, PlayerSyncScNotify notify)
    {
        GameData.ItemConfigData.TryGetValue(item.ItemId, out var itemConfig);
        if (itemConfig == null) return;
        switch (itemConfig.ItemMainType)
        {
            case ItemMainTypeEnum.Equipment:
                if (item.Count > 0)
                    notify.EquipmentList.Add(item.ToEquipmentProto());
                else
                    notify.DelEquipmentList.Add((uint)item.UniqueId);
                break;
            case ItemMainTypeEnum.Relic:
                if (item.Count > 0)
                    notify.RelicList.Add(item.ToRelicProto());
                else
                    notify.DelRelicList.Add((uint)item.UniqueId);
                break;
            case ItemMainTypeEnum.Mission:
            case ItemMainTypeEnum.Material:
            case ItemMainTypeEnum.Pet:
            case ItemMainTypeEnum.Usable:
                notify.MaterialList.Add(item.ToMaterialProto());
                break;
        }
    }

    private static void AddAvatarToProto(BaseAvatarInfo avatar, AvatarSync sync)
    {
        sync.AvatarList.Add(avatar.ToProto());

        if (avatar is FormalAvatarInfo formalAvatar)
            sync.AvatarPathDataInfoList.Add(formalAvatar.ToAvatarPathProto());
    }
}
