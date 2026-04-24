using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Systems.Damage;
using UnityEngine;

namespace Presentation.UI.Panel.BodyPartPrompt
{
    public struct Scope
    {
        public Transform leftUp;
        public Transform rightDown;
    }
    public class BodyPartPrompt : SerializedMonoBehaviour
    {
        [OdinSerialize]
        public Dictionary<BodyPartType, List<Scope>> scopes = new();
        
        [SerializeField] private RectTransform StrongHit;
        [SerializeField] private RectTransform WeakHit;

        private List<RectTransform> _hitCache = new();

        public void Hit(BodyPartType bodyPartType, bool isStrong, int bulletCount)
        {
            if (!scopes.TryGetValue(bodyPartType, out var scopeList) || scopeList.Count == 0) return;

            foreach (var hit in _hitCache)
            {
                Destroy(hit.gameObject);
            }
            
            _hitCache.Clear();
            
            var prefab = isStrong ? StrongHit : WeakHit;
            if (prefab == null) return;
            var scope = scopeList[Random.Range(0, scopeList.Count)];

            for (int i = 0; i < bulletCount; i++)
            {
                
                float minX = Mathf.Min(scope.leftUp.position.x, scope.rightDown.position.x);
                float maxX = Mathf.Max(scope.leftUp.position.x, scope.rightDown.position.x);
                
                float minY = Mathf.Min(scope.leftUp.position.y, scope.rightDown.position.y);
                float maxY = Mathf.Max(scope.leftUp.position.y, scope.rightDown.position.y);

                float randomX = Random.Range(minX, maxX);
                float randomY = Random.Range(minY, maxY);

                var hitObj = Instantiate(prefab, transform);
                _hitCache.Add(hitObj);
                hitObj.position = new Vector3(randomX, randomY, hitObj.position.z);
            }
        }
    }
}