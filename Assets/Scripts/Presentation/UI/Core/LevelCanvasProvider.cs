using Core.Log;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.UI.Core
{
	[RequireComponent(typeof(Canvas))]
	public class LevelCanvasProvider : MonoBehaviour
	{
		[SerializeField]
		[LabelText("Layer")]
		[Tooltip("Screen or World. Overlay should use UIManager's built-in canvas.")]
		private EUICanvasLayer layer = EUICanvasLayer.Screen;

		[SerializeField, ReadOnly]
		private Canvas canvas;

		private bool _registered;

		private void Reset() => canvas = GetComponent<Canvas>();

		private void Awake()
		{
			if (!canvas) canvas = GetComponent<Canvas>();
		}

		private void OnEnable()
		{
			if (_registered || layer == EUICanvasLayer.Overlay) return;

			var uiManager = RootContainer.Instance.TryResolve<UIManager>();
			if (uiManager)
			{
				uiManager.RegisterCanvas(layer, canvas);
				_registered = true;
			}
			else
				this.LogError("UIManager not found in RootContainer.");
		}

		private void OnDisable()
		{
			if (!_registered) return;
			if (!RootContainer.Instance) return;
			var uiManager = RootContainer.Instance.TryResolve<UIManager>();
			uiManager?.UnregisterCanvas(layer);
			_registered = false;
		}
	}
}
