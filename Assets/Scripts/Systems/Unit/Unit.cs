using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
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
using Systems.Vision;
using UnityEngine;

namespace Systems.Unit
{
	public enum EUnitFaction
	{
		Player, Enemy, Neutral,
		None, // 用于占位或特殊情况
	}

	public enum EWeaponSlot
	{
		Main,
		Secondary
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
        public List<DamageInfluence> BeHitDamageInfluences { get; } = new();
        
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

        private IUnitService _unitService;
        private IUnitService UnitService => _unitService ??= LevelContainer.Instance.Resolve<IUnitService>();

        private IVisionService _visionService;
        private IVisionService VisionService => _visionService ??= LevelContainer.Instance.Resolve<IVisionService>();

		internal static Unit LoadFromConfig(
			string unitId, UnitConfig config, Vector2Int startPosition, IReadOnlyList<Vector2Int> patrolWaypoints)
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
			bool canMove = HasAp && RemainingMovementAp > 0;
			actions.Add(new ActionAbility(EActionType.Move, canMove));

			// interact
			var neighbors = MapService.Data.GetNeighborCells(position);
			neighbors.Add(MapService.Data.GetCell(position));
			bool hasInteractableNeighbor = neighbors.Any(neighbor => neighbor.SceneActor is InteractableSceneActor { CanInteract: true });
			if (HasAp && hasInteractableNeighbor)
				actions.Add(new ActionAbility(EActionType.Interact));

			// attack (normal + precise)
			bool canAttackNow = !CurrentWeaponContainer.IsNullOrEmpty() &&
			                 HasAmmo &&
			                 CanAttack && HasAp &&
			                 (IsUsingMainWeapon && CanUseMainWeapon || IsUsingSecondaryWeapon) &&
			                 CalculateSelectableTargets(UnitService, VisionService).Count > 0;
			actions.Add(new ActionAbility(EActionType.Attack, canAttackNow, payload: 0));
			if (CurrentWeaponLogic.CanPreciseShoot())
				actions.Add(new ActionAbility(EActionType.Attack, canAttackNow, payload: 1));


			// reload
			bool canReload = CurrentWeaponLogic is { FullAmmo: false } &&
			                 (IsUsingMainWeapon && CanUseMainWeapon || IsUsingSecondaryWeapon);
			if (HasAp && canReload)
				actions.Add(new ActionAbility(EActionType.Reload));

			// switch weapon
			bool canSwitchWeapon = !MainWeapon.IsNullOrEmpty() && !SecondaryWeapon.IsNullOrEmpty();
			if (canSwitchWeapon)
				actions.Add(new ActionAbility(EActionType.SwitchWeapon));

			// tactical items
			if (_tacticalItems != null)
			{
				for (int i = 0; i < _tacticalItems.Length; i++)
				{
					var container = _tacticalItems[i];
					if (container.IsNullOrEmpty() || container.Logic is not TacticalItemLogic tacticalItemLogic) continue;

					bool hasRemainingUses = tacticalItemLogic.RemainingUses > 0;
					bool canUseSlot = HasAp && tacticalItemLogic.CanUse;
					if (hasRemainingUses)
						actions.Add(new ActionAbility(EActionType.UseTacticalItem, canUseSlot, payload: i));
				}
			}

			// skill
			bool canUseSkill = HasAp && Skill is { CanUse: true };
			actions.Add(new ActionAbility(EActionType.UseSkill, canUseSkill));

			// Wait
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

		public List<Unit> CalculateSelectableTargets(IUnitService unitService, IVisionService visionService)
		{
			var reachableEnemies = unitService.GetAllAliveUnits()
				.Where(u => CurrentWeaponLogic.CheckAttackable(u)).ToList();

			var visibleCells = visionService.CurrentVisibleCells;
			var enemies = reachableEnemies.Where(enemy => visibleCells.Contains(enemy.position)).ToList();

			this.Log($"Found {enemies.Count} valid targets for attack.");
			return enemies;
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
            if (context.Attacker == this)
            {
                DamageInfluences.RemoveAll(influence => influence == null);
                return DamageInfluences;
            }

            if (context.Defender == this)
            {
                BeHitDamageInfluences.RemoveAll(influence => influence == null);
                return BeHitDamageInfluences;
            }
            
            return new List<DamageInfluence>();
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

        private EquipmentContainer[] _tacticalItems;
        public IReadOnlyList<EquipmentContainer> TacticalItems => _tacticalItems;

        public EWeaponSlot CurrentWeaponSlot { get; private set; } = EWeaponSlot.Main;

        public EquipmentContainer CurrentWeaponContainer => CurrentWeaponSlot == EWeaponSlot.Main ? MainWeapon : SecondaryWeapon;
        
        public WeaponLogic CurrentWeaponLogic => CurrentWeaponContainer?.Logic as WeaponLogic;

        public bool IsUsingMainWeapon => CurrentWeaponSlot == EWeaponSlot.Main;
        public bool IsUsingSecondaryWeapon => CurrentWeaponSlot == EWeaponSlot.Secondary;

        public void InitEquipment(EquipmentConfig main, EquipmentConfig secondary, EquipmentConfig[] tacticalItems)
        {
            MainWeapon = new EquipmentContainer();
            SecondaryWeapon = new EquipmentContainer();
            MainWeapon.Init(main, this);
            SecondaryWeapon.Init(secondary, this);

            _tacticalItems = new EquipmentContainer[tacticalItems.Length];
            for (int i = 0; i < tacticalItems.Length; i++)
            {
	            _tacticalItems[i] = new EquipmentContainer();
	            _tacticalItems[i].Init(tacticalItems[i], this);
            }

            CurrentWeaponSlot = MainWeapon.IsNullOrEmpty() && !SecondaryWeapon.IsNullOrEmpty()
	            ? EWeaponSlot.Secondary
	            : EWeaponSlot.Main;

            // todo:这是一个临时写法，预期是武器为unit添加一个buff，但是buff现在没做支持参数的初始化，而我们现在也没有扔武器一说，所以暂时可以直接调整基础数值
            var (speedPenalty, moveRangePenalty) = WeaponWeight.SumPenalty(main, secondary);
            speed.Value = WeaponWeight.ApplyPenalty(speed.Value, speedPenalty);
            moveRange.Value = WeaponWeight.ApplyPenalty(moveRange.Value, moveRangePenalty);
        }

        public EquipmentContainer GetTacticalItem(int slotIndex)
        {
	        if (_tacticalItems == null)
		        throw new InvalidOperationException("TacticalItems not initialized. Call InitEquipment first.");
	        if (slotIndex < 0 || slotIndex >= _tacticalItems.Length)
		        throw new ArgumentOutOfRangeException(nameof(slotIndex), slotIndex, $"Slot index must be in [0, {_tacticalItems.Length}).");
	        return _tacticalItems[slotIndex];
        }

        public void SwitchWeapon()
        {
	        CurrentWeaponSlot = CurrentWeaponSlot == EWeaponSlot.Main
		        ? EWeaponSlot.Secondary
		        : EWeaponSlot.Main;
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
        public readonly int Payload;

        public ActionAbility(EActionType actionType = EActionType.None, bool isAvailable = true, int payload = 0)
        {
            ActionType = actionType;
            IsAvailable = isAvailable;
            Payload = payload;
        }
    }
}
