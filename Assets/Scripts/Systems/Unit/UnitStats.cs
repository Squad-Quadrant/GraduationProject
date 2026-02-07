using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Unit
{
	/// <summary>
	/// Stable values that come from UnitConfig
	/// </summary>
	[Serializable]
	public class UnitStats
	{
		[LabelText("最大生命值")]
		public int maxHp;

		[Space]
		[LabelText("速度")]
		[InfoBox("影响单位的行动顺序，数值越高越先行动", InfoMessageType.None)]
		public int speed;

		[Space]
		[LabelText("移动力")]
		[InfoBox("单位每消耗一个行动点可以移动的格子数量", InfoMessageType.None)]
		public int moveRange;

		[Space]
		[LabelText("基本行动点")]
		[InfoBox("单位每回合可用的行动点数", InfoMessageType.None)]
		public int actionPoints;

		[Space]
		[LabelText("阵营")]
		public UnitFaction faction;

		public UnitStats Clone()
		{
			return new UnitStats
			{
				maxHp = maxHp,
				speed = speed,
				moveRange = moveRange,
				actionPoints = actionPoints,
				faction = faction
			};
		}
	}

    public enum UnitFaction
    {
        Player,
        Enemy,
        Neutral, 
        None, // 用于占位或特殊情况
    }
}
