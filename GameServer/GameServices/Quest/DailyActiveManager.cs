using HyacineCore.Server.Database;
using HyacineCore.Server.Database.Quests;
using HyacineCore.Server.GameServer.Game.Player;
using HyacineCore.Server.GameServer.Server.Packet.Send.PlayerSync; 
using HyacineCore.Server.Proto;
using HyacineCore.Server.Util; 
using HyacineCore.Server.GameServer.Server.Packet.Send.Quest;
using HyacineCore.Server.Data; 
namespace HyacineCore.Server.GameServer.Game.Quest;

public class DailyActiveManager(PlayerInstance player) : BasePlayerManager(player)
{
    private static readonly Logger Log = Logger.GetByClassName();

    public DailyActiveData Data => 
        DatabaseHelper.Instance!.GetInstanceOrCreateNew<DailyActiveData>(Player.Uid);
    public async ValueTask SyncDailyActiveNotify()
    {
        var notify = new DailyActiveInfoNotify
        {
            DailyActivePoint = Data.DailyActivePoint
        };
    

    await Player.SendPacket(new PacketDailyActiveInfoNotify(notify));
   }
    public GetDailyActiveInfoScRsp GetDailyActiveInfo()
    {
        var dbData = Data;
        CheckAndResetDaily();

        var rsp = new GetDailyActiveInfoScRsp
        {
            Retcode = 0,
            DailyActivePoint = dbData.DailyActivePoint,
        };

        foreach (var info in dbData.TodayQuests.Values)
        {
            rsp.DailyActiveQuestIdList.Add(info.QuestId);
            rsp.DailyActiveLevelList.Add(info.ToProto((uint)Player.Data.WorldLevel));
        }

        return rsp;
    }

   private void CheckAndResetDaily()
{
    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();


    if (!UtilTools.IsSameDaily(Data.LastRefreshTime, now) || Data.TodayQuests.Count == 0)
    {
    Log.Info($"[DailyActiveManager] Last update time: {Data.LastRefreshTime}, Current time: {now}");

        Data.DailyActivePoint = 0;
        Data.TakenRewardList.Clear();
        Data.TodayQuests.Clear();

        var availablePool = GameData.DailyQuestConfigData.Values
            .Where(x => !x.IsDelete && 
                        Player.Data.Level >= x.MinLevel && 
                        Player.Data.Level <= x.MaxLevel)
            .ToList();

        if (availablePool.Count > 0)
        {
            var random = new Random();
            var selectedGroups = availablePool.OrderBy(x => random.Next()).Take(5).ToList();

            foreach (var group in selectedGroups)
            {
                foreach (var qId in group.QuestList)
                {
                    Data.TodayQuests[(uint)qId] = new DailyQuestInfo 
                    { 
                        QuestId = (uint)qId, 
                        Progress = 0, 
                        IsFinished = false 
                    };
                }
            }
        }

        Data.LastRefreshTime = now; 
        DatabaseHelper.ToSaveUidList.Add(Player.Uid);
    }
}

    public async ValueTask SyncDailyQuestsStatus()
    {
        var syncList = new List<QuestInfo>();
        foreach (var qId in Data.TodayQuests.Keys)
        {
            syncList.Add(new QuestInfo
            {
                QuestId = (int)qId,
                QuestStatus = QuestStatus.QuestDoing,
                Progress = 0
            });
        }
        await Player.SendPacket(new PacketPlayerSyncScNotify(syncList));
    }
}
