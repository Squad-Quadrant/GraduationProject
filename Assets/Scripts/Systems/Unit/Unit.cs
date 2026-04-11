using System;
using System.Collections.Generic;
using Core.Events;
using Data.Runtime;
using Data.Runtime.Events.Unit;
using Presentation.Unit;
using Sirenix.OdinInspector;
using Spine.Unity;
using Systems.AI.Config;
using Systems.Buff;
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
	public class Unit : ITurnUnit, IEquipable, IBuffAble
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
		public int maxMovementAp;
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
		private int _currentHp;
        private int _currentDefense;
        private int _currentSan;
		private int _currentAp;
		public int apSpentOnMovement;
        public Vector2Int position;
        public bool isStunned;
        
        public int CurrentHp
        {
            get => _currentHp;
            set
            {
                TriggerInfoChanged();
                _currentHp = value;
            }
        }
        
        public int CurrentDefense
        {
            get => _currentDefense;
            set
            {
                TriggerInfoChanged();
                _currentDefense = value;
            }
        }

        public int CurrentSan
        {
            get => _currentSan;
            set
            {
                TriggerInfoChanged();
                _currentSan = value;
            }
        }
        
        public int CurrentAp
        {
            get => _currentAp;
            set
            {
                TriggerInfoChanged();
                _currentAp = value;
            }
        }

		public bool IsAlive => CurrentHp > 0;
		public bool CanAct => IsAlive && !isStunned;
		public bool HasAp => CurrentAp > 0;

        public bool HasAmmo
        {
            get
            {
                if (CurrentWeapon == null) return false;
                
                return CurrentWeapon.CurrentAmmo() > 0;
            }
        }

        public int RemainingMovementAp => Mathf.Min(CurrentAp, maxMovementAp - apSpentOnMovement);

        protected IEventBus EventBus;
		
		internal static Unit LoadFromConfig(string unitId, UnitConfig config, Vector2Int startPosition, IEventBus eventBus)
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
				maxMovementAp = config.maxMovementAp,
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

				_currentHp = config.maxHp,
                _currentDefense = config.defense,
                _currentSan = config.san,
                _currentAp = config.actionPoints,
				position = startPosition,
				isStunned = false,
                
                EventBus = eventBus
			};
		}

		public List<ActionAbility> GetAvailableActions()
		{
			var actions = new List<ActionAbility>
			{
				new(EActionType.Move, RemainingMovementAp > 0),
				new(EActionType.Attack, !CurrentEquipment.IsNullOrEmpty() && HasAp && HasAmmo),
				new(EActionType.Wait)
			};
			// todo: 使用道具
            if (!TacticalItem0.IsNullOrEmpty())
                actions.Add(new ActionAbility(EActionType.TacticalItem0, false));
            if (!TacticalItem1.IsNullOrEmpty())
                actions.Add(new ActionAbility(EActionType.TacticalItem1, false));
            if (!TacticalItem2.IsNullOrEmpty())
                actions.Add(new ActionAbility(EActionType.TacticalItem2, false));
            if (CurrentWeapon!= null)
                actions.Add(new ActionAbility(EActionType.Reload, HasAp));
            if (!MainWeapon.IsNullOrEmpty() && !SecondaryWeapon.IsNullOrEmpty())
                actions.Add(new ActionAbility(EActionType.SwitchWeapon, HasAp));
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

        public void TriggerInfoChanged()
        {
            EventBus.Publish(new UnitInfoChangedEvent(this));
        }

		public override string ToString() =>
			$"[Unit] {name}({id}) HP:{CurrentHp}/{maxHp} AP:{CurrentAp}/{maxAp} Pos:{position}";

		#region ITurnUnit

		string ITurnUnit.Id => id;
		int ITurnUnit.Speed => speed;
		bool ITurnUnit.CanAct => CanAct;
		public int ActionPriority { get; set; }
		void ITurnUnit.OnTurnStart()
		{
			CurrentAp = maxAp;
			apSpentOnMovement = 0;
		}

		#endregion

        #region IEquipable

        public EquipmentContainer MainWeapon { get; set; }
        public EquipmentContainer SecondaryWeapon { get; set; }

        private EquipmentContainer _currentEquipment;

        public EquipmentContainer CurrentEquipment
        {
            get
            {
                _currentEquipment ??= MainWeapon.IsNullOrEmpty() ? SecondaryWeapon : MainWeapon;
                return _currentEquipment;
            }
            set
            {
                _currentEquipment = value;
                EventBus.Publish(new UnitInfoChangedEvent(this));
            }
        }
        
        public WeaponLogic CurrentWeapon
        {
            get
            {
                if (CurrentEquipment.IsNullOrEmpty()) return null;
                if (CurrentEquipment.Logic is WeaponLogic weaponLogic)
                    return weaponLogic;
                return null;
            }
        }
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
            
            MainWeapon.Init(equipmentConfigs[0], this);
            SecondaryWeapon.Init(equipmentConfigs[1], this);
            TacticalItem0.Init(equipmentConfigs[2], this);
            TacticalItem1.Init(equipmentConfigs[3], this);
            TacticalItem2.Init(equipmentConfigs[4], this);

            CurrentEquipment = MainWeapon;
        }

        public EquipmentContainer GetEquipment(EActionType actionType)
        {
	        return actionType switch
	        {
		        EActionType.Attack => CurrentEquipment,
		        EActionType.TacticalItem0 => TacticalItem0,
		        EActionType.TacticalItem1 => TacticalItem0,
		        EActionType.TacticalItem2 => TacticalItem0,
		        _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
	        };
        }

        public void SwitchWeapon()
        {
            CurrentEquipment = _currentEquipment == MainWeapon ? SecondaryWeapon : MainWeapon;
            TriggerInfoChanged();
        }
        
        #endregion

        #region IBuffable
        
        public BuffProxy BuffProxy { get; }
        
        #endregion
	}

    public struct ActionAbility
    {
        public readonly EActionType ActionType;
        public readonly bool IsAvailable;
        public ActionAbility(EActionType actionType = EActionType.None, bool isAvailable = true)
        {
            ActionType = actionType;
            IsAvailable = isAvailable;
        }
    }
}
