using System;
using System.Collections.Generic;
using Core.Events;
using Data.Runtime;
using Data.Runtime.Events.Unit;
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
		protected int currentHp;
        protected int currentDefense;
        protected int currentSan;
		protected int currentAp;
            
        public Vector2Int position;
        public bool IsStunned;
        
        public int CurrentHp
        {
            get => currentHp;
            set
            {
                TriggerInfoChanged();
                currentHp = value;
            }
        }
        
        public int CurrentDefense
        {
            get => currentDefense;
            set
            {
                TriggerInfoChanged();
                currentDefense = value;
            }
        }

        public int CurrentSan
        {
            get => currentSan;
            set
            {
                TriggerInfoChanged();
                currentSan = value;
            }
        }
        
        public int CurrentAp
        {
            get => currentAp;
            set
            {
                TriggerInfoChanged();
                currentAp = value;
            }
        }

		public bool IsAlive => CurrentHp > 0;
		public bool CanAct => IsAlive && !IsStunned;
		public bool HasAp => CurrentAp > 0;

        public bool HasAmmo
        {
            get
            {
                if (CurrentWeapon == null) return false;
                
                return CurrentWeapon.CurrentAmmo() > 0;
            }
        }

        protected IEventBus _eventBus;
		
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

				CurrentHp = config.maxHp,
                CurrentDefense = config.defense,
                CurrentSan = config.san,
				position = startPosition,
				IsStunned = false,
				CurrentAp = config.actionPoints,
                
                _eventBus = eventBus
			};
		}

		public List<ActionAbility> GetAvailableActions()
		{
			var actions = new List<ActionAbility>();
            actions.Add(new ActionAbility(EActionType.Move, HasAp));
            actions.Add(new ActionAbility(EActionType.Attack, !CurrentEquipment.IsNullOrEmpty() && HasAp && HasAmmo));
			actions.Add(new ActionAbility(EActionType.Wait));
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
            _eventBus?.Publish(new UnitInfoChangedEvent(this));
        }

		public override string ToString() =>
			$"[Unit] {name}({id}) HP:{CurrentHp}/{maxHp} AP:{CurrentAp}/{maxAp} Pos:{position}";

		#region ITurnUnit

		string ITurnUnit.Id => id;
		int ITurnUnit.Speed => speed;
		bool ITurnUnit.CanAct => CanAct;
		public int ActionPriority { get; set; }
		void ITurnUnit.OnTurnStart() => CurrentAp = maxAp;

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
                _eventBus.Publish(new UnitInfoChangedEvent(this));
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
            switch (actionType)
            {
                case EActionType.Attack:
                    return CurrentEquipment;
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

        public void SwitchWeapon()
        {
            CurrentEquipment = _currentEquipment == MainWeapon ? SecondaryWeapon : MainWeapon;
            TriggerInfoChanged();
        }
        
        #endregion
	}

    public struct ActionAbility
    {
        public EActionType actionType;
        public bool isAvailable;
        public ActionAbility(EActionType actionType = EActionType.None, bool isAvailable = true)
        {
            this.actionType = actionType;
            this.isAvailable = isAvailable;
        }
    }
}
