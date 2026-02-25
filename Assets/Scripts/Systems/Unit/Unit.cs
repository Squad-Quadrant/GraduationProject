using System;
using System.Collections.Generic;
using Data.Config;
using Data.Runtime;
using Sirenix.OdinInspector;
using Spine.Unity;
using Systems.Equipment;
using Systems.Turn;
using UnityEngine;

namespace Systems.Unit
{
	[Serializable]
	public class Unit : ITurnUnit, IEquipable
	{
		[ReadOnly] public string id;		// id for runtime instance
		[ReadOnly] public string configId;	// id for config so
		[ReadOnly] public string name;
		[ReadOnly] public UnitStats stats = new();
		[ReadOnly] public UnitRuntime runtime = new();

		[ReadOnly] public UnitAnimationConfig animationConfig;
		[ReadOnly] public SkeletonDataAsset skeletonDataAsset;
		[ReadOnly] public string frontBodySkin;
		[ReadOnly] public string backBodySkin;
		[ReadOnly] public string defaultWeaponSkin;

        public Vector2Int Position
        {
            get => runtime.position;
            set => runtime.position = value;
        }
        
		#region ITurnUnit

		string ITurnUnit.Id => id;
		int ITurnUnit.Speed => stats?.speed ?? 0;
		bool ITurnUnit.CanAct => runtime is { StillAlive: true, isStunned: false };
		public int ActionPriority { get; set; }

		#endregion
		
		internal static Unit LoadFromConfig(string unitId, UnitConfig config, Vector2Int startPosition)
		{
			var unit = new Unit()
			{
				id = unitId,
				configId = config.configId,
				name = config.unitName,
				stats = config.stats.Clone(),
				animationConfig = config.animationConfig,
				skeletonDataAsset = config.skeletonDataAsset,
				frontBodySkin = config.frontBodySkin,
				backBodySkin = config.backBodySkin,
				defaultWeaponSkin = config.defaultWeaponSkin
			};
			unit.runtime.Initialize(unit.stats.maxHp, startPosition, unit);
			return unit;
		}

		public List<EActionType> GetAvailableActions()
		{
			// todo: need to calculate based on unit state, abilities, etc.
			var actions = new List<EActionType>
			{
				EActionType.Move,
				EActionType.Attack,
				EActionType.Wait,
				EActionType.EndTurn
			};

			return actions;
		}

		public override string ToString() =>
			$"[Unit] {name}({id}) HP:{runtime?.currentHp}/{stats?.maxHp} Pos:{Position}";
        
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
