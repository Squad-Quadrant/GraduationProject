using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Presentation.UI.Panel.Menu
{
	public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private GameObject hoverImg;

		private void Start() => hoverImg.SetActive(false);

		public void OnPointerEnter(PointerEventData eventData) => hoverImg.SetActive(true);

		public void OnPointerExit(PointerEventData eventData) => hoverImg.SetActive(false);
	}
}
