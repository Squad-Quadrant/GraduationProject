using System;

namespace Presentation.Audio
{
	public readonly struct SfxHandle : IEquatable<SfxHandle>
	{
		public readonly int Id;

		public static readonly SfxHandle Invalid = default;

		public bool IsValid => Id > 0;

		internal SfxHandle(int id) => Id = id;

		public bool Equals(SfxHandle other) => Id == other.Id;
		public override bool Equals(object obj) => obj is SfxHandle h && Equals(h);
		public override int GetHashCode() => Id;

		public static bool operator ==(SfxHandle a, SfxHandle b) => a.Id == b.Id;
		public static bool operator !=(SfxHandle a, SfxHandle b) => a.Id != b.Id;

		public override string ToString() => IsValid ? $"SfxHandle({Id})" : "SfxHandle(Invalid)";
	}
}
