using HyacineCore.Server.Data;
using HyacineCore.Server.Database;
using HyacineCore.Server.Database.Gacha;
using HyacineCore.Server.Database.Inventory;
using HyacineCore.Server.Enums;
using HyacineCore.Server.Enums.Item;
using HyacineCore.Server.GameServer.Game.Player;
using HyacineCore.Server.GameServer.Server.Packet.Send.PlayerSync;
using HyacineCore.Server.Proto;
using HyacineCore.Server.Util;
using GachaInfo = HyacineCore.Server.Database.Gacha.GachaInfo;

namespace HyacineCore.Server.GameServer.Game.Gacha;

public class GachaManager : BasePlayerManager
{
    public GachaManager(PlayerInstance player) : base(player)
    {
        GachaData = DatabaseHelper.Instance!.GetInstanceOrCreateNew<GachaData>(player.Uid);

        if (GachaData.GachaHistory.Count >= 50)
            GachaData.GachaHistory.RemoveRange(0, GachaData.GachaHistory.Count - 50);

        foreach (var order in GameData.DecideAvatarOrderData.Values.ToList().OrderBy(x => -x.Order))
        {
            if (GachaData.GachaDecideOrder.Contains(order.ItemID)) continue;
            GachaData.GachaDecideOrder.Add(order.ItemID);
        }
    }

    public GachaData GachaData { get; }

    public List<int> GetPurpleAvatars()
    {
        var purpleAvatars = new List<int>();
        foreach (var avatar in GameData.AvatarConfigData.Values)
            if (avatar.Rarity == RarityEnum.CombatPowerAvatarRarityType4 &&
                !(GameData.MultiplePathAvatarConfigData.ContainsKey(avatar.AvatarID) &&
                  GameData.MultiplePathAvatarConfigData[avatar.AvatarID].BaseAvatarID != avatar.AvatarID) &&
                avatar.MaxRank > 0)
                purpleAvatars.Add(avatar.AvatarID);
        return purpleAvatars;
    }

    public List<int> GetGoldAvatars()
    {
        return [1003, 1004, 1101, 1107, 1104, 1209, 1211];
    }

    public List<int> GetAllGoldAvatars()
    {
        var avatars = new List<int>();
        foreach (var avatar in GameData.AvatarConfigData.Values)
            if (avatar.Rarity == RarityEnum.CombatPowerAvatarRarityType5)
                avatars.Add(avatar.AvatarID);
        return avatars;
    }

    public List<int> GetBlueWeapons()
    {
        var purpleWeapons = new List<int>();
        foreach (var weapon in GameData.EquipmentConfigData.Values)
            if (weapon.Rarity == RarityEnum.CombatPowerLightconeRarity3)
                purpleWeapons.Add(weapon.EquipmentID);
        return purpleWeapons;
    }

    public List<int> GetPurpleWeapons()
    {
        var purpleWeapons = new List<int>();
        foreach (var weapon in GameData.EquipmentConfigData.Values)
            if (weapon.Rarity == RarityEnum.CombatPowerLightconeRarity4)
                purpleWeapons.Add(weapon.EquipmentID);
        return purpleWeapons;
    }

    public List<int> GetGoldWeapons()
    {
        return [23000, 23002, 23003, 23004, 23005, 23012, 23013];
    }

    public List<int> GetAllGoldWeapons()
    {
        var weapons = new List<int>();
        foreach (var weapon in GameData.EquipmentConfigData.Values)
            if (weapon.Rarity == RarityEnum.CombatPowerLightconeRarity5)
                weapons.Add(weapon.EquipmentID);
        return weapons;
    }

    public int GetRarity(int itemId)
    {
        if (GetAllGoldAvatars().Contains(itemId) || GetAllGoldWeapons().Contains(itemId)) return 5;

        if (GetPurpleAvatars().Contains(itemId) || GetPurpleWeapons().Contains(itemId)) return 4;

        if (GetBlueWeapons().Contains(itemId)) return 3;

        return 0;
    }

    public int GetType(int itemId)
    {
        if (GetAllGoldAvatars().Contains(itemId) || GetPurpleAvatars().Contains(itemId)) return 1;

        if (GetAllGoldWeapons().Contains(itemId) || GetPurpleWeapons().Contains(itemId) ||
            GetBlueWeapons().Contains(itemId)) return 2;

        return 0;
    }

