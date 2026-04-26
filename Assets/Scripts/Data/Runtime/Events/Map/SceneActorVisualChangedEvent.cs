using Core.Events;

namespace Data.Runtime.Events.Map
{
	public readonly struct SceneActorVisualChangedEvent : IEvent
	{
		public readonly uint Uid;

		public SceneActorVisualChangedEvent(uint uid)
		{
			Uid = uid;
		}
	}
}
