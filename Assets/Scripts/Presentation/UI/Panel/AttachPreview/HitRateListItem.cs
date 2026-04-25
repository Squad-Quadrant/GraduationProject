using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    public class HitRateListItem : MonoBehaviour
    {
        [SerializeField] private Text reason;
        [SerializeField] private Text valude;
        public void Init(string reason, string value)
        {
            this.reason.text = reason + "：";
            this.valude.text = value;
        }
    }
}