using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Data.Runtime;
using Data.Runtime.Events.Unit;
using Presentation.Bootstrap;
using Presentation.UI.Component.UnitPortrait;
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
using Systems.Unit.Skill.Logic;
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
		public string unitClass;
		public string description;

		[TitleGroup("Config")]
		public int maxHp;
		public BuffProperty<int> speed;
		public BuffProperty<int> moveRange;
		public int maxMovementAp;
		public int maxAp;
        public BuffProperty<int> visionRange;
		public EUnitFaction faction;
        public int maxDefense;
        public float defenseRate;
        public AIArchetype aiArchetype;
        public int activationGroupId;

		[TitleGroup("Presentation")]
		public UnitAnimationConfig animationConfig;
		public SkeletonDataAsset skeletonDataAsset;
		public string frontBodySkin;
		public string backBodySkin;
		public string defaultWeaponSkin;
		public Sprite icon;
		public UnitPortraitView portraitPrefabLoadout;
		public UnitPortraitView portraitPrefabUnitInfo;

		[TitleGroup("Runtime")]
		private int _currentHp;
        private int _currentDefense;
        private int _currentSan;
		private int _currentAp;
		public int apSpentOnMovement;
        public Vector2Int position;
        public bool isStunned;
        public BuffProperty<bool> CanUseMainWeapon = new(PropertyType.CanUseMainWeapon, true);
        public BuffProperty<bool> CanAttack = new(PropertyType.CanAttack, false);

        public BuffProperty<bool> CanAIUseEye = new(PropertyType.CanAIUseEye, true);

        // for ai only
        public Vector2Int spawnPosition;
        public IReadOnlyList<Vector2Int> patrolWaypoints;
        public int patrolCursor;

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
	            _currentHp = value;
                TriggerInfoChanged();
            }
        }
        
        public int CurrentDefense
        {
            get => _currentDefense;
            set
            {
	            _currentDefense = value;
                TriggerInfoChanged();
            }
        }

        public int CurrentSan
        {
            get => _currentSan;
            set
            {
	            _currentSan = value;
                TriggerInfoChanged();
            }
        }
        
        public int CurrentAp
        {
            get => _currentAp;
            set
            {
	            _currentAp = value;
                TriggerInfoChanged();
            }
        }

		public bool IsAlive => CurrentHp > 0;
		public bool CanAct => IsAlive && !isStunned;
		public bool HasAp => CurrentAp > 0;

        public bool HasAmmo
        {
            get
            {
                if (CurrentWeaponLogic == null) return false;
                
                return CurrentWeaponLogic.CurrentAmmo() > 0;
            }
        }

        public int RemainingMovementAp => Mathf.Min(CurrentAp, maxMovementAp - apSpentOnMovement);

        private IEventBus _eventBus;
        private IEventBus EventBus => _eventBus ??= LevelContainer.Instance.Resolve<IEventBus>();

        private IMapService _mapService;
        private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();
		
		internal static Unit LoadFromConfig(
			string unitId,
			UnitConfig config,
			Vector2Int startPosition,
			IReadOnlyList<Vector2Int> patrolWaypoints = null)
		{
			var unit = new Unit
			{
				id = unitId,
				configId = config.configId,
				name = config.unitName,
				unitClass = config.unitClass,
				description = config.description,

				maxHp = config.maxHp,
				speed = new BuffProperty<int>(PropertyType.Speed ,config.speed),
				moveRange = new BuffProperty<int>(PropertyType.MoveRange, config.moveRange),
				maxMovementAp = config.maxMovementAp,
				maxAp = config.actionPoints,
                visionRange = new BuffProperty<int>(PropertyType.VisionRange, config.visionRange),
				faction = config.faction,
                maxDefense = config.defense,
                defenseRate = config.defenseRate,
                aiArchetype = config.aiArchetype,

				animationConfig = config.animationConfig,
				skeletonDataAsset = config.skeletonDataAsset,
				frontBodySkin = config.frontBodySkin,
				backBodySkin = config.backBodySkin,
				icon = config.icon,
				portraitPrefabLoadout = config.portraitPrefabLoadout,
				portraitPrefabUnitInfo = config.portraitPrefabUnitInfo,

				_currentHp = config.maxHp,
                _currentDefense = config.defense,
                _currentSan = config.san,
                _currentAp = config.actionPoints,
				position = startPosition,
				isStunned = false,

				spawnPosition = startPosition,
				patrolWaypoints = patrolWaypoints,
				patrolCursor = 0,
			};

			if (config.skillConfig)
				unit.Skill = SkillLogicFactory.Create(config.skillConfig, unit);

            return unit;
		}

		public List<ActionAbility> GetAvailableActions()
		{
			var actions = new List<ActionAbility>();

			// move
			if (HasAp && RemainingMovementAp > 0)
				actions.Add(new ActionAbility(EActionType.Move));

			// interact
			var neighbors = MapService.Data.GetNeighborCells(position);
			neighbors.Add(MapService.Data.GetCell(position));
			bool hasInteractableNeighbor = neighbors.Any(neighbor => neighbor.SceneActor is InteractableSceneActor { CanInteract: true });
			if (HasAp && hasInteractableNeighbor)
				actions.Add(new ActionAbility(EActionType.Interact));

			// attack (normal + precise)
			bool canAttack = !CurrentWeaponContainer.IsNullOrEmpty() && HasAmmo && CanAttack &&
			                 (IsUsingMainWeapon() && CanUseMainWeapon || IsUseSecondaryWeapon());
			if (HasAp && canAttack)
			{
				actions.Add(new ActionAbility(EActionType.Attack, payload: 0));
				if (CurrentWeaponLogic.CanPreciseShoot())
					actions.Add(new ActionAbility(EActionType.Attack, payload: 1));
			}

			// reload
			bool canReload = CurrentWeaponLogic is { FullAmmo: false } &&
			                 (IsUsingMainWeapon() && CanUseMainWeapon || IsUseSecondaryWeapon());
			if (HasAp && canReload)
				actions.Add(new ActionAbility(EActionType.Reload));

			// switch weapon
			bool canSwitchWeapon = !MainWeapon.IsNullOrEmpty() && !SecondaryWeapon.IsNullOrEmpty();
			if (canSwitchWeapon)
				actions.Add(new ActionAbility(EActionType.SwitchWeapon));

			// tactical items
			if (TacticalItems != null)
			{
				for (int i = 0; i < TacticalItems.Length; i++)
				{
					var container = TacticalItems[i];
					if (container.IsNullOrEmpty()) continue;

					bool slotUsable = container.Logic is TacticalItemLogic { CanUse: true };
					if (HasAp && slotUsable)
						actions.Add(new ActionAbility(EActionType.UseTacticalItem, payload: i));
				}
			}

			// skill
			if (HasAp && Skill is { CanUse: true })
				actions.Add(new ActionAbility(EActionType.UseSkill));

			actions.Add(new ActionAbility(EActionType.Wait));

			return actions;
		}

		public bool CanUseAction(EActionType type, int payload = 0)
		{
			var allAvailable = GetAvailableActions();
			foreach (var action in allAvailable)
			{
				if (type == action.ActionType && payload == action.Payload)
					return action.IsAvailable;
			}
			return false;
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

        public string DisplayName => name;

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
			Skill?.OnOwnerTurnStart();
		}

		#endregion

		public SkillLogic Skill { get; private set; }

        #region Equipment

        public EquipmentContainer MainWeapon { get; private set; }
        public EquipmentContainer SecondaryWeapon { get; private set; }

        public EquipmentContainer[] TacticalItems { get; private set; }

        public IReadOnlyList<EquipmentContainer> TacticalItemInfos => TacticalItems;

        private EquipmentContainer _currentWeaponContainer;

        public EquipmentContainer CurrentWeaponContainer
        {
            get
            {
                _currentWeaponContainer ??= MainWeapon.IsNullOrEmpty() ? SecondaryWeapon : MainWeapon;
                return _currentWeaponContainer;
            }
            set
            {
                _currentWeaponContainer = value;
                EventBus.Publish(new UnitInfoChangedEvent(this));
            }
        }
        
        public WeaponLogic CurrentWeaponLogic
        {
            get
            {
                if (CurrentWeaponContainer.IsNullOrEmpty()) return null;
                if (CurrentWeaponContainer.Logic is WeaponLogic weaponLogic)
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

            _currentWeaponContainer = MainWeapon;
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
            CurrentWeaponContainer = _currentWeaponContainer == MainWeapon ? SecondaryWeapon : MainWeapon;
            TriggerInfoChanged();
        }

        public bool IsUsingMainWeapon()
        {
	        if (MainWeapon  == null) return false;
	        return _currentWeaponContainer == MainWeapon;
        }

        public bool IsUseSecondaryWeapon()
        {
	        if (SecondaryWeapon  == null) return false;
	        return _currentWeaponContainer == SecondaryWeapon;
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
        public readonly int Payload;

        public ActionAbility(EActionType actionType = EActionType.None, bool isAvailable = true, int payload = 0)
        {
            ActionType = actionType;
            IsAvailable = isAvailable;
            Payload = payload;
        }
    }
}
