using System.Collections.Generic;
using Data.Runtime.Events.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    public class HitRateList : MonoBehaviour
    {
        [SerializeField] private Text hitRateText;
        [SerializeField] private Transform context;
        [SerializeField] private HitRateListItem hitRateListItemPrefab;
        private List<HitRateListItem> listItems = new();
        
        public void Refresh(DisplayHitPercentEvent e)
        {
            foreach (var hitRateListItem in listItems)
            {
                hitRateListItem.gameObject.SetActive(false);
                Destroy(hitRateListItem.gameObject);
            }
            listItems.Clear();
            
            if (!e.IsValid)
            {
                hitRateText.text = "无法计算命中率";
                return;
            }
            hitRateText.text = $"命中率: {e.HitPercent}%";
            foreach (var influence in e.HitRateInfluences)
            {
                var item = Instantiate(hitRateListItemPrefab, context);
                item.Init(influence.Item1, influence.Item2);
                listItems.Add(item);
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(context as RectTransform);
        }
    }
}