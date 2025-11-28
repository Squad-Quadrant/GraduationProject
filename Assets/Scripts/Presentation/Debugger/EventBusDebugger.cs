using Core.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Debugger
{
	public class EventBusDebugger : MonoBehaviour
	{
		[TitleGroup("Info")]
		[ShowInInspector] private bool EnableLogs => EventBus.EnableLogs;
		[ShowInInspector] private bool EnableWarnings => EventBus.EnableWarnings;

		[TitleGroup("Control Panel")]
		[HorizontalGroup("Control Panel/Buttons")]
		[Button(ButtonSizes.Medium)]
		private void SwitchLogs()
		{
			EventBus.EnableLogs = !EventBus.EnableLogs;
			Debug.Log("[EventBusDebugger] EventBus logging " + (EventBus.EnableLogs ? "enabled" : "disabled"));
		}

		[HorizontalGroup("Control Panel/Buttons")]
		[Button(ButtonSizes.Medium)]
		private void SwitchWarnings()
		{
			EventBus.EnableWarnings = !EventBus.EnableWarnings;
			Debug.Log("[EventBusDebugger] EventBus warnings " + (EventBus.EnableWarnings ? "enabled" : "disabled"));
		}
	}
}
