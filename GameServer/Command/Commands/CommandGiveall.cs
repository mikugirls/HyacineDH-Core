using HyacineCore.Server.Data;
using HyacineCore.Server.Database.Inventory;
using HyacineCore.Server.Enums.Avatar;
using HyacineCore.Server.Enums.Item;
using HyacineCore.Server.GameServer.Server.Packet.Send.Avatar;
using HyacineCore.Server.GameServer.Server.Packet.Send.PlayerSync;
using HyacineCore.Server.Internationalization;

namespace HyacineCore.Server.Command.Command.Cmd;

[CommandInfo("giveall", "Game.Command.GiveAll.Desc", "Game.Command.GiveAll.Usage", ["ga"])]
public class CommandGiveall : ICommand
{
    [CommandMethod("0 avatar")]
    public async ValueTask GiveAllAvatar(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("r", out var rankStr);
        arg.CharacterArgs.TryGetValue("l", out var levelStr);
        rankStr ??= "0";
        levelStr ??= "1";
        if (!int.TryParse(rankStr, out var rank) || !int.TryParse(levelStr, out var level))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var avatarList = GameData.AvatarConfigData.Values;
        foreach (var avatar in avatarList)
        {
            if (avatar.AvatarID > 2000 && avatar.AvatarID != 8001)
                continue; // Hacky way to prevent giving random avatars
            if (player.AvatarManager!.GetFormalAvatar(avatar.AvatarID) == null)
            {
                GameData.MultiplePathAvatarConfigData.TryGetValue(avatar.AvatarID, out var multiPathAvatar);
                if (multiPathAvatar != null && avatar.AvatarID != multiPathAvatar.BaseAvatarID) continue;
                // Normal avatar
                await player.InventoryManager!.AddItem(avatar.AvatarID, 1, false, sync: false);
                player.AvatarManager!.GetFormalAvatar(avatar.AvatarID)!.Level = Math.Max(Math.Min(level, 80), 0);
                player.AvatarManager!.GetFormalAvatar(avatar.AvatarID)!.Promotion =
                    GameData.GetMinPromotionForLevel(Math.Max(Math.Min(level, 80), 0));
                player.AvatarManager!.GetFormalAvatar(avatar.AvatarID)!.GetCurPathInfo().Rank =
                    Math.Max(Math.Min(rank, 6), 0);
            }
            else
            {
                player.AvatarManager!.GetFormalAvatar(avatar.AvatarID)!.Level = Math.Max(Math.Min(level, 80), 0);
                player.AvatarManager!.GetFormalAvatar(avatar.AvatarID)!.Promotion =
                    GameData.GetMinPromotionForLevel(Math.Max(Math.Min(level, 80), 0));
                player.AvatarManager!.GetFormalAvatar(avatar.AvatarID)!.GetCurPathInfo().Rank =
                    Math.Max(Math.Min(rank, 6), 0);
            }
        }

        await player.SendPacket(new PacketPlayerSyncScNotify(player.AvatarManager!.AvatarData.FormalAvatars));

        await arg.SendMsg(I18NManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18NManager.Translate("Word.Avatar"), "1"));
    }

    [CommandMethod("0 equipment")]
    public async ValueTask GiveAllLightcone(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("r", out var rankStr);
        arg.CharacterArgs.TryGetValue("l", out var levelStr);
        arg.CharacterArgs.TryGetValue("x", out var amountStr);
        rankStr ??= "1";
        levelStr ??= "1";
        amountStr ??= "1";
        if (!int.TryParse(rankStr, out var rank) || !int.TryParse(levelStr, out var level) ||
            !int.TryParse(amountStr, out var amount))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var lightconeList = GameData.EquipmentConfigData.Values;
        var items = new List<ItemData>();

        for (var i = 0; i < amount; i++)
            foreach (var lightcone in lightconeList)
            {
                var item = await player.InventoryManager!.AddItem(lightcone.EquipmentID, 1, false,
                    Math.Max(Math.Min(rank, 5), 0), Math.Max(Math.Min(level, 80), 0), false);

                if (item != null)
                    items.Add(item);
            }

        await player.SendPacket(new PacketPlayerSyncScNotify(items));

        await arg.SendMsg(I18NManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18NManager.Translate("Word.Equipment"), amount.ToString()));
    }

    [CommandMethod("0 material")]
    public async ValueTask GiveAllMaterial(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("x", out var amountStr);
        amountStr ??= "1";
        if (!int.TryParse(amountStr, out var amount) || amount <= 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var consumableUsableSubTypes = new HashSet<ItemSubTypeEnum>
        {
            ItemSubTypeEnum.Food,
            ItemSubTypeEnum.Book,
            ItemSubTypeEnum.FindChest,
            ItemSubTypeEnum.Gift,
            ItemSubTypeEnum.ForceOpitonalGift
        };

        var materialList = GameData.ItemConfigData.Values;
        var items = new List<ItemData>();
        foreach (var material in materialList)
            if (material.ID > 0 && material.PileLimit > 0 &&
                ((material.ItemMainType == ItemMainTypeEnum.Material) ||
                 (material.ItemMainType == ItemMainTypeEnum.Usable &&
                  consumableUsableSubTypes.Contains(material.ItemSubType))))
                items.Add(new ItemData
                {
                    ItemId = material.ID,
                    Count = amount
                });

        await player.InventoryManager!.AddItems(items, false);
        await arg.SendMsg(I18NManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18NManager.Translate("Word.Material"), amount.ToString()));
    }

    [CommandMethod("0 pet")]
    public async ValueTask GiveAllPet(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("x", out var amountStr);
        amountStr ??= "1";
        if (!int.TryParse(amountStr, out var amount))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var petList = GameData.ItemConfigData.Values;
        var items = new List<ItemData>();
        foreach (var pet in petList)
            if (pet.ItemMainType == ItemMainTypeEnum.Pet)
                items.Add(new ItemData
                {
                    ItemId = pet.ID,
                    Count = amount
                });
        await player.InventoryManager!.AddItems(items);
        await arg.SendMsg(I18NManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18NManager.Translate("Word.Pet"), "1"));
    }

    [CommandMethod("0 relic")]
    public async ValueTask GiveAllRelic(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("l", out var levelStr);
        levelStr ??= "1";
        if (!int.TryParse(levelStr, out var level))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        arg.CharacterArgs.TryGetValue("x", out var amountStr);
        amountStr ??= "1";
        if (!int.TryParse(amountStr, out var amount))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var relicList = GameData.RelicConfigData.Values;
        var items = new List<ItemData>();

        for (var i = 0; i < amount; i++)
            foreach (var relic in relicList)
            {
                var item = await player.InventoryManager!.AddItem(relic.ID, amount, false, 1,
                    Math.Max(Math.Min(level, relic.MaxLevel), 1), false);

                if (item != null)
                    items.Add(item);
            }

        await player.SendPacket(new PacketPlayerSyncScNotify(items));

        await arg.SendMsg(I18NManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18NManager.Translate("Word.Relic"), amount.ToString()));
    }

    [CommandMethod("0 unlock")]
    public async ValueTask GiveAllUnlock(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var materialList = GameData.ItemConfigData.Values;
        foreach (var material in materialList)
            if (material.ItemMainType == ItemMainTypeEnum.Usable)
                if (material.ItemSubType == ItemSubTypeEnum.HeadIcon ||
                    material.ItemSubType == ItemSubTypeEnum.PhoneTheme ||
                    material.ItemSubType == ItemSubTypeEnum.ChatBubble ||
                    material.ItemSubType == ItemSubTypeEnum.PersonalCard ||
                    material.ItemSubType == ItemSubTypeEnum.PhoneCase ||
                    material.ItemSubType == ItemSubTypeEnum.AvatarSkin)
                    await player.InventoryManager!.AddItem(material.ID, 1, false);

        await arg.SendMsg(I18NManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18NManager.Translate("Word.Unlock"), "1"));
    }

    [CommandMethod("0 path")]
    public async ValueTask GiveAllPath(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        foreach (var multiPathAvatar in GameData.MultiplePathAvatarConfigData.Values)
        {
            if (player.AvatarManager!.GetFormalAvatar(multiPathAvatar.BaseAvatarID) == null)
            {
                await player.InventoryManager!.AddItem(multiPathAvatar.BaseAvatarID, 1, false, sync: false);
                player.AvatarManager!.GetFormalAvatar(multiPathAvatar.BaseAvatarID)!.Level =
                    Math.Max(Math.Min(1, 80), 0);
                player.AvatarManager!.GetFormalAvatar(multiPathAvatar.BaseAvatarID)!.Promotion =
                    GameData.GetMinPromotionForLevel(Math.Max(Math.Min(1, 80), 0));
                player.AvatarManager!.GetFormalAvatar(multiPathAvatar.BaseAvatarID)!.GetCurPathInfo().Rank =
                    Math.Max(Math.Min(0, 6), 0);
            }

            var avatarData = player.AvatarManager!.GetFormalAvatar(multiPathAvatar.BaseAvatarID)!;
            if (avatarData.PathInfos.ContainsKey(multiPathAvatar.AvatarID)) continue;
            if (multiPathAvatar.BaseAvatarID > 8000 && multiPathAvatar.AvatarID % 2 != 1) continue;
            await player.ChangeAvatarPathType(multiPathAvatar.BaseAvatarID,
                (MultiPathAvatarTypeEnum)multiPathAvatar.AvatarID);
        }

        await player.SendPacket(new PacketPlayerSyncScNotify(player.AvatarManager!.AvatarData.FormalAvatars));

        await arg.SendMsg(I18NManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18NManager.Translate("Word.Avatar"),
            "1"));
    }

    [CommandMethod("0 skin")]
    public async ValueTask GiveAllSkin(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var unlockedAvatarSkinCount = 0;
        foreach (var avatarSkin in GameData.AvatarSkinData.Values.OrderBy(x => x.ID))
        {
            if (!player.PlayerUnlockData!.Skins.TryGetValue(avatarSkin.AvatarID, out var skins))
            {
                skins = [];
                player.PlayerUnlockData.Skins[avatarSkin.AvatarID] = skins;
            }

            if (skins.Contains(avatarSkin.ID)) continue;
            skins.Add(avatarSkin.ID);
            unlockedAvatarSkinCount++;

            await player.SendPacket(new PacketUnlockAvatarSkinScNotify(avatarSkin.ID));
        }

        var outfitCount = 0;
        List<ItemData> syncItems = [];
        var outfitIds = GameData.ItemConfigData.Values
            .Where(x => x.ItemMainType == ItemMainTypeEnum.Usable &&
                        (x.ItemSubType == ItemSubTypeEnum.PlayerOutfit ||
                         x.ItemSubType == ItemSubTypeEnum.HipplenOutfit))
            .Select(x => x.ID)
            .Distinct()
            .OrderBy(x => x);

        foreach (var outfitId in outfitIds)
        {
            if (player.InventoryManager!.GetItem(outfitId) != null) continue;

            var item = await player.InventoryManager.AddItem(outfitId, 1, false, sync: false, returnRaw: true);
            if (item == null) continue;

            syncItems.Add(item);
            outfitCount++;
        }

        if (syncItems.Count > 0)
            await player.SendPacket(new PacketPlayerSyncScNotify(syncItems));

        await arg.SendMsg(
            $"Unlocked avatar skins: {unlockedAvatarSkinCount}, Trailblazer outfits: {outfitCount}");
    }
}
