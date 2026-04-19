using System.Collections.Generic;
using Core.Log;
using Data.Config;
using Sirenix.OdinInspector;
using Systems.Buff.Config;
using Systems.Unit;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Data
{
    // 这是用于局内外数据管理和获取配置引用的类
    public class DataManager : MonoBehaviour
    {
	    [Title("装备配置")]
	    [SerializeField, AssetList(Path = "Data/Equipment")]
	    private List<EquipmentConfig> equipmentConfigs = new();

	    [Title("Buff 配置")]
	    [SerializeField]
	    private List<BuffData> buffs = new();

	    [Title("关卡配置")]
	    [SerializeField, AssetList(Path = "Data/Levels")]
	    private List<LevelConfig> levels = new();

	    [Title("玩家装备覆盖 (Runtime)")]
	    [ShowInInspector, ReadOnly, DictionaryDrawerSettings(KeyLabel = "configId", ValueLabel = "loadout")]
	    public Dictionary<string, Loadout> PlayerLoadouts { get; } = new();

	    [Title("当前选中关卡 (Runtime)")]
	    [ShowInInspector, ReadOnly]
	    public LevelConfig SelectedLevel { get; set; }

	    public IReadOnlyList<LevelConfig> Levels => levels;

	    private Dictionary<int, EquipmentConfig> _equipmentLookup;
	    private Dictionary<BuffType, BuffData> _buffLookup;
	    private bool _isInitialized;

	    #region Initialization

	    private void Awake() => EnsureInitialized();

	    private void EnsureInitialized()
	    {
		    if (_isInitialized) return;
		    BuildEquipmentLookup();
		    BuildBuffLookup();
		    _isInitialized = true;
	    }

	    private void BuildEquipmentLookup()
	    {
		    _equipmentLookup = new Dictionary<int, EquipmentConfig>(equipmentConfigs.Count);
		    foreach (var config in equipmentConfigs)
		    {
			    if (!config)
			    {
				    this.LogError("equipmentConfigs 中存在空引用，跳过");
				    continue;
			    }
			    if (config.id <= 0)
			    {
				    this.LogError($"装备 '{config.name}' 的 Id={config.id} 非法，必须 > 0（Id=0 保留给空槽）");
				    continue;
			    }
			    if (_equipmentLookup.TryGetValue(config.id, out var equipmentConfig))
			    {
				    this.LogError($"装备 Id={config.id} 重复：'{equipmentConfig.name}' 与 '{config.name}'");
				    continue;
			    }
			    _equipmentLookup[config.id] = config;
		    }
		    this.Log($"装备字典构建完成：{_equipmentLookup.Count} 项");
	    }

	    private void BuildBuffLookup()
	    {
		    _buffLookup = new Dictionary<BuffType, BuffData>(buffs.Count);
		    foreach (var buff in buffs)
		    {
			    if (!buff)
			    {
				    this.LogError("buffs 中存在空引用，跳过");
				    continue;
			    }
			    if (buff.buffType == BuffType.None)
			    {
				    this.LogError($"Buff '{buff.name}' 的 buffType=None，未设置有效类型");
				    continue;
			    }
			    if (_buffLookup.TryGetValue(buff.buffType, out var buffData))
			    {
				    this.LogError($"Buff 类型 {buff.buffType} 重复：'{buffData.name}' 与 '{buff.name}'");
				    continue;
			    }
			    _buffLookup[buff.buffType] = buff;
		    }
		    this.Log($"Buff 字典构建完成：{_buffLookup.Count} 项");
	    }

	    #endregion

	    public EquipmentConfig GetEquipment(int id)
	    {
		    EnsureInitialized();
		    if (id <= 0) return null;

		    if (_equipmentLookup.TryGetValue(id, out var config))
			    return config;

		    this.LogError($"GetEquipment: 找不到 Id={id} 的装备配置");
		    return null;
	    }

	    public BuffData GetBuffData(BuffType buffType)
	    {
		    EnsureInitialized();
		    if (buffType == BuffType.None) return null;

		    if (_buffLookup.TryGetValue(buffType, out var data))
			    return data;

		    this.LogError($"GetBuffData: 找不到类型 {buffType} 的 Buff 配置");
		    return null;
	    }

	    // 为 UnitPlacement 解析最终使用的 Loadout
	    // 玩家单位 > UnitPlacement 初始配置 > null
	    public Loadout GetLoadoutFor(UnitPlacement placement)
	    {
		    if (placement == null || !placement.unitConfig)
			    return null;

		    if (placement.unitConfig.faction == EUnitFaction.Player &&
		        PlayerLoadouts.TryGetValue(placement.unitConfig.configId, out var custom))
			    return custom;

		    return placement.initialLoadout;
	    }
    }
}
