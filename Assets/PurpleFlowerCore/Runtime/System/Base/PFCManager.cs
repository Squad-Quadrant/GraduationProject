using UnityEngine;
using UnityEngine.UI;

namespace PurpleFlowerCore.Base
{
    public class PFCManager : MonoBehaviour
    {
        // private readonly Dictionary<Type,> _modules = new();
        // public static Dictionary<Type, MonoBehaviour> Modules => Instance._modules;

        private static PFCManager _instance;
        public static PFCManager Instance
        {
            get
            {
                if (_instance is not null) return _instance;
                GameObject pfcGameObject = new GameObject
                {
                    name = "PurpleFlowerCore"
                };
                _instance = pfcGameObject.AddComponent<PFCManager>();
                DontDestroyOnLoad(pfcGameObject);
                return _instance;
            }
        }

        private static Canvas _canvas;
        public static Canvas Canvas
        {
            get
            {
                if (_canvas is not null) return _canvas;
                GameObject canvasGameObject = new GameObject
                {
                    name = "Canvas"
                };
                _canvas = canvasGameObject.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGameObject.AddComponent<CanvasScaler>();
                canvasGameObject.AddComponent<GraphicRaycaster>();
                canvasGameObject.transform.parent = Instance.transform;
                return _canvas;
            }
        }
        
        //todo:add system
        // public void AddSystem()
        // {
        //     
        // }
        
        private void Awake()
        {
            if(_instance is not null)
                Destroy(_instance.gameObject);
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDisable()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}