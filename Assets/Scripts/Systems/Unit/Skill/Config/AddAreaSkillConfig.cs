using PurpleFlowerCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Unit.Skill
{
    public abstract class AddAreaSkillConfig : SkillConfig
    {
        [LabelText("覆盖格相对偏移")]
        [Tooltip("相对 TargetCell（落点）的偏移，决定覆盖格形状。必须包含 (0,0) 才能包括落点本身。")]
        public Vector2Int[] coverageOffsets = { Vector2Int.zero };

        public int persistTurns = 2;
        
        [LabelText("持续区域特效")]
        public GameObject persistentVfxPrefab;
    }
}