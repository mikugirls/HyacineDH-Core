using HyacineCore.Server.Configuration;
using HyacineCore.Server.Data;

namespace HyacineCore.Server.Util;

public static class GameConstants
{
    public const string GAME_VERSION = "4.0.0";
    public const string AvatarDbVersion = "20250430";
    public const int GameVersionInt = 3200;
    public const int MAX_STAMINA = 300;
    public const int MAX_STAMINA_RESERVE = 2400;
    public const int STAMINA_RECOVERY_TIME = 360; // 6 minutes
    public const int STAMINA_RESERVE_RECOVERY_TIME = 1080; // 18 minutes
    public const int INVENTORY_MAX_EQUIPMENT = 1500;
    public const int INVENTORY_MAX_RELIC = 1500;
    public const int INVENTORY_MAX_MATERIAL = 2000;
    public const int MAX_LINEUP_COUNT = 9;
    public const int LAST_TRAIN_WORLD_ID = 501;
    public const int AMBUSH_BUFF_ID = 1000102;
    public const int CHALLENGE_ENTRANCE = 100000103;
    public const int CHALLENGE_PEAK_ENTRANCE = 100000352;
    public const int CHALLENGE_STORY_ENTRANCE = 102020107;
    public const int CHALLENGE_BOSS_ENTRANCE = 1030402;
    public const int CURRENT_ROGUE_TOURN_SEASON = 2;

    public static uint CHALLENGE_PEAK_BRONZE_FRAME_ID { get; private set; } = 226001;
    public static uint CHALLENGE_PEAK_SILVER_FRAME_ID { get; private set; } = 226002;
    public static uint CHALLENGE_PEAK_GOLD_FRAME_ID { get; private set; } = 226003;
    public static uint CHALLENGE_PEAK_ULTRA_FRAME_ID { get; private set; } = 226004;

    public static uint CHALLENGE_PEAK_CUR_GROUP_ID { get; private set; } = 1;
    public static Dictionary<uint, List<uint>> CHALLENGE_PEAK_TARGET_ENTRY_ID { get; private set; } =
        CreateDefaultChallengePeakTargetEntries();

    public static readonly List<int> UpgradeWorldLevel = [20, 30, 40, 50, 60, 65];
    public static readonly List<int> AllowedChessRogueEntranceId = [8020701, 8020901, 8020401, 8020201];

    private static Dictionary<uint, List<uint>> CreateDefaultChallengePeakTargetEntries()
    {
        return new Dictionary<uint, List<uint>>
        {
            {1, [3013501, 8]},
            {2, [3013701, 10]},
            {3, [3012302, 5]},
            {4, [3014001, 7]}
        };
    }

    public static void ApplyChallengePeakConfig(ChallengePeakOption? option)
    {
        if (option == null)
        {
            CHALLENGE_PEAK_BRONZE_FRAME_ID = 226001;
            CHALLENGE_PEAK_SILVER_FRAME_ID = 226002;
            CHALLENGE_PEAK_GOLD_FRAME_ID = 226003;
            CHALLENGE_PEAK_ULTRA_FRAME_ID = 226004;
            CHALLENGE_PEAK_CUR_GROUP_ID = 1;
            CHALLENGE_PEAK_TARGET_ENTRY_ID = CreateDefaultChallengePeakTargetEntries();
            return;
        }

        CHALLENGE_PEAK_BRONZE_FRAME_ID = option.BronzeFrameId > 0 ? option.BronzeFrameId : 226001;
        CHALLENGE_PEAK_SILVER_FRAME_ID = option.SilverFrameId > 0 ? option.SilverFrameId : 226002;
        CHALLENGE_PEAK_GOLD_FRAME_ID = option.GoldFrameId > 0 ? option.GoldFrameId : 226003;
        CHALLENGE_PEAK_ULTRA_FRAME_ID = option.UltraFrameId > 0 ? option.UltraFrameId : 226004;
        CHALLENGE_PEAK_CUR_GROUP_ID = option.CurrentGroupId > 0 ? option.CurrentGroupId : 1;

        var targetEntryByGroup = new Dictionary<uint, List<uint>>();
        var sourceMap = option.TargetEntryByGroup ?? CreateDefaultChallengePeakTargetEntries();
        foreach (var kv in sourceMap)
        {
            if (kv.Key == 0 || kv.Value.Count < 2) continue;

            var entryId = kv.Value[0];
            var mazeGroupId = kv.Value[1];
            if (entryId == 0 || mazeGroupId == 0) continue;

            targetEntryByGroup[kv.Key] = [entryId, mazeGroupId];
        }

        CHALLENGE_PEAK_TARGET_ENTRY_ID = targetEntryByGroup.Count > 0
            ? targetEntryByGroup
            : CreateDefaultChallengePeakTargetEntries();

        if (!CHALLENGE_PEAK_TARGET_ENTRY_ID.ContainsKey(CHALLENGE_PEAK_CUR_GROUP_ID))
            CHALLENGE_PEAK_CUR_GROUP_ID = CHALLENGE_PEAK_TARGET_ENTRY_ID.Keys.Min();
    }

