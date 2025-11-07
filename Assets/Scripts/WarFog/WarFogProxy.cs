using System;
using UnityEngine;

namespace WarFog
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
        [SerializeField] private SpriteRenderer _spriteRenderer;
        public void SetWarFogState(WarFogState newState)
        {
            _currentState = newState;
            // todo: temp, 需要溶解上层地图的美术效果
            switch (_currentState)
            {
                case WarFogState.None:
                    _spriteRenderer.color = Color.clear;
                    break;
                case WarFogState.Partial:
                    _spriteRenderer.color = new Color(0, 0, 0, 0.5f);
                    break;
                case WarFogState.Full:
                    _spriteRenderer.color = Color.black;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}