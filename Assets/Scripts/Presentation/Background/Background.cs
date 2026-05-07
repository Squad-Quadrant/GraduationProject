using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Presentation.Background
{
	public class Background : MonoBehaviour
	{
		[TitleGroup("References")]
		[SerializeField, Required] private RawImage rawImage;
		[SerializeField, Required] private Canvas canvas;

		[TitleGroup("Response Shape")]
		[SerializeField, Range(0f, 0.6f), LabelText("Dead Zone Radius")]
		[Tooltip("鼠标距屏幕中心归一化距离小于此值时不响应。0.3 表示屏幕中心 30% 半径是死区。")]
		private float deadZoneRadius = 0.3f;

		[SerializeField, Range(0f, 0.2f), LabelText("Max UV Offset")]
		[Tooltip("鼠标到屏幕四角时的最大 UV 偏移量。0.05 = 5% 屏幕尺寸，hub 感最佳区间 0.03~0.08。")]
		private float maxOffset = 0.05f;

		[TitleGroup("Response Shape")]
		[SerializeField, LabelText("Invert Direction")]
		[Tooltip("默认关闭：鼠标向右 → 背景向右（跟随视差）。开启后：鼠标向右 → 背景向左（窗户反视差）。")]
		private bool invert = false;

		[TitleGroup("Smoothing")]
		[SerializeField, Range(0.05f, 2f), LabelText("Smooth Time")]
		[Tooltip("SmoothDamp 的平滑时间，数字越大偏移跟随越慢，拖影越长。0.3 秒是舒适值。")]
		private float smoothTime = 0.3f;

		private static readonly int OffsetPropertyId = Shader.PropertyToID("_ParallaxOffset");

		private Material _runtimeMaterial;
		private Vector2 _currentOffset;
		private Vector2 _velocity;

		private void Reset()
		{
			rawImage = GetComponent<RawImage>();
			canvas = GetComponentInParent<Canvas>();
		}

		private void Awake()
		{
			if (!rawImage) rawImage = GetComponent<RawImage>();

			if (!rawImage.material) return;
			_runtimeMaterial = new Material(rawImage.material);
			rawImage.material = _runtimeMaterial;
		}

		private void OnDestroy()
		{
			if (_runtimeMaterial)
				Destroy(_runtimeMaterial);
		}

		private void Update()
		{
			if (!_runtimeMaterial) return;

			var mouse = Mouse.current;
			if (mouse == null) return;

			var mousePixel = mouse.position.ReadValue();
			var screenSize = new Vector2(Screen.width, Screen.height);
			if (screenSize.x <= 0 || screenSize.y <= 0) return;
			var mouseNorm = (mousePixel / screenSize) * 2f - Vector2.one;

			var absN = new Vector2(Mathf.Abs(mouseNorm.x), Mathf.Abs(mouseNorm.y));
			float dist = Mathf.Max(absN.x, absN.y);
			float t = Mathf.InverseLerp(deadZoneRadius, 1f, dist);
			t = Mathf.SmoothStep(0f, 1f, t);

			var targetOffset = mouseNorm * t * maxOffset;
			if (invert) targetOffset = -targetOffset;

			_currentOffset = Vector2.SmoothDamp(
				_currentOffset,
				targetOffset,
				ref _velocity,
				smoothTime);

			_runtimeMaterial.SetVector(OffsetPropertyId, _currentOffset);
		}
	}
}
