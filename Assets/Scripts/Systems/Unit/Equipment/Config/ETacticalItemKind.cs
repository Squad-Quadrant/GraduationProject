namespace Systems.Unit.Equipment.Config
{
	public enum ETacticalItemKind
	{
		InstantMedpack,     // 即时医疗：直接作用于自己
		Grenade,            // 投掷：落点立即爆炸造成范围伤害
		Burn,               // 投掷：落点生成燃烧区域（AreaEffect + BuffAttach）
		TimerBomb,          // 投掷：落点生成定时炸弹（AreaEffect + 倒计时爆炸）
		ScoutEye,           // 投掷：落点生成侦察眼（AreaEffect + 持续透视）
	}
}
