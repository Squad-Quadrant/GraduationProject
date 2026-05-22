using Presentation.Audio;
using Presentation.Bootstrap;
using Presentation.Dialogue;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Test.WZHTest
{
	public class DialogueControllerHelper : MonoBehaviour
	{
		[SerializeField, Required] private DialogueController dialogueController;

		private void Awake()
		{
			var uiManager = RootContainer.Instance.Resolve<UIManager>();
			var audioService = RootContainer.Instance.Resolve<AudioService>();
			dialogueController.Initialize(uiManager, audioService, null);
		}
	}
}
