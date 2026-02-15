using System.Text.Json;
using System.Text.Json.Serialization;
using HyacineCore.Server.Data;
using HyacineCore.Server.Database;
using HyacineCore.Server.Database.Avatar;
using HyacineCore.Server.Database.Inventory;
using HyacineCore.Server.Enums.Item;
using HyacineCore.Server.GameServer.Server.Packet.Send.PlayerSync;
using HyacineCore.Server.Internationalization;
using HyacineCore.Server.Util;

namespace HyacineCore.Server.Command.Command.Cmd;

[CommandInfo("json", "Game.Command.Json.Desc", "/json [路径/数字/clear] - 从 freesr-data.json 导入角色/光锥/遗器数据")]
public class CommandJson : ICommand
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static IEnumerable<DirectoryInfo> GetFreeSrDataDirectories()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<DirectoryInfo> AddCandidateBase(string? baseDir)
        {
            if (string.IsNullOrWhiteSpace(baseDir)) yield break;
            var cur = new DirectoryInfo(baseDir);
            for (var i = 0; i < 8 && cur.Exists; i++)
            {
                var candidate = new DirectoryInfo(Path.Combine(cur.FullName, "freesr-data"));
                if (candidate.Exists && seen.Add(candidate.FullName))
                    yield return candidate;

                cur = cur.Parent;
                if (cur == null) break;
            }
        }

        foreach (var d in AddCandidateBase(Environment.CurrentDirectory)) yield return d;
        foreach (var d in AddCandidateBase(AppContext.BaseDirectory)) yield return d;
    }

    [CommandDefault]
    public async ValueTask Import(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var input = (arg.Raw ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            await ShowFileList(arg);
            return;
        }

        if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            var (changedAvatars, removedItems) = await ClearRelicAndEquipment(player);
            if (changedAvatars.Count > 0)
                await player.SendPacket(new PacketPlayerSyncScNotify(changedAvatars));
            if (removedItems.Count > 0)
                await player.SendPacket(new PacketPlayerSyncScNotify(removedItems));

            DatabaseHelper.ToSaveUidList.SafeAdd(player.Uid);
            await arg.SendMsg("已清空玩家库存中的光锥和遗器");
            return;
        }

        var selectedPath = ResolveInputPath(input, out var pathError);
        if (selectedPath == null)
        {
            if (!string.IsNullOrWhiteSpace(pathError))
                await arg.SendMsg(pathError);
            return;
        }

        if (!File.Exists(selectedPath))
        {
            await arg.SendMsg($"未找到文件：{selectedPath}");
            return;
        }

        FreeSrData? data;
        try
        {
            var json = await File.ReadAllTextAsync(selectedPath);
            data = JsonSerializer.Deserialize<FreeSrData>(json, JsonOptions);
        }
        catch (Exception e)
        {
            await arg.SendMsg($"读取或解析 JSON 失败：{e.Message}");
            return;
        }

        if (data == null)
        {
            await arg.SendMsg("JSON 内容为空或格式不正确");
            return;
        }

        var (clearedAvatars, clearedItems) = await ClearRelicAndEquipment(player);
        if (clearedAvatars.Count > 0)
            await player.SendPacket(new PacketPlayerSyncScNotify(clearedAvatars));
        if (clearedItems.Count > 0)
            await player.SendPacket(new PacketPlayerSyncScNotify(clearedItems));

        var avatarChanged = await ImportAvatars(player, data, arg);
        var importedItems = await ImportRelicsAndLightcones(player, data, avatarChanged);

        if (importedItems.Count > 0)
            await player.SendPacket(new PacketPlayerSyncScNotify(importedItems));
        if (avatarChanged.Count > 0)
            await player.SendPacket(new PacketPlayerSyncScNotify(avatarChanged));

        DatabaseHelper.ToSaveUidList.SafeAdd(player.Uid);

        await arg.SendMsg(
            $"已从 {Path.GetFileName(selectedPath)} 导入：avatar={data.Avatars?.Count ?? 0} relic={data.Relics?.Count ?? 0} lightcone={data.Lightcones?.Count ?? 0}");
    }

    private static string? ResolveInputPath(string input, out string? error)
    {
        error = null;
        input = input.Trim();
        if (input.Length >= 2 && input.StartsWith('"') && input.EndsWith('"'))
            input = input[1..^1];

        if (int.TryParse(input, out var choice))
        {
            var files = GetFreeSrDataFiles().OrderBy(f => f.LastWriteTimeUtc).ToList();
            if (files.Count == 0)
            {
                error = "freesr-data 文件夹中未找到含有 freesr-data 的 JSON 文件（提示：默认会在工作目录/程序目录向上查找 freesr-data 文件夹；也可用 /json [绝对路径]）";
                return null;
            }

            if (choice < 1 || choice > files.Count)
            {
                error = $"无效的选择，请输入 1-{files.Count} 之间的数字";
                return null;
            }

            return files[choice - 1].FullName;
        }

        var looksLikePath = input.Contains('/') || input.Contains('\\') || Path.IsPathRooted(input);
        if (looksLikePath)
            return Path.GetFullPath(input);

        // Treat as filename under freesr-data folders
        var fileName = input.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? input : input + ".json";
        foreach (var dir in GetFreeSrDataDirectories())
        {
            var candidate = Path.Combine(dir.FullName, input);
            if (File.Exists(candidate)) return candidate;
            candidate = Path.Combine(dir.FullName, fileName);
            if (File.Exists(candidate)) return candidate;
        }

        // Fallback to cwd-relative for old behavior
        return Path.GetFullPath(Path.Combine("freesr-data", input));
    }

    private static List<FileInfo> GetFreeSrDataFiles()
    {
        var files = new List<FileInfo>();
        foreach (var dir in GetFreeSrDataDirectories())
        {
            try
            {
                files.AddRange(dir.GetFiles("*.json", SearchOption.TopDirectoryOnly)
                    .Where(f => f.Name.Contains("freesr-data", StringComparison.OrdinalIgnoreCase)));
            }
            catch
            {
                // ignore
            }
        }

        // de-dup by full path
        return files
            .GroupBy(f => f.FullName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    private static async ValueTask ShowFileList(CommandArg arg)
    {
        var files = GetFreeSrDataFiles().OrderBy(f => f.LastWriteTimeUtc).ToList();
        if (files.Count == 0)
        {
            var searched = GetFreeSrDataDirectories().Select(d => d.FullName).Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            await arg.SendMsg("freesr-data 文件夹中未找到含有 freesr-data 的 JSON 文件");
            if (searched.Count > 0)
            {
                await arg.SendMsg("已搜索以下目录：");
                foreach (var s in searched) await arg.SendMsg($"- {s}");
            }
            return;
        }

        await arg.SendMsg("在 freesr-data 文件夹中找到以下文件：");
        for (var i = 0; i < files.Count; i++)
            await arg.SendMsg($"{i + 1}. {files[i].Name}");
        await arg.SendMsg("使用 /json [数字] 选择文件，或 /json [路径] 指定自定义路径");
    }

    private static async ValueTask<(List<FormalAvatarInfo> changedAvatars, List<ItemData> removedItems)>
        ClearRelicAndEquipment(HyacineCore.Server.GameServer.Game.Player.PlayerInstance player)
    {
        var changed = new Dictionary<int, FormalAvatarInfo>();

        void MarkChanged(FormalAvatarInfo avatar)
        {
            if (!changed.ContainsKey(avatar.AvatarId))
                changed.Add(avatar.AvatarId, avatar);
        }

        var inv = player.InventoryManager!.Data;

        foreach (var item in inv.EquipmentItems)
        {
            if (item.EquipAvatar <= 0) continue;
            var avatar = player.AvatarManager?.GetFormalAvatar(item.EquipAvatar);
            if (avatar == null) continue;
            var pathInfo = avatar.PathInfos.GetValueOrDefault(item.EquipAvatar)
                           ?? avatar.PathInfos.Values.FirstOrDefault(x => x.EquipId == item.UniqueId);
            if (pathInfo != null && pathInfo.EquipId == item.UniqueId)
                pathInfo.EquipId = 0;
            item.EquipAvatar = 0;
            MarkChanged(avatar);
        }

        foreach (var item in inv.RelicItems)
        {
            if (item.EquipAvatar <= 0) continue;
            var avatar = player.AvatarManager?.GetFormalAvatar(item.EquipAvatar);
            if (avatar == null) continue;
            var pathInfo = avatar.PathInfos.GetValueOrDefault(item.EquipAvatar)
                           ?? avatar.PathInfos.Values.FirstOrDefault(x => x.Relic.Values.Contains(item.UniqueId));
            if (pathInfo != null)
            {
                var toRemoveSlots = pathInfo.Relic.Where(kv => kv.Value == item.UniqueId).Select(kv => kv.Key).ToList();
                foreach (var slot in toRemoveSlots) pathInfo.Relic.Remove(slot);
            }

            item.EquipAvatar = 0;
            MarkChanged(avatar);
        }

        var toRemove = new List<(int itemId, int count, int uniqueId)>(inv.EquipmentItems.Count + inv.RelicItems.Count);
        toRemove.AddRange(inv.EquipmentItems.Select(x => (x.ItemId, 1, x.UniqueId)));
        toRemove.AddRange(inv.RelicItems.Select(x => (x.ItemId, 1, x.UniqueId)));

        // Remove without syncing; caller sends a single batch sync.
        var removed = await player.InventoryManager.RemoveItems(toRemove, sync: false);

        return ([.. changed.Values], removed);
    }

    private static async ValueTask<List<FormalAvatarInfo>> ImportAvatars(
        HyacineCore.Server.GameServer.Game.Player.PlayerInstance player,
        FreeSrData data,
        CommandArg arg)
    {
        var changed = new Dictionary<int, FormalAvatarInfo>();

        if (data.Avatars == null || data.Avatars.Count == 0) return [];

        foreach (var (avatarKey, avatarJson) in data.Avatars)
        {
            var avatarId = avatarJson.AvatarId > 0 ? avatarJson.AvatarId : avatarKey;
            var baseAvatarId = GameData.MultiplePathAvatarConfigData.TryGetValue(avatarId, out var multiplePath)
                ? multiplePath.BaseAvatarID
                : avatarId;

            if (!GameData.AvatarConfigData.ContainsKey(avatarId))
            {
                await arg.SendMsg($"未找到角色 Excel：{avatarId}");
                continue;
            }

            if (player.AvatarManager?.GetFormalAvatar(baseAvatarId) == null)
            {
                await player.InventoryManager!.AddItem(baseAvatarId, 1, notify: false, sync: false);
            }

            var avatar = player.AvatarManager?.GetFormalAvatar(baseAvatarId);
            if (avatar == null) continue;
            if (!avatar.PathInfos.ContainsKey(avatarId))
            {
                avatar.PathInfos[avatarId] = new PathInfo(avatarId);
                avatar.PathInfos[avatarId].GetSkillTree();
            }

            avatar.Level = Math.Clamp(avatarJson.Level, 1, 80);
            avatar.Promotion = avatarJson.Promotion > 0
                ? Math.Clamp(avatarJson.Promotion, 0, 6)
                : GameData.GetMinPromotionForLevel(avatar.Level);

            var pathInfo = avatar.PathInfos[avatarId];
            pathInfo.Rank = Math.Clamp(avatarJson.Data?.Rank ?? 0, 0, 6);

            // skills: pointId -> level
            if (avatarJson.Data?.Skills != null)
            {
                var skillTree = pathInfo.GetSkillTree();
                skillTree.Clear();
                foreach (var (pointId, level) in avatarJson.Data.Skills)
                    skillTree[pointId] = Math.Max(1, level);
            }

            changed[avatar.BaseAvatarId] = avatar;
        }

        return [.. changed.Values];
    }

    private static async ValueTask<List<ItemData>> ImportRelicsAndLightcones(
        HyacineCore.Server.GameServer.Game.Player.PlayerInstance player,
        FreeSrData data,
        List<FormalAvatarInfo> avatarChanged)
    {
        var importedItems = new List<ItemData>(Math.Max(16, (data.Relics?.Count ?? 0) + (data.Lightcones?.Count ?? 0)));
        var avatarChangedMap = avatarChanged.ToDictionary(x => x.BaseAvatarId, x => x);

        FormalAvatarInfo? GetAvatar(int pathOrBaseAvatarId)
        {
            var baseAvatarId = GameData.MultiplePathAvatarConfigData.TryGetValue(pathOrBaseAvatarId, out var multiPath)
                ? multiPath.BaseAvatarID
                : pathOrBaseAvatarId;

            if (avatarChangedMap.TryGetValue(baseAvatarId, out var existing)) return existing;
            var avatar = player.AvatarManager?.GetFormalAvatar(baseAvatarId);
            if (avatar == null) return null;
            avatarChangedMap[baseAvatarId] = avatar;
            return avatar;
        }

        void EnsurePath(FormalAvatarInfo avatar, int avatarId)
        {
            if (!avatar.PathInfos.ContainsKey(avatarId))
            {
                avatar.PathInfos[avatarId] = new PathInfo(avatarId);
                avatar.PathInfos[avatarId].GetSkillTree();
            }
        }

        if (data.Relics != null)
        {
            foreach (var relic in data.Relics)
            {
                if (!GameData.RelicConfigData.TryGetValue(relic.RelicId, out var relicConfig)) continue;
                if (!GameData.ItemConfigData.TryGetValue(relic.RelicId, out var itemConfig) ||
                    itemConfig.ItemMainType != ItemMainTypeEnum.Relic)
                    continue;
                if (!GameData.RelicMainAffixData.TryGetValue(relicConfig.MainAffixGroup, out var mainAffixGroup) ||
                    mainAffixGroup.Count == 0)
                    continue;

                var subAffixes = new List<ItemSubAffix>(relic.SubAffixes?.Count ?? 0);
                if (relic.SubAffixes != null &&
                    GameData.RelicSubAffixData.TryGetValue(relicConfig.SubAffixGroup, out var subGroup) &&
                    subGroup != null)
                    foreach (var sub in relic.SubAffixes)
                    {
                        if (!subGroup.ContainsKey(sub.SubAffixId)) continue;
                        subAffixes.Add(new ItemSubAffix
                        {
                            Id = sub.SubAffixId,
                            Count = Math.Max(1, sub.Count),
                            Step = Math.Max(0, sub.Step)
                        });
                    }

                var mainAffixId = mainAffixGroup.ContainsKey(relic.MainAffixId)
                    ? relic.MainAffixId
                    : mainAffixGroup.Keys.First();

                var item = await player.InventoryManager!.PutItem(
                    relic.RelicId,
                    1,
                    level: Math.Clamp(relic.Level, 0, relicConfig.MaxLevel),
                    mainAffix: mainAffixId,
                    subAffixes: subAffixes,
                    uniqueId: ++player.InventoryManager.Data.NextUniqueId);

                importedItems.Add(item);

                if (relic.EquipAvatar > 0)
                {
                    var targetPathId = relic.EquipAvatar;
                    if (!GameData.AvatarConfigData.ContainsKey(targetPathId)) continue;

                    var avatar = GetAvatar(targetPathId);
                    if (avatar == null) continue;

                    EnsurePath(avatar, targetPathId);
                    var slot = (int)relicConfig.Type;
                    avatar.PathInfos[targetPathId].Relic[slot] = item.UniqueId;
                    item.EquipAvatar = targetPathId;
                }
            }
        }

        if (data.Lightcones != null)
        {
            foreach (var lightcone in data.Lightcones)
            {
                if (!GameData.ItemConfigData.TryGetValue(lightcone.ItemId, out var itemConfig) ||
                    itemConfig.ItemMainType != ItemMainTypeEnum.Equipment)
                    continue;
                if (!GameData.EquipmentConfigData.TryGetValue(lightcone.ItemId, out var equipmentConfig))
                    continue;

                var item = await player.InventoryManager!.PutItem(
                    lightcone.ItemId,
                    1,
                    rank: Math.Clamp(lightcone.Rank, 1, Math.Max(1, equipmentConfig.MaxRank)),
                    promotion: Math.Clamp(lightcone.Promotion, 0, Math.Max(0, equipmentConfig.MaxPromotion)),
                    level: Math.Clamp(lightcone.Level, 1, 80),
                    uniqueId: ++player.InventoryManager.Data.NextUniqueId);

                importedItems.Add(item);

                if (lightcone.EquipAvatar > 0)
                {
                    var targetPathId = lightcone.EquipAvatar;
                    if (!GameData.AvatarConfigData.ContainsKey(targetPathId)) continue;

                    var avatar = GetAvatar(targetPathId);
                    if (avatar == null) continue;
                    EnsurePath(avatar, targetPathId);
                    avatar.PathInfos[targetPathId].EquipId = item.UniqueId;
                    item.EquipAvatar = targetPathId;
                }
            }
        }

        // refresh caller list
        avatarChanged.Clear();
        avatarChanged.AddRange(avatarChangedMap.Values);

        return importedItems;
    }

    private sealed class FreeSrData
    {
        [JsonPropertyName("relics")] public List<RelicJson>? Relics { get; set; }
        [JsonPropertyName("lightcones")] public List<LightconeJson>? Lightcones { get; set; }
        [JsonPropertyName("avatars")] public Dictionary<int, AvatarJson>? Avatars { get; set; }
    }

    private sealed class RelicJson
    {
        [JsonPropertyName("level")] public int Level { get; set; }
        [JsonPropertyName("relic_id")] public int RelicId { get; set; }
        [JsonPropertyName("main_affix_id")] public int MainAffixId { get; set; }
        [JsonPropertyName("equip_avatar")] public int EquipAvatar { get; set; }
        [JsonPropertyName("sub_affixes")] public List<RelicSubAffixJson>? SubAffixes { get; set; }
    }

    private sealed class RelicSubAffixJson
    {
        [JsonPropertyName("sub_affix_id")] public int SubAffixId { get; set; }
        [JsonPropertyName("count")] public int Count { get; set; }
        [JsonPropertyName("step")] public int Step { get; set; }
    }

    private sealed class LightconeJson
    {
        [JsonPropertyName("level")] public int Level { get; set; }
        [JsonPropertyName("equip_avatar")] public int EquipAvatar { get; set; }
        [JsonPropertyName("item_id")] public int ItemId { get; set; }
        [JsonPropertyName("rank")] public int Rank { get; set; }
        [JsonPropertyName("promotion")] public int Promotion { get; set; }
    }

    private sealed class AvatarJson
    {
        [JsonPropertyName("avatar_id")] public int AvatarId { get; set; }
        [JsonPropertyName("level")] public int Level { get; set; }
        [JsonPropertyName("promotion")] public int Promotion { get; set; }
        [JsonPropertyName("data")] public AvatarExtraJson? Data { get; set; }
    }

    private sealed class AvatarExtraJson
    {
        [JsonPropertyName("rank")] public int Rank { get; set; }
        [JsonPropertyName("skills")] public Dictionary<int, int>? Skills { get; set; }
    }
}
