using UnityEngine;

namespace Presentation.Map.GunLine
{
	public class GunLineHighlightLayerMask
	{
		public const string LayerName = "Highlighted";

		private const int Unresolved = -2; // NameToLayer失败返回-1，用-2区分"尚未解析"

		private static int _id = Unresolved;

		public static int Id
		{
			get
			{
				if (_id != Unresolved) return _id;

				_id = LayerMask.NameToLayer(LayerName);

				if (_id < 0)
					Debug.LogError($"[GunLineHighlightLayer] Layer '{LayerName}' 未定义，请在Project Settings中创建");

				return _id;
			}
		}

		public static bool IsValid => Id >= 0;
	}
}
