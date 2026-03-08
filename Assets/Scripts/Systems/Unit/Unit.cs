using System;
using System.Collections.Generic;
using Data.Runtime;
using Presentation.Unit;
using Sirenix.OdinInspector;
using Spine.Unity;
using Systems.Equipment;
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
        public int attackRange;
		public EUnitFaction faction;

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
                attackRange = config.attackRange,
				faction = config.faction,

				animationConfig = config.animationConfig,
				skeletonDataAsset = config.skeletonDataAsset,
				frontBodySkin = config.frontBodySkin,
				backBodySkin = config.backBodySkin,
				defaultWeaponSkin = config.defaultWeaponSkin,
				icon = config.icon,

				currentHp = config.maxHp,
				position = startPosition,
				isStunned = false,
				currentAp = config.actionPoints
			};
		}

		public List<EActionType> GetAvailableActions()
		{
			var actions = new List<EActionType>();
			if (HasAp)
			{
				actions.Add(EActionType.Move);
				actions.Add(EActionType.Attack);
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

        // todo:武器道具初始化
        public WeaponInfo MainWeapon { get; set; }
        public WeaponInfo SecondaryWeapon { get; set; }
        public TacticalItemInfo TacticalItemInfo0 { get; set; }
        public TacticalItemInfo TacticalItemInfo1 { get; set; }
        public TacticalItemInfo TacticalItemInfo2 { get; set; }
        public List<TacticalItemInfo> TacticalItemInfos => new()
        {
            TacticalItemInfo0,
            TacticalItemInfo1,
            TacticalItemInfo2
        };

        #endregion
	}
}
