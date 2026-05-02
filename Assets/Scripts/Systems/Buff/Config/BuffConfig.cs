using PurpleFlowerCore;
using Sirenix.OdinInspector;
using Systems.Buff.Influence;
using UnityEngine;

namespace Systems.Buff.Config
{
    /// <summary>
    /// Buff增加时的更新方式
    /// </summary>
    public enum BuffAttachType
    {
        [InspectorName("增加层数")]
        Add,
        [InspectorName("重置时间")]
        Override,
        [InspectorName("无法叠加")]
        Keep
    }

    /// <summary>
    /// Buff时间到时的更新方式
    /// </summary>
    public enum BuffLostType
    {
        [InspectorName("不消失")]
        None,
        [InspectorName("减少一层")]
        Reduce,
        [InspectorName("全部清除")]
        Clear
    }
    
    public enum BuffType
    {
        [InspectorName("请选择类型")]
        None,
        [InspectorName("手部骨折")]
        HandFracture,
        [InspectorName("腿部骨折")]
        LegFracture,
        [InspectorName("中毒")]
        Toxicosis,
        [InspectorName("失明")]
        Blind,
        [InspectorName("燃烧")]
        Burn,
        [InspectorName("眩晕")]
        Dizzy,
        [InspectorName("流血")]
        Bleed,
        [InspectorName("虚弱")]
        Weak
    }

    [Configurable("Buff/BuffData")]
    [CreateAssetMenu(fileName = "BuffData", menuName = "Game/Buff/BuffData")]
    public class BuffData : ScriptableObject
    {
        [Title("Base Info")]
        public BuffType buffType;
        public string buffName;
        public string description;
        public Sprite icon;
        public int maxStack = 1;
        public string[] tags;
        public bool showInUI;

        [Title("Time Info")]
        // public bool willLastForever;
        public int durationTurn = -1; // 大于等于100则认为持续时间无限
        // public float tickTime;

        [Title("Update Type")]
        public BuffAttachType attachType;
        public BuffLostType lostType;

        [Title("Callback Event")]
        // [InlineEditor] public BuffEvent[] onInitEvents;
        [InlineEditor] public BuffEvent[] onAttachEvents; // 附加buff时调用
        [InlineEditor] public BuffEvent[] onLostEvents; // 移除buff时调用
        [InlineEditor] public BuffEvent[] onTurnEvents; // 每回合调用
        [InlineEditor] public BuffEvent[] onResetEvents; // 重制时调用
        [InlineEditor] public BuffInfluence[] propertyInfluences; // 属性影响
    }
}