    public static void RefreshChallengePeakTargetEntriesFromResource()
    {
        if (GameData.ChallengePeakGroupConfigData.Count == 0 || GameData.MapEntranceData.Count == 0)
            return;

        var autoMap = new Dictionary<uint, List<uint>>();
        foreach (var group in GameData.ChallengePeakGroupConfigData.Values.OrderBy(x => x.ID))
        {
            var mapEntranceId = group.MapEntranceID > 0 ? group.MapEntranceID : group.MapEntranceBoss;
            if (mapEntranceId <= 0) continue;
            if (!GameData.MapEntranceData.TryGetValue(mapEntranceId, out var entrance)) continue;
            if (entrance.StartGroupID <= 0) continue;

            autoMap[(uint)group.ID] = [(uint)mapEntranceId, (uint)entrance.StartGroupID];
        }

        if (autoMap.Count == 0) return;

        CHALLENGE_PEAK_TARGET_ENTRY_ID = autoMap;
        if (!CHALLENGE_PEAK_TARGET_ENTRY_ID.ContainsKey(CHALLENGE_PEAK_CUR_GROUP_ID))
            CHALLENGE_PEAK_CUR_GROUP_ID = CHALLENGE_PEAK_TARGET_ENTRY_ID.Keys.Min();
    }

    public static int ResolveChallengePeakGroupIdByLevel(int peakLevelId)
    {
        var group = GameData.ChallengePeakGroupConfigData.Values
            .FirstOrDefault(x => x.BossLevelID == peakLevelId || x.PreLevelIDList.Contains(peakLevelId));

        if (group != null) return group.ID;

        return (int)CHALLENGE_PEAK_CUR_GROUP_ID;
    }

    public static int ResolveChallengePeakEntryId(int groupId, bool isBossMode)
    {
        var group = GameData.ChallengePeakGroupConfigData.GetValueOrDefault(groupId);
        if (group != null)
        {
            var entranceId = isBossMode && group.MapEntranceBoss > 0 ? group.MapEntranceBoss : group.MapEntranceID;
            if (entranceId > 0) return entranceId;
        }

        if (CHALLENGE_PEAK_TARGET_ENTRY_ID.TryGetValue((uint)groupId, out var oldData) && oldData.Count > 0)
            return (int)oldData[0];

        return CHALLENGE_PEAK_ENTRANCE;
    }

    public static int ResolveChallengePeakStartGroupId(int groupId, bool isBossMode)
    {
        var entryId = ResolveChallengePeakEntryId(groupId, isBossMode);
        if (GameData.MapEntranceData.TryGetValue(entryId, out var entrance) && entrance.StartGroupID > 0)
            return entrance.StartGroupID;

        if (CHALLENGE_PEAK_TARGET_ENTRY_ID.TryGetValue((uint)groupId, out var oldData) && oldData.Count > 1)
            return (int)oldData[1];

        return 0;
    }
}
