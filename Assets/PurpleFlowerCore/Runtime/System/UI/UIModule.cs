using System;
using System.Collections.Generic;
using UnityEngine;

namespace PurpleFlowerCore
{
    public class UIModule : MonoBehaviour
    {
        private readonly List<Transform> _uiLayers = new();

        private void OnEnable()
        {
            EventSystem.AddEventListener(PFCEvent.LoadScene, DestroyThis);
        }
        
        private void OnDisable()
        {
            EventSystem.RemoveEventListener(PFCEvent.LoadScene, DestroyThis);
        }

        public void DestroyThis()
        {
            Destroy(gameObject);
        }

        public Transform GetUILayer(int index = 0)
        {
            if (index < _uiLayers.Count)
            {
                return _uiLayers[index];
            }
            Transform newLayer = new GameObject("UILayer" + index).transform;
            newLayer.SetParent(transform);
            newLayer.localPosition = Vector3.zero;
            newLayer.localScale = Vector3.one;
            newLayer.localRotation = Quaternion.identity;
            _uiLayers.Add(newLayer);
            newLayer.SetSiblingIndex(index);
            return newLayer;
        }
    }
}