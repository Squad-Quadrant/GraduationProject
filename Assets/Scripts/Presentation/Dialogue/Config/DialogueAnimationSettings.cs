using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Dialogue.Config
{
	[CreateAssetMenu(fileName = "DialogueAnimationSettings", menuName = "Game/Dialogue/Animation Settings")]
	public class DialogueAnimationSettings : ScriptableObject
	{
		[Title("Fade", bold: true)]
		[LabelText("淡入/淡出时长")]
		[Range(0.05f, 1f)]
		public float fadeDuration = 0.3f;

		[LabelText("淡入/淡出 Ease")]
		public Ease fadeEase = Ease.OutCubic;


		[Title("Slide", bold: true)]
		[LabelText("滑入/滑出时长")]
		[Range(0.05f, 1f)]
		public float slideDuration = 0.35f;

		[LabelText("滑动距离 (像素)")]
		[MinValue(0f)]
		public float slideDistance = 200f;

		[LabelText("滑入/滑出 Ease")]
		public Ease slideEase = Ease.OutCubic;


		[Title("Fade With Slight", bold: true)]
		[LabelText("轻微位移距离 (像素)")]
		[MinValue(0f)]
		public float slightOffset = 40f;
	}
}
