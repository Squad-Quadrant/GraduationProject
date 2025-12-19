using System.Collections.Generic;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.Config
{
	[CreateAssetMenu(fileName = "NewPanelConfig", menuName = "Game/UI/Panel Config")]
	public class UIPanelConfig : ScriptableObject
	{
		[TitleGroup("Identity")]
        [SerializeField]
        [Tooltip("Unique identifier for this panel. Used for lookups and debugging.")]
        private string panelId;

        [TitleGroup("Identity")]
        [SerializeField, Required]
        [PreviewField(50, ObjectFieldAlignment.Left)]
        private UIPanel prefab;

        [TitleGroup("Rendering")]
        [SerializeField]
        [Tooltip("Which canvas layer this panel belongs to.\n" +
                 "• Overlay: Persists across scenes (DDOL)\n" +
                 "• Screen: Scene-specific interactive UI\n" +
                 "• World: 3D space UI (health bars, etc.)")]
        private EUICanvasLayer layer = EUICanvasLayer.Screen;

        [TitleGroup("Behavior")]
        [SerializeField]
        [LabelText("Managed By Stack")]
        [Tooltip("If true, this panel participates in navigation stack and ESC handling.\n" +
                 "If false, this panel has independent lifecycle (for HUD elements).")]
        private bool managedByStack = true;

        [TitleGroup("Behavior")]
        [SerializeField]
        [LabelText("Hide When Covered")]
        [Tooltip("If true, this panel becomes invisible when another panel is pushed on top.")]
        private bool hideWhenCovered;

        [TitleGroup("Behavior")]
        [SerializeField]
        [LabelText("Close On Back")]
        [Tooltip("If true, pressing ESC/Back will close this panel (unless OnBackPressed returns true).")]
        public bool closeOnBack = true;

        [TitleGroup("Behavior")]
        [SerializeField]
        [LabelText("Block Input")]
        [Tooltip("If true, blocks raycasts to UI elements behind this panel.")]
        private bool blockInput = true;

        [TitleGroup("Lifecycle")]
        [SerializeField]
        [LabelText("Preload")]
        [Tooltip("If true, instantiate this panel during initialization (hidden). Reduces open latency.")]
        public bool preload;

        [TitleGroup("Lifecycle")]
        [SerializeField]
        [LabelText("Cache On Close")]
        [Tooltip("If true, hide instead of destroy when closed. Reuses instance on next open.")]
        public bool cacheOnClose;

        public string PanelId => string.IsNullOrEmpty(panelId) ? name : panelId;
        public UIPanel Prefab => prefab;
        public EUICanvasLayer Layer => layer;
        public bool ManagedByStack => managedByStack;
        public bool HideWhenCovered => managedByStack && hideWhenCovered;
        public bool CloseOnBack => managedByStack && closeOnBack;
        public bool BlockInput => managedByStack && blockInput;
        public bool Preload => preload;
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
