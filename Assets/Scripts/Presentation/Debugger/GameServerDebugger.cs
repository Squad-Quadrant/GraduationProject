using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.GamePlay;
using UnityEngine;

namespace Presentation.Debugger
{
	[AddComponentMenu("Debugger/Game Server Debugger")]
	public class GameServerDebugger : MonoBehaviour
	{
		#region Connection

		[TitleGroup("Connection", order: -100)]
		[ShowInInspector, ReadOnly]
		[GUIColor("@IsConnected ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f)")]
		private bool IsConnected => _gameServer != null;

		[TitleGroup("Connection")]
		[ShowInInspector, ReadOnly, DisplayAsString]
		[HideIf("IsConnected")]
		private string ConnectionHint => "Waiting for Target...";

		#endregion

		[TitleGroup("Control Panel")]
		[Button("Start Game")]
		[EnableIf("@IsConnected")]
		private void StartGame()
		{
			_gameServer?.StartGame();
			Debug.Log("[GameServerDebugger] Start Game");
		}

		#region Private Fields

		private IGameServer _gameServer;

		#endregion

		#region Unity Lifecycle

		private void OnEnable()
		{
			if (Application.isPlaying)
				TryConnect();
		}

		private void Update()
		{
			if (Application.isPlaying && _gameServer == null)
				TryConnect();
		}

		private void OnDisable() => _gameServer = null;

		#endregion

		#region Connection

		private void TryConnect()
		{
			if (LevelContainer.Instance == null)
				return;

			_gameServer = LevelContainer.Instance.TryResolve<IGameServer>();
		}

		#endregion
	}
}
