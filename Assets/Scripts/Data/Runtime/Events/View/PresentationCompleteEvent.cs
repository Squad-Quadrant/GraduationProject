using Core.Events;

namespace Data.Runtime.Events.View
{
	public enum EPresentationCategory
	{
		Animation,
		UI,
		Camera,
		Audio,
		Custom
	}

	public readonly struct PresentationCompleteEvent : IEvent
	{
		public EPresentationCategory Category { get; }
		public string Type { get; }
		public string EntityId { get; }
		public string Tag { get; }

		public PresentationCompleteEvent(
			EPresentationCategory category,
			string type,
			string entityId = null,
			string tag = null)
		{
			Category = category;
			Type = type;
			EntityId = entityId;
			Tag = tag;
		}

		public bool Matches(
			EPresentationCategory? category = null,
			string type = null,
			string entityId = null,
			string tag = null)
		{
			return !((category.HasValue && Category != category.Value) ||
			         (type != null && Type != type) ||
			         (entityId != null && EntityId != entityId) ||
			         (tag != null && Tag != tag));
		}

		public override string ToString()
		{
			var parts = new System.Text.StringBuilder();
			parts.Append($"[PresentationComplete] {Category}.{Type}");
			if (EntityId != null) parts.Append($" Entity:{EntityId}");
			if (Tag != null) parts.Append($" Tag:{Tag}");
			return parts.ToString();
		}
	}

	public static class PresentationType
	{
		public static class Animation
		{
			public const string Move = "Move";
			public const string Attack = "Attack";
			public const string Skill = "Skill";
			public const string BeHit = "BeHit";
			public const string Death = "Death";
			public const string Spawn = "Spawn";
			public const string Idle = "Idle";
		}

		public static class UI
		{
			public const string TurnStart = "TurnStart";           // "Round N begins" splash
			public const string UnitTransition = "UnitTransition"; // Banner slides to next unit
			public const string TurnEnd = "TurnEnd";               // "Round N complete" splash

			public const string DamageNumber = "DamageNumber";
			public const string ActionMenu = "ActionMenu";
			public const string DialogBox = "DialogBox";
		}

		public static class Camera
		{
			public const string Focus = "Focus";
			public const string Pan = "Pan";
			public const string Shake = "Shake";
		}
	}
}
