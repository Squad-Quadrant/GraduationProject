using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Test.WZHTest
{
	public class Menu : MonoBehaviour
	{
		[SerializeField] private Button startButton;
		[SerializeField] private string levelScene;

		private void OnEnable()
		{
			startButton.onClick.AddListener(() => SceneManager.LoadScene(levelScene));
		}
	}
}
