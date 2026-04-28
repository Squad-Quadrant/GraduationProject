using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Presentation.UI.Panel.TacticalItemMenu
{
	public class TacticalItemDetailLine : MonoBehaviour
	{
		[SerializeField, Required] private TextMeshProUGUI titleTmp;
		[SerializeField, Required] private TextMeshProUGUI valueTmp;
		[SerializeField] private string defaultTitle;
		[SerializeField] private string defaultValue;

		public void SetDefault()
		{
			titleTmp.text = defaultTitle;
			valueTmp.text = defaultValue;
		}

		public void SetPair(string title, string value)
		{
			titleTmp.text = title;
			valueTmp.text = value;
		}
	}
}
