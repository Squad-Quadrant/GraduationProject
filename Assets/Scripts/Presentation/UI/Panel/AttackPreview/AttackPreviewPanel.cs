using System.Collections.Generic;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.AttackPreview
{
    public class AttackPreviewPanel : UIPanel, IInitializable<Systems.Unit.Unit>
    {
	    [SerializeField, ChildGameObjectsOnly, Required] private AttackContextDisplayPanel attackContextDisplayPanel;

	    private Systems.Unit.Unit _currentUnit;

	    protected override void OnOpen() => EventBus.Subscribe<DisplayAttackContextEvent>(OnDisplayAttackContext);

	    protected override void OnClose() => EventBus.Unsubscribe<DisplayAttackContextEvent>(OnDisplayAttackContext);

	    public void DataInitialize(Systems.Unit.Unit unit)
	    {
		    _currentUnit = unit;
		    attackContextDisplayPanel.Default();
	    }

	    private void OnDisplayAttackContext(DisplayAttackContextEvent e)
	    {
		    if (e.Context == null)
		    {
			    attackContextDisplayPanel.Default();
			    return;
		    }

		    attackContextDisplayPanel.Show(e.Context, _currentUnit, e.ContextDic);
	    }
    }
}
