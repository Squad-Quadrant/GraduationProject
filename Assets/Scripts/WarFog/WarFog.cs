using UnityEngine;

namespace WarFog
{
    public enum WarFogState
    {
        None,
        Partial,
        Full
    }
    
    public class WarFog : MonoBehaviour
    {
        public WarFogState currentState = WarFogState.None;
    }
}