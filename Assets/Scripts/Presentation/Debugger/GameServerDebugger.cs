using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.BattleFlow;
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
		private bool IsConnected => _battleServer != null;

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
			_battleServer?.StartBattle();
			Debug.Log("[GameServerDebugger] Start Game");
		}

		#region Private Fields

		private IBattleServer _battleServer;

		#endregion

		#region Unity Lifecycle

		private void OnEnable()
		{
			if (Application.isPlaying)
				TryConnect();
		}

		private void Update()
		{
			if (Application.isPlaying && _battleServer == null)
				TryConnect();
		}

		private void OnDisable() => _battleServer = null;

		#endregion

		#region Connection

		private void TryConnect()
		{
			if (LevelContainer.Instance == null)
				return;

			_battleServer = LevelContainer.Instance.TryResolve<IBattleServer>();
		}

		#endregion
	}
}
