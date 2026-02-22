using HyacineCore.Server.Database.Gacha;
using HyacineCore.Server.Enums;
using HyacineCore.Server.Proto;
using GachaInfo = HyacineCore.Server.Proto.GachaInfo;

namespace HyacineCore.Server.Data.Custom;

public class BannersConfig
{
    public List<BannerConfig> Banners { get; set; } = [];
}

public class BannerConfig
{
    public int GachaId { get; set; }
    public long BeginTime { get; set; }
    public long EndTime { get; set; }
    public GachaTypeEnum GachaType { get; set; }
    public List<int> RateUpItems5 { get; set; } = [];
    public List<int> RateUpItems4 { get; set; } = [];
    public int GetRateUpItem5Chance { get; set; } = 6; // 0.6% chance (0.6%*1000)
    public int MaxCount { get; set; } = 90;
    public int EventChance { get; set; } = 50;

    public int DoGacha(List<int> goldAvatars, List<int> purpleAvatars, List<int> purpleWeapons, List<int> goldWeapons,
        List<int> blueWeapons, GachaData data)
    {
        var random = new Random();
        data.LastGachaFailedCount += 1;
        int item;

        var allGoldItems = new List<int>();
        allGoldItems.AddRange(goldAvatars);
        allGoldItems.AddRange(goldWeapons);

        var allNormalItems = new List<int>();
        allNormalItems.AddRange(purpleAvatars);
        allNormalItems.AddRange(purpleWeapons);

        if (data.LastGachaFailedCount >= MaxCount || IsRateUp())
        {
            data.LastGachaFailedCount = 0;
            if (GachaType is GachaTypeEnum.Normal or GachaTypeEnum.Newbie)
            {
                item = allGoldItems[random.Next(0, allGoldItems.Count)];
            }
            else if (GachaType == GachaTypeEnum.WeaponUp)
            {
                item = GetRateUpItem5(goldWeapons, data.LastWeaponGachaFailed);
                if (RateUpItems5.Contains(item))
                    data.LastWeaponGachaFailed = false;
                else
                    data.LastWeaponGachaFailed = true;
            }
            else if (GachaType == GachaTypeEnum.AvatarUp)
            {
                item = GetRateUpItem5(goldAvatars, data.LastAvatarGachaFailed);
                if (RateUpItems5.Contains(item))
                    data.LastAvatarGachaFailed = false;
                else
                    data.LastAvatarGachaFailed = true;
            }
            else
            {
                item = GetRateUpItem5(allGoldItems, false);
            }
        }
        else
        {
            // 4 - 3 star
            if (IsRate4() || data.LastGachaPurpleFailedCount >= 10)
            {
                if (GachaType is GachaTypeEnum.Normal or GachaTypeEnum.Newbie)
                {
                    item = allNormalItems[random.Next(0, allNormalItems.Count)];
                }
                else if (IsRateUp4() && RateUpItems4.Count > 0)
                {
                    item = RateUpItems4[random.Next(0, RateUpItems4.Count)];
                }
                else
                {
                    item = allNormalItems[random.Next(0, allNormalItems.Count)];
                }
                data.LastGachaPurpleFailedCount = 0;
            }
            else
            {
                item = blueWeapons[random.Next(0, blueWeapons.Count)];
                data.LastGachaPurpleFailedCount += 1;
            }
        }

        return item;
    }

    public bool IsEvent()
    {
        var random = new Random();
        return random.Next(0, 100) < EventChance;
    }

    public bool IsRateUp()
    {
        var random = new Random();
        return random.Next(0, 1000) < GetRateUpItem5Chance;
    }

    public bool IsRateUp4()
    {
        var random = new Random();
        return random.Next(0, 100) < 50;
    }

    public bool IsRate4()
    {
        var random = new Random();
        return random.Next(0, 100) < 10;
    }

    public int GetRateUpItem5(List<int> gold, bool forceUp)
    {
        var random = new Random();
        if ((IsEvent() || forceUp) && RateUpItems5.Count > 0)
            return RateUpItems5[random.Next(0, RateUpItems5.Count)];
        return gold[random.Next(0, gold.Count)];
    }

    public GachaInfo ToInfo(
        List<int> purpleAvatars,
        List<int> goldAvatars,
        List<int> purpleWeapons,
        List<int> goldWeapons,
        List<int> defaultFeaturedIds)
    {
        var info = new GachaInfo
        {
            GachaId = (uint)GachaId,
            MHMENMHKACL = (uint)GachaType.GetCostItemId(),
            DetailUrl = "",
            HistoryUrl = ""
        };

        if (GachaType != GachaTypeEnum.Normal)
        {
            info.BeginTime = BeginTime;
            info.EndTime = EndTime == 0 ? 2524608000 : EndTime;
        }

        if (GachaId == 1001)
        {
            info.GachaCeiling = new GachaCeiling
            {
                IsClaimed = false,
                CeilingNum = 0,
                AvatarList = { goldAvatars.Select(id => new GachaCeilingAvatar { AvatarId = (uint)id, RepeatedCnt = 300 }) }
            };

            info.ItemDetailList.AddRange(purpleAvatars.Select(id => (uint)id));
            info.ItemDetailList.AddRange(goldAvatars.Select(id => (uint)id));
            info.ItemDetailList.AddRange(purpleWeapons.Select(id => (uint)id));
            info.ItemDetailList.AddRange(goldWeapons.Select(id => (uint)id));

            info.PrizeItemList.AddRange(defaultFeaturedIds.Select(id => (uint)id));
        }
        else
        {
            if (RateUpItems4.Count > 0) info.ItemDetailList.AddRange(RateUpItems4.Select(id => (uint)id));

            if (RateUpItems5.Count > 0)
            {
                info.PrizeItemList.AddRange(RateUpItems5.Select(id => (uint)id));
                info.ItemDetailList.AddRange(RateUpItems5.Select(id => (uint)id));
            }

            info.GachaCeiling = new GachaCeiling
            {
                IsClaimed = true,
                CeilingNum = 0
            };
        }

        return info;
    }
}
