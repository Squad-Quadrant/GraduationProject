using System;
using System.Collections;
using System.Collections.Generic;
using Core.Log;
using Data.Config;
using Data.Runtime.Events.Vision;
using Presentation.Input;
using Sirenix.OdinInspector;
using Spine.Unity;
using Systems.Interfaces;
using UnityEngine;

namespace Presentation.Unit
{
	/// <summary>
	/// Unit在场景中的实体
	/// </summary>
	public class UnitView : MonoBehaviour, IClickableUnit
	{
		[TitleGroup("Settings")]
		[SerializeField] private float moveSpeed = 3f;

		[TitleGroup("References")]
		[SerializeField, Required, ChildGameObjectsOnly]
		private UnitAnimator animator;
		private MeshRenderer _renderer;

		private UnitAnimationConfig _config;
		private ICoordinateConverter _coordConverter;

		private EUnitStance _stance;
		private EGripType _grip;
		private bool _facingRight;

		private string _frontBodySkinName;
		private string _backBodySkinName;

		private Coroutine _moveCoroutine;

		#region IClickableUnit

		public string UnitId { get; private set; }

		#endregion

		public void Initialize(
			string unitId,
			UnitAnimationConfig config,
			ICoordinateConverter coordConverter,
			SkeletonDataAsset skeletonDataAsset,
			string frontBodySkinName,
			string backBodySkinName,
			Vector2Int initialGridPos,
			string weaponSkinName = null)
		{
			UnitId = unitId;
			_config = config;
			_coordConverter = coordConverter;
			_frontBodySkinName = frontBodySkinName;
			_backBodySkinName = backBodySkinName;

			animator.Initialize(skeletonDataAsset, frontBodySkinName, weaponSkinName);
			_renderer = animator.GetComponent<MeshRenderer>();

			_stance = config.DefaultStance;
			_grip = config.DefaultGrip;
			_facingRight = true;

			transform.position = _coordConverter.CellToWorld(initialGridPos);
			transform.position = new Vector3(transform.position.x, transform.position.y, 0f); // ensure z=0 for correct sorting

			PlayAction("idle");

			this.Log($"UnitView initialized at grid {initialGridPos} (world {transform.position}) with body skins '{frontBodySkinName}' and '{backBodySkinName}'");
		}

		private void OnDestroy()
		{
			if (_moveCoroutine == null) return;
			StopCoroutine(_moveCoroutine);
			_moveCoroutine = null;
		}

		// onComplete will be called even if the animation fails to play (e.g. not found)
		public void PlayAction(string action, Action onComplete = null)
		{
			if (action == "idle")
			{
				var idleSet = _config.GetIdleSet(_stance, _grip);
				if (idleSet.HasFidgets)
				{
					animator.PlayLoopWithFidgets(
						idleSet.BaseClips, idleSet.FidgetClips,
						_config.FidgetMinDelay, _config.FidgetMaxDelay);
					onComplete?.Invoke(); // Loop has no natural "complete"
					return;
				}
			}

			var result = _config.GetAnimation(action, _stance, _grip);
			if (!result.IsValid)
			{
				this.LogError($"No animation found for action '{action}' with stance '{_stance}' and grip '{_grip}'");
				onComplete?.Invoke();
				return;
			}

			animator.Play(result.ClipName, result.Loop, onComplete);
		}

		// If a transition animation exists in config, plays it first
		public void SetStance(EUnitStance newStance, Action onComplete = null)
		{
			if (_stance == newStance)
			{
				onComplete?.Invoke();
				return;
			}

			var transition = _config.GetTransition(_stance, newStance, _grip);
			if (!string.IsNullOrEmpty(transition))
			{
				animator.Play(transition, false, () =>
				{
					_stance = newStance;
					PlayAction("idle");
					onComplete?.Invoke();
				});
			}
			else
			{
				_stance = newStance;
				PlayAction("idle");
				onComplete?.Invoke();
			}
		}

		public void SetGrip(EGripType newGrip)
		{
			if (_grip == newGrip) return;
			_grip = newGrip;
			if (_moveCoroutine == null)
				PlayAction("idle");
		}

		public void SetFacing(Vector2Int direction)
		{
			if (direction == Vector2Int.zero) return;

			var targetSkin = (direction.x < 0 || direction.y < 0) ? _frontBodySkinName : _backBodySkinName;
			animator.SetBodySkin(targetSkin);

			_facingRight = direction.x > 0 || direction.y < 0;
			animator.SetFaceRight(_facingRight);
		}

		public void SetWeaponSkin(string skinName) => animator.SetWeaponSkin(skinName);

