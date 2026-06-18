using System.Collections.Generic;
using System.Linq;
using Systems.Buff;
using UnityEngine;

namespace Presentation.UI.Component.Buff
{
    public class BuffList : MonoBehaviour
    {
        [SerializeField] private Transform buffContainer;
        [SerializeField] private BuffListItem itemPrefab;
        [SerializeField] private BuffTooltip tooltip;

        private BuffProxy _proxy;
        private readonly Dictionary<BuffInfo, BuffListItem> _itemDict = new();
        
        public void Init(IBuffAble owner)
        {
	        Unbind();

	        if (owner?.BuffProxy == null) return;
	        _proxy = owner.BuffProxy;

	        foreach (var info in _proxy.BuffInfos)
		        Attach(info);

	        _proxy.OnAttach += Attach;
	        _proxy.OnLost += Lost;
        }

        private void OnDestroy() => Unbind();

        public void Unbind()
        {
	        if (_proxy != null)
	        {
		        _proxy.OnAttach -= Attach;
		        _proxy.OnLost -= Lost;
		        _proxy = null;
	        }
	        ClearItems();
        }

        private void Attach(BuffInfo info)
        {
            if (!info.BuffData.showInUI) return;
            if (_itemDict.ContainsKey(info)) return;
            var item = Instantiate(itemPrefab, buffContainer);
            item.Init(info, tooltip);
            _itemDict.Add(info, item);
        }

        private void Lost(BuffInfo info)
        {
	        if (!_itemDict.TryGetValue(info, out var item)) return;
	        if (item) Destroy(item.gameObject);
	        _itemDict.Remove(info);
        }

        private void ClearItems()
        {
	        foreach (var item in _itemDict.Values.Where(item => item))
		        Destroy(item.gameObject);
	        _itemDict.Clear();
        }
    }
}
