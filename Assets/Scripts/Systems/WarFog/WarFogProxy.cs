using System;
using UnityEngine;
using System.Collections;

namespace Systems.WarFog
{
    public enum WarFogState
    {
        None,
        Partial,
        Full
    }
    
    public class WarFogProxy : MonoBehaviour
    {
        private WarFogState _currentState = WarFogState.None;
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Material _warFogMaterialInstance;

        private Vector2 _direction;
        private float _progress = -0.3f;
        
        void Awake()
        {
            ApplyShaderProperties();
            
            _warFogMaterialInstance = spriteRenderer.material;
            if (_warFogMaterialInstance == null || _warFogMaterialInstance.shader.name != "Pditine/WarFog")
            {
                Debug.LogError("The material assigned to SpriteRenderer does not use the 'Pditine/WarFog' shader or is null.", this);
                enabled = false;
                return;
            }
        }
        
        public void SetWarFogState(WarFogState newState, Vector2 direction)
        {
            _direction = direction;
            _currentState = newState;
            switch (_currentState)
            {
                case WarFogState.None:
                    // spriteRenderer.color = Color.clear;
                    StartCoroutine(DoWarFogDisappear());
                    break;
                case WarFogState.Partial:
                    // spriteRenderer.color = new Color(0, 0, 0, 0.5f);
                    break;
                case WarFogState.Full:
                    // spriteRenderer.color = Color.black;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    
        public void ApplyShaderProperties()
        {
            if (_warFogMaterialInstance == null) return;
    
            _warFogMaterialInstance.SetFloat("_Progress", _progress);
            var normalizedDirection = _direction.normalized;
            _warFogMaterialInstance.SetFloat("_DirectionX", normalizedDirection.x);
            _warFogMaterialInstance.SetFloat("_DirectionY", normalizedDirection.y);
        }

        // todo: 咱们应该有时间控制吧
        private IEnumerator DoWarFogDisappear()
        {
            ApplyShaderProperties();
            float duration = 1.0f;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _progress = Mathf.Clamp01(elapsed / duration);
                ApplyShaderProperties();
                yield return null;
            }
            _progress = 1.0f;
            ApplyShaderProperties();
        }
    }
}