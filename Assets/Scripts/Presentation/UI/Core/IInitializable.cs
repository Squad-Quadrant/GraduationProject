namespace Presentation.UI.Core
{
	/// <summary>
	/// Interface for panels that require data to display.
	/// Implement this to receive typed data when the panel is opened.
	/// </summary>
	public interface IInitializable<in TData>
	{
		void Initialize(TData data);
	}
}
