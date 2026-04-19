using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Data.Runtime;
using Data.Runtime.Events.Unit;
using Presentation.Bootstrap;
using Presentation.Unit;
using Sirenix.OdinInspector;
using Spine.Unity;
using Systems.AI.Config;
using Systems.Buff;
using Systems.Damage;
using Systems.Map;
using Systems.Map.SceneActor;
using Systems.Turn;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Config;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Systems.Unit
{
	public enum EUnitFaction
	{
		Player, Enemy, Neutral,
		None, // 用于占位或特殊情况
	}

	[Serializable]
	public class Unit : ITurnUnit, IBuffAble, IDamageInfluencer
	{
		[TitleGroup("Identity")]
		public string id;		// id for runtime instance
		public string configId;	// id for config so
		public string name;
		public string description;

		[TitleGroup("Config")]
		public int maxHp;
		public BuffProperty<int> speed;
		public BuffProperty<int> moveRange;
		public int maxMovementAp;
		public int maxAp;
        public BuffProperty<int> visionRange;
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
        public BuffProperty<bool> CanUseEquipment = new(PropertyType.CanUseEquipment, true);
        public BuffProperty<bool> CanAttack = new(PropertyType.CanAttack, false);
        
        public List<DamageInfluence> DamageInfluences { get; } = new();
        
        public Dictionary<BodyPartType, int> BodyPartInfo = new()
		{
			{ BodyPartType.None, 0},
	        { BodyPartType.Head, 0 },
	        { BodyPartType.Torso, 0 },
	        { BodyPartType.Arms, 0 },
	        { BodyPartType.Legs, 0 },
		};

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

        private IEventBus _eventBus;
        private IEventBus EventBus => _eventBus ??= LevelContainer.Instance.Resolve<IEventBus>();

        private IMapService _mapService;
        private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();
		
		internal static Unit LoadFromConfig(string unitId, UnitConfig config, Vector2Int startPosition)
		{
			var unit = new Unit
			{
				id = unitId,
				configId = config.configId,
				name = config.unitName,
				description = config.description,

				maxHp = config.maxHp,
				speed = new BuffProperty<int>(PropertyType.Speed ,config.speed),
				moveRange = new BuffProperty<int>(PropertyType.MoveRange, config.moveRange),
				maxMovementAp = config.maxMovementAp,
				maxAp = config.actionPoints,
                visionRange = new BuffProperty<int>(PropertyType.VisionRange, config.visionRange),
				faction = config.faction,
                defense = config.defense,
                defenseRate = config.defenseRate,
                aiBrainConfig = config.aiBrainConfig,

				animationConfig = config.animationConfig,
				skeletonDataAsset = config.skeletonDataAsset,
				frontBodySkin = config.frontBodySkin,
				backBodySkin = config.backBodySkin,
				icon = config.icon,

				_currentHp = config.maxHp,
                _currentDefense = config.defense,
                _currentSan = config.san,
                _currentAp = config.actionPoints,
				position = startPosition,
				isStunned = false,
			};
            
            return unit;
		}

		public List<ActionAbility> GetAvailableActions()
		{
			var neighbors = MapService.Data.GetNeighborCells(position);
			neighbors.Add(MapService.Data.GetCell(position));
			bool hasInteractableNeighbor = neighbors.Any(neighbor => neighbor.SceneActor is InteractableSceneActor { CanInteract: true });

			var actions = new List<ActionAbility>
			{
				new (EActionType.Move, RemainingMovementAp > 0),
				new (EActionType.Interact, hasInteractableNeighbor),
				new (EActionType.Attack, !CurrentEquipment.IsNullOrEmpty() && HasAp && HasAmmo && CanUseEquipment && CanAttack),
				new (EActionType.Wait)
			};

			bool hasAnyTacticalItem = TacticalItems != null && TacticalItems.Any(t => !t.IsNullOrEmpty());
			if (hasAnyTacticalItem)
			{
				bool hasAnyUsable = TacticalItems.Any(t =>
					!t.IsNullOrEmpty() && t.Logic is TacticalItemLogic { CanUse: true });
				actions.Add(new ActionAbility(EActionType.UseTacticalItem, HasAp && hasAnyUsable));
			}

            if (CurrentWeapon!= null)
	            actions.Add(new ActionAbility(EActionType.Reload, HasAp && CanUseEquipment));

            if (!MainWeapon.IsNullOrEmpty() && !SecondaryWeapon.IsNullOrEmpty())
	            actions.Add(new ActionAbility(EActionType.SwitchWeapon, HasAp && CanUseEquipment));

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

        #region IDamageInfluencer

        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            DamageInfluences.RemoveAll(influence => influence == null);
            return DamageInfluences;
        }

        #endregion

        #region ITurnUnit

		string ITurnUnit.Id => id;

		string ITurnUnit.DisplayName => name;

		int ITurnUnit.Speed => speed;

		bool ITurnUnit.CanAct => CanAct;

		int ITurnUnit.ActionPriority { get; set; }

		EUnitFaction ITurnUnit.Faction => faction;

		Sprite ITurnUnit.DisplayIcon => icon;

		Vector2Int ITurnUnit.CellPosition => position;

		void ITurnUnit.OnTurnStart()
		{
			CanAttack.Value = true;
			CurrentAp = maxAp;
			apSpentOnMovement = 0;
		}

		#endregion

        #region Equipment

        public EquipmentContainer MainWeapon { get; private set; }
        public EquipmentContainer SecondaryWeapon { get; private set; }

        public EquipmentContainer[] TacticalItems { get; private set; }

        public IReadOnlyList<EquipmentContainer> TacticalItemInfos => TacticalItems;

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

        public void InitEquipment(EquipmentConfig main, EquipmentConfig secondary, EquipmentConfig[] tacticalItems)
        {
            MainWeapon = new EquipmentContainer();
            SecondaryWeapon = new EquipmentContainer();
            MainWeapon.Init(main, this);
            SecondaryWeapon.Init(secondary, this);

            TacticalItems = new EquipmentContainer[tacticalItems.Length];
            for (int i = 0; i < tacticalItems.Length; i++)
            {
	            TacticalItems[i] = new EquipmentContainer();
	            TacticalItems[i].Init(tacticalItems[i], this);
            }

            CurrentEquipment = MainWeapon;
        }

        public EquipmentContainer GetTacticalItem(int slotIndex)
        {
	        if (TacticalItems == null)
		        throw new InvalidOperationException("TacticalItems not initialized. Call InitEquipment first.");
	        if (slotIndex < 0 || slotIndex >= TacticalItems.Length)
		        throw new ArgumentOutOfRangeException(nameof(slotIndex), slotIndex, $"Slot index must be in [0, {TacticalItems.Length}).");
	        return TacticalItems[slotIndex];
        }

        public void SwitchWeapon()
        {
            CurrentEquipment = _currentEquipment == MainWeapon ? SecondaryWeapon : MainWeapon;
            TriggerInfoChanged();
        }
        
        #endregion

        #region IBuffable
        
        public BuffProxy BuffProxy { get; set; }

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
