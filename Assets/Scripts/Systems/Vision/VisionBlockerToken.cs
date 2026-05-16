namespace Systems.Vision
{
	public readonly struct VisionBlockerToken
	{
		public readonly int Id;
		public bool IsValid => Id > 0;

		internal VisionBlockerToken(int id) => Id = id;

		public override string ToString() => $"VisionBlockerToken({Id})";

		public static readonly VisionBlockerToken Invalid = new(-1);
	}
}

