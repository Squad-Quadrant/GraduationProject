using Core.Events;

namespace Data.Runtime.Events.View
{
	public enum EAnimationType
	{
		Move,
		Attack,
		Skill,
		Hit,
		Death,
		Spawn,
		Custom
	}

	/// <summary>
	/// Published by View layer when an animation finishes playing.
	///
	/// AsyncCommands can subscribe to this event to know when
	/// to call CompleteExecution() and let the CommandQueue proceed.
	///
	/// Example flow:
	/// 1. MoveCommand executes, publishes UnitMovedEvent
	/// 2. UnitView receives event, starts move animation
	/// 3. Animation completes, UnitView publishes AnimationCompleteEvent
	/// 4. MoveCommand receives event, calls CompleteExecution()
	/// 5. CommandQueue proceeds to next command
	/// </summary>
	public readonly struct AnimationCompleteEvent : IEvent
	{
		public string EntityId { get; }

		public EAnimationType AnimationType { get; }

		/// <summary>
		/// Optional tag for distinguishing multiple animations of same type.
		/// </summary>
		public string Tag { get; }

		public AnimationCompleteEvent(
			string entityId,
			EAnimationType animationType,
			string tag = null)
		{
			EntityId = entityId;
			AnimationType = animationType;
			Tag = tag;
		}

		public override string ToString()
		{
			var tagStr = Tag != null ? $", Tag:{Tag}" : "";
			return $"[AnimationComplete] {EntityId}: {AnimationType}{tagStr}";
		}
	}
}
