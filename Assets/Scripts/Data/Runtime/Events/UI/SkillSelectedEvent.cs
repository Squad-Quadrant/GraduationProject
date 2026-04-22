using Core.Events;

namespace Data.Runtime.Events.UI
{
	// 当前每单位只有一个技能，所有AbilitySelectionState直接从context.selectedUnit.Skill取Logic
	// 要是以后要加再说
	public readonly struct SkillSelectedEvent : IEvent
	{
		public override string ToString() => "[SkillSelected]";
	}
}
