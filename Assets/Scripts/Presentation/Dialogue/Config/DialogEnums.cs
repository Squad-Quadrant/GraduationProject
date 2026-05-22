using UnityEngine;

namespace Presentation.Dialogue.Config
{
	public enum EPortraitMode
	{
		[InspectorName("Spine 骨骼动画")] Spine  = 0,
		[InspectorName("静态图片")]       Sprite = 1,
	}

	public enum EPortraitPosition
	{
		[InspectorName("左")] Left = 0,
		[InspectorName("中")] Center = 1,
		[InspectorName("右")] Right = 2,
	}

	public enum EEntranceStyle
	{
		[InspectorName("瞬切")]           Cut = 0,
		[InspectorName("淡入")]           Fade = 1,
		[InspectorName("从侧方滑入")]      SlideFromSide = 2,
		[InspectorName("淡入+轻微位移")]   FadeWithSlight = 3,
	}

	public enum EExitStyle
	{
		[InspectorName("瞬切")]           Cut = 0,
		[InspectorName("淡出")]           Fade = 1,
		[InspectorName("向侧方滑出")]      SlideToSide = 2,
		[InspectorName("淡出+轻微位移")]   FadeWithSlight = 3,
	}
}
