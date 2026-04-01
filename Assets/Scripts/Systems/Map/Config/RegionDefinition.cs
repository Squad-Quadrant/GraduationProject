using System;
using Sirenix.OdinInspector;

namespace Systems.Map.Config
{
	[Serializable]
	public class RegionDefinition
	{
		[HorizontalGroup("Main", Width = 50)]
		[LabelText("ID"), LabelWidth(20)]
		public int regionId;

		[HorizontalGroup("Main")]
		[LabelText("名称"), LabelWidth(30)]
		public string regionName;

		[HorizontalGroup("Main", Width = 80)]
		[LabelText("初始解锁"), LabelWidth(55)]
		public bool initiallyUnlocked;

		public RegionDefinition(int id, string name, bool unlocked)
		{
			regionId = id;
			regionName = name;
			initiallyUnlocked = unlocked;
		}

		/// <summary>
		/// The default outdoor region (id=0), always starts unlocked.
		/// </summary>
		public static RegionDefinition DefaultOutdoor => new(0, "户外", true);
	}
}
