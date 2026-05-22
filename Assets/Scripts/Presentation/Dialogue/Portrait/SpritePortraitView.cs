using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Dialogue.Portrait
{
	public class SpritePortraitView : PortraitViewBase
	{
		[Serializable]
		public class SpritePoseEntry
		{
			public string poseName;

			[PreviewField(48, ObjectFieldAlignment.Left)]
			public Sprite sprite;
		}

		[TitleGroup("Sprite References")]
		[SerializeField, Required, ChildGameObjectsOnly]
		private Image image;

		[SerializeField]
		[TableList(ShowIndexLabels = false, AlwaysExpanded = true, DrawScrollView = false)]
		private List<SpritePoseEntry> poses = new();

		protected override void ApplyIdle(string poseName, string skinName)
		{
			if (!image) return;

			var sprite = LookupSprite(poseName);
			if (sprite) image.sprite = sprite;
		}

		protected override void PlayOneShotThenIdle(string oneShotAnim, string followingPose, string followingSkin)
		{
			if (!string.IsNullOrEmpty(oneShotAnim))
				this.LogWarning($"OneShot '{oneShotAnim}' not supported on Sprite portrait. Switching directly to '{followingPose}'.");

			ApplyIdle(followingPose, followingSkin);
		}

		private Sprite LookupSprite(string poseName)
		{
			if (string.IsNullOrEmpty(poseName)) return null;

			foreach (var entry in poses.Where(entry => entry.poseName == poseName))
				return entry.sprite;

			this.LogWarning($"Pose '{poseName}' not found in sprite poses list.");
			return null;
		}
	}
}
