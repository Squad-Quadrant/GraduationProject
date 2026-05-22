using System;
using System.Collections.Generic;
using Core.Log;
using Presentation.Dialogue.Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Dialogue.Portrait
{
	public class PortraitStage : MonoBehaviour
	{
		private class ActivePortrait
		{
			public CharacterConfig Character;
			public EPortraitPosition Position;
			public IPortraitView View;
			public GameObject GameObject;

			public string CurrentPose;
			public string CurrentSkin;
		}

		[TitleGroup("Anchors")]
        [SerializeField, Required] private RectTransform anchorLeft;
        [SerializeField, Required] private RectTransform anchorCenter;
        [SerializeField, Required] private RectTransform anchorRight;

        private readonly Dictionary<CharacterConfig, ActivePortrait> _activeMap = new();

        public void Apply(IReadOnlyList<PortraitEntry> portraits, Action onComplete)
        {
            Diff(portraits, out var toEnter, out var toExit, out var toChange);

            var totalCount = toEnter.Count + toExit.Count + toChange.Count;
            if (totalCount == 0)
            {
                onComplete?.Invoke();
                return;
            }

            var completed = 0;

            foreach (var active in toExit)
                ExecuteExit(active, HandleCompletion);

            foreach (var entry in toEnter)
                ExecuteEnter(entry, HandleCompletion);

            foreach (var (active, entry) in toChange)
                ExecuteChange(active, entry, HandleCompletion);
            return;

            void HandleCompletion()
            {
	            completed++;
	            if (completed == totalCount) onComplete?.Invoke();
            }
        }

        public void ClearAll(Action onComplete) => Apply(Array.Empty<PortraitEntry>(), onComplete);

        private void Diff(
            IReadOnlyList<PortraitEntry> targetPortraits,
            out List<PortraitEntry> toEnter,
            out List<ActivePortrait> toExit,
            out List<(ActivePortrait active, PortraitEntry entry)> toChange)
        {
            toEnter  = new List<PortraitEntry>();
            toExit   = new List<ActivePortrait>();
            toChange = new List<(ActivePortrait, PortraitEntry)>();

            var targetByChar = new Dictionary<CharacterConfig, PortraitEntry>();
            foreach (var portrait in targetPortraits)
            {
                if (!portrait.character)
                {
                    this.LogError("PortraitEntry with null character. Skipping.");
                    continue;
                }
                if (!targetByChar.TryAdd(portrait.character, portrait))
	                this.LogError($"Character '{portrait.character.displayName}' duplicated in single node. Keeping first entry, ignoring rest.");
            }

            foreach (var (character, active) in _activeMap)
            {
	            if (!targetByChar.TryGetValue(character, out var entry))
		            toExit.Add(active);
	            else if (active.Position != entry.position)
                {
                    toExit.Add(active);
                    toEnter.Add(entry);
                }
                else if (HasAppearanceChange(active, entry))
		            toChange.Add((active, entry));
            }

            foreach (var (character, entry) in targetByChar)
	            if (!_activeMap.ContainsKey(character))
		            toEnter.Add(entry);
        }

        private static bool HasAppearanceChange(ActivePortrait active, PortraitEntry entry) =>
	        active.CurrentPose != entry.Pose ||
	        active.CurrentSkin != entry.Skin ||
	        !string.IsNullOrEmpty(entry.oneShotPose);

        private void ExecuteEnter(PortraitEntry entry, Action onComplete)
        {
            var character = entry.character;
            var prefab = character.portraitPrefab;
            if (!prefab)
            {
                this.LogError($"Character '{character.displayName}' has no portraitPrefab. Skipping enter.");
                onComplete?.Invoke();   // 仍然计数，避免 totalCount 永远凑不齐
                return;
            }

            var anchor = GetAnchor(entry.position);
            var go = Instantiate(prefab, anchor, worldPositionStays: false);
            var view = go.GetComponent<IPortraitView>();

            if (view == null)
            {
                this.LogError($"Prefab '{prefab.name}' has no IPortraitView component. Destroying instance.");
                Destroy(go);
                onComplete?.Invoke();
                return;
            }

            view.Setup(character, entry.Pose, entry.Skin);

            _activeMap[character] = new ActivePortrait
            {
                Character   = character,
                Position    = entry.position,
                View        = view,
                GameObject  = go,
                CurrentPose = entry.Pose,
                CurrentSkin = entry.Skin,
            };

            view.PlayEntrance(entry.position, entry.oneShotPose, onComplete);
        }

        private void ExecuteExit(ActivePortrait active, Action onComplete)
        {
            var go = active.GameObject;
            _activeMap.Remove(active.Character);

            active.View.PlayExit(active.Position, () =>
            {
                if (go) Destroy(go);
                onComplete?.Invoke();
            });
        }

        private void ExecuteChange(ActivePortrait active, PortraitEntry entry, Action onComplete)
        {
            active.CurrentPose = entry.Pose;
            active.CurrentSkin = entry.Skin;
            active.View.ChangeAppearance(entry.Pose, entry.Skin, entry.oneShotPose, onComplete);
        }

        private RectTransform GetAnchor(EPortraitPosition position) => position switch
        {
            EPortraitPosition.Left   => anchorLeft,
            EPortraitPosition.Center => anchorCenter,
            EPortraitPosition.Right  => anchorRight,
            _ => null
        };
	}
}
