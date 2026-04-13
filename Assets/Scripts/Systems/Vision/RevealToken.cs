namespace Systems.Vision
{
	public readonly struct RevealToken
	{
		public readonly int Id;

		internal RevealToken(int id) => Id = id;

		public bool IsValid => Id > 0;

		public override string ToString() => $"RevealToken({Id})";

		public static readonly RevealToken Invalid = new(-1);
	}
}
