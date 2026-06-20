using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Presentation.UI.Component
{
	[RequireComponent(typeof(Canvas))]
	public class CanvasMainCameraBinder : MonoBehaviour
	{
		[SerializeField, ReadOnly] private Canvas canvas;

		private void Reset() => canvas = GetComponent<Canvas>();

		private void Awake()
		{
			if (!canvas) canvas = GetComponent<Canvas>();
		}

		private void OnEnable()
		{
			BindToMainCamera();
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => BindToMainCamera();

		private void BindToMainCamera()
		{
			var mainCamera = Camera.main;
			if (!mainCamera) return;

			canvas.renderMode = RenderMode.ScreenSpaceCamera;
			canvas.worldCamera = mainCamera;
		}
	}
}
