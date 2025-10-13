using System;
using UnityEngine;

namespace PurpleFlowerCore
{
    public abstract class UINode: MonoBehaviour
    {
#if UNITY_EDITOR
        [HideInInspector] public Color TagColor = Color.white;
#endif
        [HideInInspector] public String NodeName = "";

        public virtual bool IsShow => gameObject.activeSelf;
        private void Awake()
        {
            InitEvent();
            Register();
        }

        /// <summary>
        /// Do not call or override this method manually
        /// </summary>
        protected virtual void InitEvent()
        {
            
        }
        
        

        protected virtual void Register()
        {
            UISystem.RegisterUI(string.IsNullOrEmpty(NodeName)?name: NodeName,this);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public virtual void Switch(bool isShow)
        {
            if (isShow)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}