		public void Move(IReadOnlyList<Vector2Int> path, Action<Vector2Int> onStep = null, Action onComplete = null)
		{
			if (path == null || path.Count < 2)
			{
				this.LogError("Invalid path for movement.");
				onComplete?.Invoke();
				return;
			}

			if (_moveCoroutine != null)
			{
				StopCoroutine(_moveCoroutine);
				_moveCoroutine = null;
				this.LogWarning("Interrupted ongoing movement.");
			}

			_moveCoroutine = StartCoroutine(MoveCoroutine(path, onStep, onComplete));
		}

		public void CancelMovement()
		{
			if (_moveCoroutine == null) return;

			StopCoroutine(_moveCoroutine);
			_moveCoroutine = null;
			PlayAction("idle");
			this.Log("Movement cancelled.");
		}

		public void Pause() => animator.Pause();
		public void Resume() => animator.Resume();

		public void SetVisible(bool visible) => _renderer.enabled = visible;
		public bool GetVisible() => _renderer.enabled;

		private IEnumerator MoveCoroutine(IReadOnlyList<Vector2Int> path, Action<Vector2Int> onStep, Action onComplete)
		{
			if (path == null || path.Count < 2)
			{
				this.LogError("Invalid path for movement.");
				onComplete?.Invoke();
				yield break;
			}

			SetFacing(path[1] - path[0]);

			bool startDone = false;
			bool moveSignal = false;
			PlayAction("move_start", () =>
			{
				startDone = true;
				moveSignal = true;
			});
			animator.ListenForSpineEvent("move_start", () => moveSignal = true);
			while (!moveSignal) yield return null;

			int lastIdx = path.Count - 1;
			bool switchedToLoop = false;

			for (int i = 1; i < lastIdx; i++)
			{
				if (startDone && !switchedToLoop)
				{
					PlayAction("move_loop");
					switchedToLoop = true;
				}

				SetFacing(path[i] - path[i - 1]);
				yield return LerpSegment(path[i - 1], path[i]);
				onStep?.Invoke(path[i]);
			}

			SetFacing(path[lastIdx] - path[lastIdx - 1]);

			bool stopSignal = false;
			bool endDone = false;

			PlayAction("move_end", () =>
			{
				endDone = true;
				stopSignal = true;
			});
			animator.ListenForSpineEvent("move_stop", () => stopSignal = true);

			// Lerp the final segment — stop early if move_stop event fires
			var fromWorld = _coordConverter.CellToWorld(path[lastIdx - 1]);
			var toWorld = _coordConverter.CellToWorld(path[lastIdx]);
			var dist = Vector3.Distance(fromWorld, toWorld);
			var duration = dist > 0.001f ? dist / moveSpeed : 0f;
			var elapsed = 0f;

			while (elapsed < duration && !stopSignal)
			{
				elapsed += Time.deltaTime;
				var t = Mathf.Clamp01(elapsed / duration);
				transform.position = Vector3.Lerp(fromWorld, toWorld, t);
				yield return null;
			}

			// Snap to exact final position
			transform.position = toWorld;
			onStep?.Invoke(path[lastIdx]);

			while (!endDone) yield return null; // Wait for move_end animation to finish

			PlayAction("idle");
			_moveCoroutine = null;
			onComplete?.Invoke();

			this.Log($"Move complete → {path[lastIdx]}");
		}

		private IEnumerator LerpSegment(Vector2Int fromCell, Vector2Int toCell)
		{
			var fromWorld = _coordConverter.CellToWorld(fromCell);
			var toWorld = _coordConverter.CellToWorld(toCell);
			var distance = Vector3.Distance(fromWorld, toWorld);
			var duration = distance > 0.001f ? distance / moveSpeed : 0f;
			var elapsed = 0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				var t = Mathf.Clamp01(elapsed / duration);
				transform.position = Vector3.Lerp(fromWorld, toWorld, t);
				yield return null;
			}
			transform.position = toWorld;
		}

		#region Debug

		[TitleGroup("Debug — Identity")]
		[ShowInInspector, ReadOnly, LabelText("Unit ID")]
		[GUIColor(0.3f, 0.8f, 1f)]
		private string DbgId => UnitId ?? "not initialized";

		[TitleGroup("Debug — State")]
		[ShowInInspector, ReadOnly, LabelText("Stance")]
		private EUnitStance DbgStance => _stance;

		[TitleGroup("Debug — State")]
		[ShowInInspector, ReadOnly, LabelText("Grip")]
		private EGripType DbgGrip => _grip;

		[TitleGroup("Debug — State")]
		[ShowInInspector, ReadOnly, LabelText("Facing Right")]
		private bool DbgFacing => _facingRight;

		[TitleGroup("Debug — State")]
		[ShowInInspector, ReadOnly, LabelText("Is Moving")]
		[GUIColor("@DbgMoving ? new Color(1f, 0.8f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
		private bool DbgMoving => _moveCoroutine != null;

		#endregion
	}
}
