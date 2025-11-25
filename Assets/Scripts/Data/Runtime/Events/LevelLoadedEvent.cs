using Core.Events;

namespace Data.Runtime.Events
{
	public readonly struct LevelLoadedEvent : IEvent
	{
		public string LevelId { get; }
		public string LevelName { get; }

		public LevelLoadedEvent(string levelId, string levelName)
		{
			LevelId = levelId;
			LevelName = levelName;
		}

		public override string ToString() => $"[LevelLoaded] {LevelName} ({LevelId})";
	}
}
