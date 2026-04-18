namespace Systems.Interaction
{
	public static class InteractionStates
	{
		public const string Idle = "Idle";
		public const string UnitSelected = "UnitSelected";
		public const string MovementPreview = "MovementPreview";
		public const string AttackPreview = "AttackPreview";
		public const string SkillPreview = "SkillPreview";
		public const string InteractPreview = "InteractPreview";
		public const string ItemSelection = "ItemSelection";
		public const string Targeting = "Targeting"; // 在地图上选目标格（道具/技能通用）
		public const string Executing = "Executing";
		public const string Paused = "Paused";
		public const string GameOver = "GameOver";
		public const string WaitingForSystem = "WaitingForSystem";
	}
}
