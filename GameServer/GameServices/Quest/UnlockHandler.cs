using HyacineCore.Server.Data;
using HyacineCore.Server.Enums.Mission;
using HyacineCore.Server.Enums.Quest;
using HyacineCore.Server.GameServer.Game.Player;
using HyacineCore.Server.Util;

namespace HyacineCore.Server.GameServer.Game.Quest;

public class UnlockHandler(PlayerInstance player)
{
    public PlayerInstance Player { get; } = player;

    public bool GetUnlockStatus(int unlockId)
    {
        if (!ConfigManager.Config.ServerOption.EnableMission || !ConfigManager.Config.ServerOption.EnableQuest)
            return true;

        GameData.FuncUnlockDataData.TryGetValue(unlockId, out var unlockData);
        if (unlockData == null) return false;

        // judge
        foreach (var condition in unlockData.Conditions)
            switch (condition.Type)
            {
                case ConditionTypeEnum.WorldLevel:
                    if (Player.Data.WorldLevel < int.Parse(condition.Param)) return false; // less than it
                    break;
                case ConditionTypeEnum.FinishMainMission:
                    if (Player.MissionManager?.GetMainMissionStatus(int.Parse(condition.Param)) !=
                        MissionPhaseEnum.Finish) return false;
                    break;
                case ConditionTypeEnum.InStoryLine:
                    if (Player.StoryLineManager?.StoryLineData.CurStoryLineId != int.Parse(condition.Param))
                        return false;
                    break;
                case ConditionTypeEnum.PlayerLevel:
                    if (Player.Data.Level < int.Parse(condition.Param)) return false;
                    break;
                case ConditionTypeEnum.FinishSubMission:
                case ConditionTypeEnum.RealFinishSubMission:
                    if (Player.MissionManager?.GetSubMissionStatus(int.Parse(condition.Param)) !=
                        MissionPhaseEnum.Finish) return false;
                    break;
            }

        return true;
    }
}
