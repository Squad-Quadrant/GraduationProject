using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Test.WZHTest
{
	public class Test : MonoBehaviour
	{
		[SerializeField] private Tilemap tilemap;
		[Button]
		private void Foo(int x, int y, Color color)
		{
			Vector3Int pos = new Vector3Int(x, y, 0);
			tilemap.SetTileFlags(pos, TileFlags.None);
			tilemap.SetColor(pos, color);
		}
	}
}
