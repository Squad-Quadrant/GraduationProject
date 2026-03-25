using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.CameraControl
{
	[CreateAssetMenu(fileName = "CameraConfig", menuName = "Game/Camera Config")]
	public class CameraConfig : ScriptableObject
	{
		[FoldoutGroup("Focus")] // 自动聚焦相关
		[SuffixLabel("sec"), Range(0.1f, 2f)]
		public float focusDuration = 0.4f;

		[FoldoutGroup("Focus")]
		public Ease focusEase = Ease.OutCubic;

		[FoldoutGroup("Pan")]
		[InfoBox("鼠标拖拽速度")]
		[Range(1f, 30f)]
		public float dragSpeed = 10f;

		[FoldoutGroup("Pan")]
		[InfoBox("WASD移动速度")]
		[Range(0.1f, 5f)]
		public float panSpeed = 1f;

		[FoldoutGroup("Pan")]
		[Range(3f, 30f)]
		public float panSmoothing = 8f;

		[FoldoutGroup("Zoom")]
		[MinMaxSlider(1f, 20f, true)]
		public Vector2 zoomRange = new(3f, 10f);
		public float ZoomMin => zoomRange.x;
		public float ZoomMax => zoomRange.y;

		[FoldoutGroup("Zoom")]
		[Range(0.1f, 5f)]
		public float zoomSensitivity = 1f;

		[FoldoutGroup("Zoom")]
		[Range(1f, 30f)]
		public float zoomSmoothSpeed = 10f;

		[FoldoutGroup("Boundary")]
		[SuffixLabel("units"), Range(0f, 5f)]
		public float boundaryPadding = 1f;

		[FoldoutGroup("Discovery")]
		[SuffixLabel("sec"), Range(0.1f, 1f)]
		public float discoveryFocusDuration = 0.3f;

		[FoldoutGroup("Discovery")]
		[SuffixLabel("sec"), Range(0.1f, 2f)]
		public float discoveryDwellDuration = 0.6f;

		[FoldoutGroup("Discovery")]
		public Ease discoveryEase = Ease.OutCubic;

		[FoldoutGroup("Shake")]
		[SuffixLabel("sec"), Range(0.05f, 1f)]
		public float shakeDuration = 0.2f;

		[FoldoutGroup("Shake")]
		[Range(0.01f, 1f)]
		public float shakeStrength = 0.15f;
	}
}
