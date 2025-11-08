using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PurpleFlowerCore.Utility
{
    public static class FadeUtility
    {
        private static HashSet<Object> transformBuffer = new();

        private static void AddBuffer(Object transform)
        {
            transformBuffer.Add(transform);
        }

        private static void RemoveBuffer(Object transform)
        {
            if(!transformBuffer.Contains(transform)) return;
            transformBuffer.Remove(transform);
        }
        public static bool FadeOut(Graphic graphic,float speed, UnityAction callBack = null, float alpha = 1)
        {
            //if (transformBuffer.Contains(graphic)) return false;
            AddBuffer(graphic);
            MonoSystem.Start_Coroutine(DoFadeOut(graphic,speed,callBack,alpha));
            return true;
        }

        private static IEnumerator DoFadeOut(Graphic graphic,float speed, UnityAction callBack,float alpha)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            graphic.enabled = true;
            while (graphic.color.a>0.05f)
            {
                yield return new WaitForSeconds(1/speed);
                graphic.color -= new Color(0, 0, 0, 0.01f);
            }
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0);
            graphic.enabled = false;
            callBack?.Invoke();
        }
        
        public static bool FadeOut(CanvasGroup graphic,float speed, UnityAction callBack = null, float alpha = 1)
        {
            //if (transformBuffer.Contains(graphic)) return false;
            AddBuffer(graphic);
            MonoSystem.Start_Coroutine(DoFadeOut(graphic,speed,callBack,alpha));
            return true;
        }

        private static IEnumerator DoFadeOut(CanvasGroup graphic,float speed, UnityAction callBack,float alpha)
        {
            graphic.alpha = alpha;
            graphic.enabled = true;
            while (graphic.alpha>0.05f)
            {
                yield return new WaitForSeconds(1/speed);
                graphic.alpha -= 0.01f;
            }

            graphic.alpha = 0;
            // graphic.enabled = false;
            callBack?.Invoke();
        }
        
        public static bool FadeOutTo(Graphic graphic,float speed, UnityAction callBack = null, float alpha = 1)
        {
            //if (transformBuffer.Contains(graphic)) return false;
            AddBuffer(graphic);
            MonoSystem.Start_Coroutine(DoFadeOutTo(graphic,speed,callBack,alpha));
            return true;
        }

        private static IEnumerator DoFadeOutTo(Graphic graphic,float speed, UnityAction callBack,float alpha)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 1);
            graphic.enabled = true;
            while (graphic.color.a>alpha+0.05f)
            {
                yield return new WaitForSeconds(1/speed);
                graphic.color -= new Color(0, 0, 0, 0.01f);
            }
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            callBack?.Invoke();
        }
        
        public static bool FadeOut(SpriteRenderer graphic,float speed, UnityAction callBack = null, float alpha = 1)
        {
            //if (transformBuffer.Contains(graphic)) return false;
            AddBuffer(graphic);
            MonoSystem.Start_Coroutine(DoFadeOut(graphic,speed,callBack,alpha));
            return true;
        }

        private static IEnumerator DoFadeOut(SpriteRenderer graphic,float speed, UnityAction callBack,float alpha)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            graphic.enabled = true;
            while (graphic.color.a>0.05f)
            {
                yield return new WaitForSeconds(1/speed);
                graphic.color -= new Color(0, 0, 0, 0.01f);
            }
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0);
            graphic.enabled = false;
            callBack?.Invoke();
        }
        
        public static bool FadeIn(Graphic graphic,float speed, UnityAction callBack = null, float alpha = 1)
        {
            //if (transformBuffer.Contains(graphic)) return false;
            AddBuffer(graphic);
            MonoSystem.Start_Coroutine(DoFadeIn(graphic,speed,callBack,alpha));
            return true;
        }

        private static IEnumerator DoFadeIn(Graphic graphic,float speed, UnityAction callBack, float alpha)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0);
            graphic.enabled = true;
            while (graphic.color.a<alpha-0.05f)
            {
                yield return new WaitForSeconds(1/speed);
                graphic.color += new Color(0, 0, 0, 0.01f);
            }
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            graphic.enabled = false;
            callBack?.Invoke();
        }
        public static bool FadeInAndStay(Graphic graphic,float speed, UnityAction callBack = null, float alpha = 1)
        {
            //if (transformBuffer.Contains(graphic)) return false;
            AddBuffer(graphic);
            MonoSystem.Start_Coroutine(DoFadeInAndStay(graphic,speed,callBack,alpha));
            return true;
        }

        private static IEnumerator DoFadeInAndStay(Graphic graphic,float speed, UnityAction callBack, float alpha)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0);
            graphic.enabled = true;
            while (graphic.color.a<alpha - 0.05f)
            {
                yield return new WaitForSeconds(1/speed);
                graphic.color += new Color(0, 0, 0, 0.01f);
            }
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            callBack?.Invoke();
        }
        
        public static bool FadeInAndStay(SpriteRenderer graphic,float speed, UnityAction callBack = null, float alpha = 1)
        {
            //if (transformBuffer.Contains(graphic)) return false;
            AddBuffer(graphic);
            MonoSystem.Start_Coroutine(DoFadeInAndStay(graphic,speed,callBack,alpha));
            return true;
        }
        
        private static IEnumerator DoFadeInAndStay(SpriteRenderer graphic,float speed, UnityAction callBack, float alpha)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0);
            graphic.enabled = true;
            while (graphic.color.a<alpha - 0.05f)
            {
                yield return new WaitForSeconds(1/speed);
                graphic.color += new Color(0, 0, 0, 0.01f);
            }
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            callBack?.Invoke();
        }
        
        public static bool FadeInAndStay(CanvasGroup graphic,float speed, UnityAction callBack = null, float alpha = 1)
        {
            //if (transformBuffer.Contains(graphic)) return false;
            AddBuffer(graphic);
            MonoSystem.Start_Coroutine(DoFadeInAndStay(graphic,speed,callBack,alpha));
            return true;
        }

        private static IEnumerator DoFadeInAndStay(CanvasGroup graphic,float speed, UnityAction callBack, float alpha)
        {
            graphic.alpha = 0;
            graphic.enabled = true;
            while (graphic.alpha<alpha - 0.05f)
            {
                yield return new WaitForSeconds(1/speed);
                graphic.alpha += 0.01f;
            }
            graphic.alpha = alpha;
            callBack?.Invoke();
        }
    }
}