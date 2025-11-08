using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PurpleFlowerCore.Utility
{
    public static class ScaleUtility
    {
        private static HashSet<Transform> transformBuffer = new();

        private static void AddBuffer(Transform transform)
        {
            transformBuffer.Add(transform);
        }

        private static void RemoveBuffer(Transform transform)
        {
            if(!transformBuffer.Contains(transform)) return;
            transformBuffer.Remove(transform);
        }
        
        public static bool Lerp(Vector3 target, Transform transform, float speed, UnityAction callBack = null)
        {
            if (transformBuffer.Contains(transform)) return false;
            AddBuffer(transform);
            MonoSystem.Start_Coroutine(DoLerp(target, transform, speed, callBack));
            return true;
        }

        private static IEnumerator DoLerp(Vector3 target, Transform transform, float speed, UnityAction callBack)
        {
            while (Mathf.Abs(transform.localScale.x-target.x)>0.01f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, target, 0.02f);
                yield return new WaitForSeconds(1/speed);
            }
            transform.localScale = target;
            RemoveBuffer(transform);
            callBack?.Invoke();
            
        }
        
        public static bool MoveTowards(Vector3 target, Transform transform, float speed, UnityAction callBack = null)
        {
            if (transformBuffer.Contains(transform)) return false;
            AddBuffer(transform);
            MonoSystem.Start_Coroutine(DoMoveTowards(target, transform, speed, callBack));
            return true;
        }
        
        private static IEnumerator DoMoveTowards(Vector3 target, Transform transform, float speed, UnityAction callBack)
        {
            while (Vector3.Distance(transform.localScale, target)>0.01f)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, target, 0.02f);
                yield return new WaitForSeconds(1/speed);
            }
            transform.localScale = target;
            RemoveBuffer(transform);
            callBack?.Invoke();
        }
    }
}