using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Map.Config.SceneActor
{
	[CreateAssetMenu(fileName = "AtlasSliceManifest", menuName = "Game/Map/SceneActor/Atlas Slice Manifest")]
	public class AtlasSliceManifest : ScriptableObject
	{
		[Title("Atlas")]
		public Texture2D atlas;

		[Tooltip("Atlas 上每个格子的像素尺寸（如 400×600，含美术余量）")]
		public Vector2Int atlasGridSize = new(400, 600);

		[Tooltip("游戏 cell 的像素尺寸（如 400×200）；决定 pivot 在 atlas 格子里的垂直位置（底对齐）")]
		public Vector2Int gameCellSize = new(400, 200);

		[Title("Actors")]
		[Tooltip("此 atlas 上的所有场景物体")]
		public List<SceneActorConfig> configs = new();

		public RectOffset padding;
	}
}