    public async ValueTask<DoGachaScRsp?> DoGacha(int bannerId, int times)
    {
        if (times <= 0) return new DoGachaScRsp { Retcode = (uint)Retcode.RetGachaNumInvalid };
        if (times > 10) return new DoGachaScRsp { Retcode = (uint)Retcode.RetGachaNumInvalid };

        var banner = GameData.BannersConfig.Banners.Find(x => x.GachaId == bannerId);
        if (banner == null) return new DoGachaScRsp { Retcode = (uint)Retcode.RetGachaIdNotExist };

        var costItemId = banner.GachaType.GetCostItemId();
        var costItem = Player.InventoryManager?.GetItem(costItemId);
        if (costItem == null || costItem.Count < times)
            return new DoGachaScRsp { Retcode = (uint)Retcode.RetItemNotEnough, GachaId = (uint)bannerId, GachaNum = (uint)times };

        // pay cost (aggregate sync at the end)
        var syncItems = new List<ItemData>();
        var paid = await Player.InventoryManager!.RemoveItem(costItemId, times, sync: false);
        if (paid != null) syncItems.Add(paid);

        // Decide order can be empty in some data packs (e.g. DecideAvatarOrderExcel missing).
        // Fallback to the standard 5* avatar pool to avoid crashing on GetRange.
        var decideItem = GachaData.GachaDecideOrder.Take(7).ToList();
        if (decideItem.Count < 7)
        {
            foreach (var id in GetGoldAvatars())
            {
                if (decideItem.Count >= 7) break;
                if (!decideItem.Contains(id)) decideItem.Add(id);
            }

            if (decideItem.Count < 7)
                foreach (var id in GetAllGoldAvatars())
                {
                    if (decideItem.Count >= 7) break;
                    if (!decideItem.Contains(id)) decideItem.Add(id);
                }
        }

        if (GachaData.GachaDecideOrder.Count < decideItem.Count)
            GachaData.GachaDecideOrder = decideItem.ToList();
        var items = new List<int>();
        for (var i = 0; i < times; i++)
        {
            var item = banner.DoGacha(decideItem, GetPurpleAvatars(), GetPurpleWeapons(), GetGoldWeapons(),
                GetBlueWeapons(), GachaData);
            items.Add(item);
        }

        var gachaItems = new List<GachaItem>();
        // get rarity of item
        foreach (var item in items)
        {
            var dirt = 0;
            var star = 0;
            var rarity = GetRarity(item);

            GachaData.GachaHistory.Add(new GachaInfo
            {
                GachaId = bannerId,
                ItemId = item,
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
            var gachaItem = new GachaItem();
            if (rarity == 5)
            {
                var type = GetType(item);
                if (type == 1)
                {
                    var avatar = Player.AvatarManager?.GetFormalAvatar(item);
                    if (avatar != null)
                    {
                        star += 40;
                        var rankUpItemId = item + 10000;
                        var rankUpItem = Player.InventoryManager!.GetItem(rankUpItemId);
                        if (avatar.PathInfos[item].Rank + rankUpItem?.Count >= 6)
                        {
                            star += 60;
                        }
                        else
                        {
                            var dupeItem = new ItemList();
                            dupeItem.ItemList_.Add(new Item
                            {
                                ItemId = (uint)rankUpItemId,
                                Num = 1
                            });
                            gachaItem.TransferItemList = dupeItem;
                        }
                    }
                }
                else
                {
                    star += 20;
                }
            }
            else if (rarity == 4)
            {
                var type = GetType(item);
                if (type == 1)
                {
                    var avatar = Player.AvatarManager?.GetFormalAvatar(item);
                    if (avatar != null)
                    {
                        star += 8;
                        var rankUpItemId = item + 10000;
                        var rankUpItem = Player.InventoryManager!.GetItem(rankUpItemId);
                        if (avatar.PathInfos[item].Rank + rankUpItem?.Count >= 6)
                        {
                            star += 12;
                        }
                        else
                        {
                            var dupeItem = new ItemList();
                            dupeItem.ItemList_.Add(new Item
                            {
                                ItemId = (uint)rankUpItemId,
                                Num = 1
                            });
                            gachaItem.TransferItemList = dupeItem;
                        }
                    }
                }
                else
                {
                    star += 8;
                }
            }
            else
            {
                dirt += 20;
            }

            ItemData? i;
            if (GameData.ItemConfigData[item].ItemMainType == ItemMainTypeEnum.AvatarCard &&
                Player.AvatarManager!.GetFormalAvatar(item) == null)
            {
                i = null;
                await Player.AvatarManager!.AddAvatar(item, isGacha: true);
            }

            else
            {
                i = await Player.InventoryManager!.AddItem(item, 1, false, sync: false, returnRaw: true);
            }

            if (i != null) syncItems.Add(i);

            gachaItem.GachaItem_ = new Item
            {
                ItemId = (uint)item,
                Num = 1,
                Level = 1,
                Rank = 1
            };

            var tokenItem = new ItemList();
            if (dirt > 0)
            {
                var it = await Player.InventoryManager!.AddItem(251, dirt, false, sync: false, returnRaw: true);
                if (it != null)
                {
                    var oldItem = syncItems.Find(x => x.ItemId == 251);
                    if (oldItem == null)
                        syncItems.Add(it);
                    else
                        oldItem.Count = it.Count;
                }

                tokenItem.ItemList_.Add(new Item
                {
                    ItemId = 251,
                    Num = (uint)dirt
                });
            }

            if (star > 0)
            {
                var it = await Player.InventoryManager!.AddItem(252, star, false, sync: false, returnRaw: true);
                if (it != null)
                {
                    var oldItem = syncItems.Find(x => x.ItemId == 252);
                    if (oldItem == null)
                        syncItems.Add(it);
                    else
                        oldItem.Count = it.Count;
                }

                tokenItem.ItemList_.Add(new Item
                {
                    ItemId = 252,
                    Num = (uint)star
                });
            }

            gachaItem.TokenItem = tokenItem;

            gachaItem.TransferItemList ??= new ItemList();

            gachaItems.Add(gachaItem);
        }

        await Player.SendPacket(new PacketPlayerSyncScNotify(syncItems));
        var proto = new DoGachaScRsp
        {
            GachaId = (uint)bannerId,
            GachaNum = (uint)times
        };
        proto.GachaItemList.AddRange(gachaItems);

        DatabaseHelper.ToSaveUidList.SafeAdd(Player.Uid);

        return proto;
    }

    public GetGachaInfoScRsp ToProto()
    {
        var proto = new GetGachaInfoScRsp
        {
            Retcode = 0,
            GachaRandom = (uint)Random.Shared.Next(1000, 1999)
        };

        var purpleAvatars = GetPurpleAvatars();
        var goldAvatars = GetGoldAvatars();
        var purpleWeapons = GetPurpleWeapons();
        var goldWeapons = GetGoldWeapons();
        var defaultFeaturedIds = new List<int> { 23002, 1003, 1101, 1104, 23000, 23003 };

        var gachaIdsByCostItem = new Dictionary<int, HashSet<int>>();
        foreach (var banner in GameData.BannersConfig.Banners)
        {
            proto.GachaInfoList.Add(banner.ToInfo(purpleAvatars, goldAvatars, purpleWeapons, goldWeapons, defaultFeaturedIds));

            var costItemId = banner.GachaType.GetCostItemId();
            if (!gachaIdsByCostItem.TryGetValue(costItemId, out var set))
            {
                set = [];
                gachaIdsByCostItem[costItemId] = set;
            }
            set.Add(banner.GachaId);
        }

        foreach (var (costItemId, gachaIds) in gachaIdsByCostItem)
        {
            if (costItemId == 0) continue;

            var count = Player.InventoryManager?.GetItem(costItemId)?.Count ?? 0;
            var info = new JMDPCOPDNNH
            {
                HHKJCOLOKFF = (uint)costItemId,
                EJHKEFAIEBG = (uint)count
            };
            info.MPOFFMIELAF.AddRange(gachaIds.Select(x => (uint)x));
            proto.LLNLIGALCDC.Add(info);
        }

        return proto;
    }
}
