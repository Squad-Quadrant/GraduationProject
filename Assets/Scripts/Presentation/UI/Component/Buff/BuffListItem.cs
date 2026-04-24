using Systems.Buff;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Component.Buff
{
    public class BuffListItem : MonoBehaviour
    {
        [SerializeField] private Image icon;

        public void Init(BuffInfo info)
        {
            icon.sprite = info.BuffData.icon;
        }
    }
}