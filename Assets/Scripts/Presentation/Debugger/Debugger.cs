using UnityEngine;

namespace Presentation.Debugger
{
	public class Debugger : MonoBehaviour
	{
		private void Awake() => DontDestroyOnLoad(gameObject);
	}
}
