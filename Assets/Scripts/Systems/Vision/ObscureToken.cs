using System;

namespace Systems.Vision
{
	public readonly struct ObscureToken
	{
		public readonly int Id;
		public bool IsValid => Id > 0;

		internal ObscureToken(int id) => Id = id;

		public override string ToString() => $"ObscureToken({Id})";

		public static readonly ObscureToken Invalid = new(-1);
	}
}

