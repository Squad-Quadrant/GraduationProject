using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.UI.Config
{
	[CreateAssetMenu(fileName = "NewPanelConfig", menuName = "Game/UI/Panel Config")]
	public class UIPanelConfig : ScriptableObject
	{
		[TitleGroup("Identity")]
        [SerializeField]
        [Tooltip("Unique identifier for this panel. Used for lookups and debugging.")]
        private string panelId;

        [TitleGroup("Identity")]
        [SerializeField, Required, AssetsOnly]
        private UIPanel prefab;

        [TitleGroup("Rendering")]
        [SerializeField]
        [Tooltip("Which canvas layer this panel belongs to.\n" +
                 "• Overlay: Persists across scenes (DDOL)\n" +
                 "• Screen: Scene-specific interactive UI\n" +
                 "• World: 3D space UI (health bars, etc.)")]
        private EUICanvasLayer layer = EUICanvasLayer.Screen;

        [TitleGroup("Lifecycle")]
        [SerializeField]
        [LabelText("Cache On Close")]
        [Tooltip("If true, hide instead of destroy when closed. Reuses instance on next open.")]
        private bool cacheOnClose;

        public string PanelId => string.IsNullOrEmpty(panelId) ? name : panelId;
        public UIPanel Prefab => prefab;
        public EUICanvasLayer Layer => layer;
        public bool CacheOnClose => cacheOnClose;

        private void OnValidate()
        {
	        // Auto-generate panelId from prefab name if empty
	        if (string.IsNullOrEmpty(panelId) && prefab)
		        panelId = prefab.name;
        }

        [TitleGroup("Debug")]
        [Button("Validate Config", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
        private void ValidateConfig()
        {
	        bool valid = true;

	        if (string.IsNullOrEmpty(PanelId))
	        {
		        Debug.LogWarning($"[UIPanelConfig] Panel ID is empty");
		        valid = false;
	        }

	        if (!prefab)
	        {
		        Debug.LogWarning($"[UIPanelConfig] '{PanelId}': Prefab not assigned");
		        valid = false;
	        }
	        else if (!prefab.GetComponent<CanvasGroup>())
	        {
		        Debug.LogWarning($"[UIPanelConfig] '{PanelId}': Prefab missing CanvasGroup");
		        valid = false;
	        }

	        if (valid)
		        Debug.Log($"[UIPanelConfig] '{PanelId}' ✓");
        }
	}
}
