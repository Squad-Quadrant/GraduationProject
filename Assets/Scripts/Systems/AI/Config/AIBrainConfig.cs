using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.AI.Config
{
	[CreateAssetMenu(fileName = "AIBrainConfig", menuName = "Game/Unit/AI Brain Config")]
	public class AIBrainConfig : ScriptableObject
	{
		[FoldoutGroup("Movement"), Tooltip("移动动作的基础分数")]
		[Range(0f, 2f)]
        [LabelText("机动")]
		public float moveBase = 0.3f;

		[FoldoutGroup("Movement"), Tooltip("离敌人距离的权重")]
		[Range(0f, 1f)]
        [LabelText("感知")]
		public float closenessWeight = 0.2f;

		[FoldoutGroup("Movement"), Tooltip("有敌人在攻击范围的权重")]
		[Range(0f, 1f)]
        [LabelText("警觉")]
		public float inRangeBonus = 0.2f;

		[FoldoutGroup("Movement")]
		[Range(0f, 0.5f)]
        [LabelText("战术-游击")]
		public float apConservationBonus = 0.1f;


		[FoldoutGroup("Wait")]
		[Range(0f, 0.5f)]
        [LabelText("犹豫")]
		public float waitScore = 0.1f;
        
        [FoldoutGroup("Attack"), Tooltip("攻击动作的基础分数")]
        [Range(0f, 2f)]
        [LabelText("勇敢")]
        public float attackBase = 0.6f;

        [FoldoutGroup("Attack"), Tooltip("损失的血量占总血量的比例的影响, 如剩余30%血，则总分数-【该值*0.7】")]
        [Range(0f, 1f)]
        [LabelText("恐惧")]
        public float fear = 0.5f;
        
        [FoldoutGroup("Attack"), Tooltip("某敌人损失的血量占总血量的比例的影响, 如敌人剩余30%血，则总分数+【该值*0.7】")]
        [Range(0f, 1f)]
        [LabelText("杀意")]
        public float killAwareness = 0.5f;
        
        [FoldoutGroup("Attack"), Tooltip("敌人距离的影响,总分数-[该值*距敌人曼哈顿距离]")]
        [Range(0f, 0.1f)]
        [LabelText("迟钝")]
        public float bluntness = 0.01f;
        
        [FoldoutGroup("Attack"), Tooltip("剩余ap的影响,总分数+[该值*剩余ap]")]
        [Range(0f, 1f)]
        [LabelText("战术-强攻")]
        public float tacticsAttack = 0.1f;
        
        [FoldoutGroup("Reload"), Tooltip("换弹动作的基础分数")]
        [Range(0f, 2f)]
        [LabelText("换弹癌")]
        public float reloadBase = 0.3f;
        
        [FoldoutGroup("Reload"), Tooltip("已消耗子弹的影响,如消耗了30%的子弹,则总分数+[0.3*该值]")]
        [Range(0f, 2f)]
        [LabelText("空仓焦虑")]
        public float ammoAnxiety = 1f;
        
        [FoldoutGroup("Reload"), Tooltip("剩余ap的影响,总分数+[该值*剩余ap]")]
        [Range(0f, 1f)]
        [LabelText("战术-持久战")]
        public float tacticsReload = 0.1f;
    }
}
