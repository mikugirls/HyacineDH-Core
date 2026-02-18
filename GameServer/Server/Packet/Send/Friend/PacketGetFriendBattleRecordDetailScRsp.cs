using HyacineCore.Server.Data;
using HyacineCore.Server.Database.Avatar;
using HyacineCore.Server.Database.Challenge;
using HyacineCore.Server.Database.Friend;
using HyacineCore.Server.Kcp;
using HyacineCore.Server.Proto;
using HyacineCore.Server.Util;

namespace HyacineCore.Server.GameServer.Server.Packet.Send.Friend;

public class PacketGetFriendBattleRecordDetailScRsp : BasePacket
{
    public PacketGetFriendBattleRecordDetailScRsp(FriendRecordData recordData, ChallengeData challengeData,
        AvatarData avatarData) : base(
        CmdIds.GetFriendBattleRecordDetailScRsp)
    {
        var proto = new GetFriendBattleRecordDetailScRsp
        {
            Uid = (uint)recordData.Uid,
            FMIKAEHPMAC = new IHJCHGKJJGD()
        };

        foreach (var group in recordData.ChallengeGroupStatistics.Values)
        {
            var maxBoss = group.BossGroupStatistics?.Values.MaxBy(x => x.Level);
            if (maxBoss != null)
            {
                proto.CICODDEKOPI.Add(new FriendChallengeClearanceInfo
                {
                    GroupId = group.GroupId,
                    ChallengeBoss = maxBoss.ToProto()
                });
            }

            var maxStory = group.StoryGroupStatistics?.Values.MaxBy(x => x.Level);
            if (maxStory != null)
            {
                proto.CICODDEKOPI.Add(new FriendChallengeClearanceInfo
                {
                    GroupId = group.GroupId,
                    ChallengeStory = maxStory.ToProto()
                });
            }

            var maxMemory = group.MemoryGroupStatistics?.Values.MaxBy(x => x.Level);
            if (maxMemory != null)
            {
                proto.CICODDEKOPI.Add(new FriendChallengeClearanceInfo
                {
                    GroupId = group.GroupId,
                    ChallengeDefault = maxMemory.ToProto()
                });
            }
        }

        var peakConfig = GameData.ChallengePeakGroupConfigData.GetValueOrDefault((int)GameConstants
            .CHALLENGE_PEAK_CUR_GROUP_ID);
        if (peakConfig != null)
        {
            var peakRec = new OBEJAHHMOOB
            {
                GroupId = GameConstants.CHALLENGE_PEAK_CUR_GROUP_ID
            };

            foreach (var preId in peakConfig.PreLevelIDList)
            {
                var rec = challengeData.PeakLevelDatas.GetValueOrDefault(preId);
                if (rec == null) continue;

                peakRec.GIHMKNCAEHD.Add(new PlayerChallengePeakRecordMobData
                {
                    PeakId = (uint)preId,
                    CyclesUsed = rec.RoundCnt,
                    Lineup = new ChallengeLineupList
                    {
                        AvatarList =
                        {
                            rec.BaseAvatarList.Select((x, index) => new ChallengeAvatarInfo
                            {
                                Index = (uint)index,
                                Id = x,
                                AvatarType = AvatarType.AvatarFormalType,
                                Level = (uint)(avatarData.FormalAvatars.Find(a => a.BaseAvatarId == x)?.Level ?? 1)
                            })
                        }
                    }
                });
            }

            var bossRec = challengeData.PeakBossLevelDatas.GetValueOrDefault((peakConfig.BossLevelID << 2) | 1);
            bossRec ??= challengeData.PeakBossLevelDatas.GetValueOrDefault((peakConfig.BossLevelID << 2) | 0);
            if (bossRec != null)
            {
                peakRec.DPEKNAKGCOH = new PlayerChallengePeakRecordBossData
                {
                    PeakId = (uint)bossRec.LevelId,
                    BuffId = bossRec.BuffId,
                    BestCycleCount = bossRec.RoundCnt,
                    Lineup = new ChallengeLineupList
                    {
                        AvatarList =
                        {
                            bossRec.BaseAvatarList.Select((x, index) => new ChallengeAvatarInfo
                            {
                                Index = (uint)index,
                                Id = x,
                                AvatarType = AvatarType.AvatarFormalType,
                                Level = (uint)(avatarData.FormalAvatars.Find(a => a.BaseAvatarId == x)?.Level ?? 1)
                            })
                        }
                    }
                };
            }

            proto.DBLCPPKMIGB.Add(peakRec);
        }

        SetData(proto);
    }

    public PacketGetFriendBattleRecordDetailScRsp(Retcode code) : base(CmdIds.GetFriendBattleRecordDetailScRsp)
    {
        var proto = new GetFriendBattleRecordDetailScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }
}
