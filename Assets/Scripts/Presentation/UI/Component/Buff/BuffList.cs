using System.Collections.Generic;
using Systems.Buff;
using UnityEngine;

namespace Presentation.UI.Component.Buff
{
    public class BuffList : MonoBehaviour
    {
        [SerializeField] private Transform buffContainer;
        [SerializeField] private BuffListItem itemPrefab;
        private IBuffAble _owner;
        private BuffProxy _proxy;
        
        private Dictionary<BuffInfo, BuffListItem> itemDict = new();
        
        public void Init(IBuffAble owner)
        {
            _owner = owner;
            _proxy = owner.BuffProxy;
            _proxy.OnAttach += Attach;
            _proxy.OnLost += Lost;
        }

        private void Attach(BuffInfo info)
        {
            var item = Instantiate(itemPrefab, buffContainer);
            item.Init(info);
            itemDict.Add(info, item);
        }

        private void Lost(BuffInfo info)
        {
            Destroy(itemDict[info]);
            itemDict.Remove(info);
        }
    }
}
