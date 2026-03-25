using System;
using System.Collections.Generic;
using Data.Runtime;
using Presentation.Unit;
using Sirenix.OdinInspector;
using Spine.Unity;
using Systems.AI.Config;
using Systems.Equipment;
using Systems.Equipment.Config;
using Systems.Turn;
using UnityEngine;

namespace Systems.Unit
{
	public enum EUnitFaction
	{
		Player, Enemy, Neutral,
		None, // 用于占位或特殊情况
	}

	[Serializable]
	public class Unit : ITurnUnit, IEquipable
	{
		[TitleGroup("Identity")]
		public string id;		// id for runtime instance
		public string configId;	// id for config so
		public string name;
		public string description;

		[TitleGroup("Config")]
		public int maxHp;
		public int speed;
		public int moveRange;
		public int maxAp;
        public int visionRange;
		public EUnitFaction faction;
        public int defense;
        public float defenseRate;
        public AIBrainConfig aiBrainConfig;

		[TitleGroup("Presentation")]
		public UnitAnimationConfig animationConfig;
		public SkeletonDataAsset skeletonDataAsset;
		public string frontBodySkin;
		public string backBodySkin;
		public string defaultWeaponSkin;
		public Sprite icon;

		[TitleGroup("Runtime")]
		public int currentHp;
		public Vector2Int position;
		public bool isStunned;
		public int currentAp;
        public int currentDefense;

		public bool IsAlive => currentHp > 0;
		public bool CanAct => IsAlive && !isStunned;
		public bool HasAp => currentAp > 0;
		
		internal static Unit LoadFromConfig(string unitId, UnitConfig config, Vector2Int startPosition)
		{
			return new Unit
			{
				id = unitId,
				configId = config.configId,
				name = config.unitName,
				description = config.description,

				maxHp = config.maxHp,
				speed = config.speed,
				moveRange = config.moveRange,
				maxAp = config.actionPoints,
                visionRange = config.visionRange,
				faction = config.faction,
                defense = config.defense,
                defenseRate = config.defenseRate,
                aiBrainConfig = config.aiBrainConfig,

				animationConfig = config.animationConfig,
				skeletonDataAsset = config.skeletonDataAsset,
				frontBodySkin = config.frontBodySkin,
				backBodySkin = config.backBodySkin,
				defaultWeaponSkin = config.defaultWeaponSkin,
				icon = config.icon,

				currentHp = config.maxHp,
                currentDefense = config.defense,
				position = startPosition,
				isStunned = false,
				currentAp = config.actionPoints
			};
		}

		public List<EActionType> GetAvailableActions()
		{
			var actions = new List<EActionType>();
            // todo: 计算攻击所需的AP
			if (HasAp)
			{
				actions.Add(EActionType.Move);
                if (!MainWeapon.IsNullOrEmpty())
                    actions.Add(EActionType.MainWeapon);
                if (!SecondaryWeapon.IsNullOrEmpty())
                    actions.Add(EActionType.SecondaryWeapon);
                if (!TacticalItem0.IsNullOrEmpty())
                    actions.Add(EActionType.TacticalItem0);
                if (!TacticalItem1.IsNullOrEmpty())
                    actions.Add(EActionType.TacticalItem1);
                if (!TacticalItem2.IsNullOrEmpty())
                    actions.Add(EActionType.TacticalItem2);
			}
			actions.Add(EActionType.Wait);
			return actions;
		}

		public int CalculateMovementApCost(int pathCost)
		{
			return moveRange <= 0
				? pathCost
				: Mathf.CeilToInt((float)pathCost / moveRange);
		}

		public bool IsHostile(Unit other)
		{
			if (other == null) return false;
			if (faction is EUnitFaction.None or EUnitFaction.Neutral || other.faction is EUnitFaction.None or EUnitFaction.Neutral)
				return false;
			return faction != other.faction;
		}

		public override string ToString() =>
			$"[Unit] {name}({id}) HP:{currentHp}/{maxHp} AP:{currentAp}/{maxAp} Pos:{position}";

		#region ITurnUnit

		string ITurnUnit.Id => id;
		int ITurnUnit.Speed => speed;
		bool ITurnUnit.CanAct => CanAct;
		public int ActionPriority { get; set; }
		void ITurnUnit.OnTurnStart() => currentAp = maxAp;

		#endregion

        #region IEquipable

        public EquipmentContainer MainWeapon { get; set; }
        public EquipmentContainer SecondaryWeapon { get; set; }
        public EquipmentContainer TacticalItem0 { get; set; }
        public EquipmentContainer TacticalItem1 { get; set; }
        public EquipmentContainer TacticalItem2 { get; set; }
        public List<EquipmentContainer> TacticalItemInfos => new()
        {
            TacticalItem0,
            TacticalItem1,
            TacticalItem2
        };

        public void InitEquipment(List<EquipmentConfig> equipmentConfigs)
        {
            MainWeapon = new EquipmentContainer();
            SecondaryWeapon = new EquipmentContainer();
            TacticalItem0 = new EquipmentContainer();
            TacticalItem1 = new EquipmentContainer();
            TacticalItem2 = new EquipmentContainer();
            
            MainWeapon.Init(equipmentConfigs[0]);
            SecondaryWeapon.Init(equipmentConfigs[1]);
            TacticalItem0.Init(equipmentConfigs[2]);
            TacticalItem1.Init(equipmentConfigs[3]);
            TacticalItem2.Init(equipmentConfigs[4]);
        }

        public EquipmentContainer GetEquipment(EActionType actionType)
        {
            switch (actionType)
            {
                case EActionType.MainWeapon:
                    return MainWeapon;
                case EActionType.SecondaryWeapon:
                    return SecondaryWeapon;
                case EActionType.TacticalItem0:
                    return TacticalItem0;
                case EActionType.TacticalItem1:
                    return TacticalItem0;
                case EActionType.TacticalItem2:
                    return TacticalItem0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }
        }
        
        #endregion
	}
}
