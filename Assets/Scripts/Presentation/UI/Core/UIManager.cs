using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Presentation.UI.Core
{
	public class UIManager : MonoBehaviour
	{
		[TitleGroup("Configuration")]
		[SerializeField]
		[LabelText("Enable ESC Navigation")]
		[Tooltip("If true, ESC key triggers Navigator.HandleBack()")]
		private bool enableEscNavigation = true;

		[TitleGroup("Configuration")]
		[SerializeField, Required]
		private Canvas overlayCanvas;

		[TitleGroup("Configuration")]
		[SerializeField, Required]
		private InputSystemUIInputModule inputModule;

		public UINavigator Navigator { get; private set; }

		public Transform OverlayCanvasTransform => overlayCanvas?.transform;

		#region Debug Info

		[TitleGroup("Runtime Status")]
		[ShowInInspector, ReadOnly]
		public bool IsInitialized => Navigator != null;

		[TitleGroup("Runtime Status")]
		[ShowInInspector, ReadOnly]
		[LabelText("Panel Stack Size")]
		public int PanelCount => Navigator?.Count ?? 0;

		[TitleGroup("Runtime Status")]
		[ShowInInspector, ReadOnly]
		[LabelText("Top Panel")]
		public string TopPanelName => Navigator?.TopPanel?.PanelName ?? "None";

		[TitleGroup("Runtime Status")]
		[ShowInInspector, ReadOnly]
		[LabelText("Panel Stack")]
		private List<string> PanelStackNames
		{
			get
			{
				if (Navigator == null) return new List<string> { "Not initialized" };
				var panels = Navigator.GetAllPanels();
				var names = new List<string>();
				for (int i = 0; i < panels.Count; i++)
				{
					var prefix = i == panels.Count - 1 ? "▶ " : "  ";
					names.Add($"{prefix}{panels[i]?.PanelName ?? "null"}");
				}
				return names.Count > 0 ? names : new List<string> { "Empty" };
			}
		}

		#endregion

		public void Awake()
		{
			if (!overlayCanvas || !inputModule)
			{
				this.LogError("UIManager is missing required references!");
				return;
			}

			DontDestroyOnLoad(gameObject);
			Navigator = new UINavigator();
			inputModule.cancel.action.performed += HandleCancelInput;
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			if (inputModule)
				inputModule.cancel.action.performed -= HandleCancelInput;
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
			Navigator?.Clear();
			Navigator = null;
		}

		private void HandleCancelInput(InputAction.CallbackContext ctx)
		{
			if (!enableEscNavigation)
			{
				this.Log("ESC ignored (navigation disabled)");
				return;
			}

			if (Navigator.HandleBack())
			{
				this.Log("Cancel handled by Navigator");
			}
			else
			{
				this.Log("Cancel ignored (no panels in stack)");
			}
		}

		private void OnSceneUnloaded(Scene scene)
		{
			Navigator.CleanupDestroyedPanels();
			this.Log($"Cleaned up panels after scene unload: {scene.name}");
		}

		public void SetEscNavigationEnabled(bool enable)
		{
			enableEscNavigation = enable;
			this.Log($"ESC navigation {(enable ? "enabled" : "disabled")}");
		}

		public void PushPanel(UIPanel panel) => Navigator.Push(panel);

		public UIPanel PopPanel() => Navigator.Pop();

		public void PopAllPanels()
		{
			while (Navigator.Count > 0)
				Navigator.Pop();
			this.Log("Popped all panels");
		}

		public bool RemovePanel(UIPanel panel) => Navigator.Remove(panel);

		public bool HasOpenPanels() => Navigator.Count > 0;

		public bool IsPanelOpen(UIPanel panel) => Navigator.Contains(panel);

		#region Debug Actions

		[TitleGroup("Debug Actions")]
		[HorizontalGroup("Debug Actions/Row1")]
		[Button("Pop Top Panel"), GUIColor(1f, 0.7f, 0.3f)]
		[EnableIf("@UnityEngine.Application.isPlaying && PanelCount > 0")]
		private void DebugPopPanel() => PopPanel();

		[HorizontalGroup("Debug Actions/Row1")]
		[Button("Pop All Panels"), GUIColor(1f, 0.4f, 0.3f)]
		[EnableIf("@UnityEngine.Application.isPlaying && PanelCount > 0")]
		private void DebugPopAllPanels() => PopAllPanels();

		[HorizontalGroup("Debug Actions/Row1")]
		[Button("Cleanup Destroyed"), GUIColor(0.8f, 0.8f, 0.4f)]
		[EnableIf("@UnityEngine.Application.isPlaying")]
		private void DebugCleanup() => Navigator.CleanupDestroyedPanels();

		#endregion
	}
}
