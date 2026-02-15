using HyacineCore.Server.Data;
using HyacineCore.Server.Data.Config;
using HyacineCore.Server.GameServer.Game.Player;
using HyacineCore.Server.GameServer.Game.Scene;
using HyacineCore.Server.GameServer.Game.Scene.Entity;
using HyacineCore.Server.Proto;

namespace HyacineCore.Server.GameServer.Game.Battle;

public class SceneSkillManager(PlayerInstance player) : BasePlayerManager(player)
{
    public async ValueTask<SkillResultData> OnCast(SceneCastSkillCsReq req)
    {
        // get entities (对齐 LC：客户端会在不同场景把“命中目标”放在不同字段里，这里取并集来保证能开战)
        var hitTargetIds = new HashSet<int>(); // real hit targets -> start battle
        var assistTargetIds = new HashSet<int>(); // around cast location (not actually hitting)
        var assistMonsterIds = new HashSet<int>(); // assist monsters (waves)

        foreach (var entityId in req.AssistMonsterEntityIdList)
        {
            var id = (int)entityId;
            hitTargetIds.Add(id);
            assistTargetIds.Add(id);
        }

        foreach (var assistWave in req.AssistMonsterEntityInfo)
        foreach (var entityId in assistWave.EntityIdList)
            assistMonsterIds.Add((int)entityId);

        foreach (var entityId in req.HitTargetEntityIdList)
        {
            var id = (int)entityId;
            assistTargetIds.Add(id);
            hitTargetIds.Add(id); // 某些版本/动作会把真实命中目标放在 hit_target_entity_id_list
        }

        var targetEntities = new List<BaseGameEntity>();
        foreach (var id in hitTargetIds)
            if (Player.SceneInstance!.Entities.TryGetValue(id, out var entity))
                targetEntities.Add(entity);

        var attackEntity = Player.SceneInstance!.Entities.GetValueOrDefault((int)req.AttackedByEntityId) ??
                           Player.SceneInstance!.Entities.GetValueOrDefault((int)req.CastEntityId);
        if (attackEntity == null) return new SkillResultData(Retcode.RetSceneEntityNotExist);
        // get ability file
        var abilities = GetAbilityConfig(attackEntity);
        if (abilities == null || abilities.AbilityList.Count < 1)
            return new SkillResultData(Retcode.RetMazeNoAbility);

        var abilityName = !string.IsNullOrEmpty(req.MazeAbilityStr) ? req.MazeAbilityStr :
            req.SkillIndex == 0 ? "NormalAtk01" : "MazeSkill";
        var targetAbility = abilities.AbilityList.Find(x => x.Name.Contains(abilityName));
        if (targetAbility == null)
        {
            targetAbility = abilities.AbilityList.FirstOrDefault();
            if (targetAbility == null)
                return new SkillResultData(Retcode.RetMazeNoAbility);
        }

        // execute ability
        var res = await Player.TaskManager!.AbilityLevelTask.TriggerTasks(abilities, targetAbility.OnStart,
            attackEntity, targetEntities, req);

        // check if avatar execute
        if (attackEntity is AvatarSceneInfo) await Player.SceneInstance!.OnUseSkill(req);

        var instance = res.Instance;
        var battleInfos = res.BattleInfos ?? [];

        if (instance == null && hitTargetIds.Count > 0)
        {
            instance = await Player.BattleManager!.StartBattle(attackEntity, targetEntities, req.SkillIndex == 1,
                req.MazeAbilityStr);
            if (instance != null && battleInfos.Count == 0)
            {
                foreach (var id in hitTargetIds)
                    battleInfos.Add(new HitMonsterInstance(id, MonsterBattleType.TriggerBattle));
            }
        }

        return new SkillResultData(Retcode.RetSucc, instance, battleInfos);
    }

    private AdventureAbilityConfigListInfo? GetAbilityConfig(BaseGameEntity entity)
    {
        if (entity is EntityMonster monster)
            return GameData.AdventureAbilityConfigListData.GetValueOrDefault(monster.MonsterData.ID);

        if (entity is AvatarSceneInfo avatar)
            if (GameData.AvatarConfigData.TryGetValue(avatar.AvatarInfo.AvatarId, out var excel))
                return GameData.AdventureAbilityConfigListData.GetValueOrDefault(excel.AdventurePlayerID);

        return null;
    }
}

public record SkillResultData(
    Retcode RetCode,
    BattleInstance? Instance = null,
    List<HitMonsterInstance>? TriggerBattleInfos = null);
