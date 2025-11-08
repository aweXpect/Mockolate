namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Use default event parameters when raising events.
	/// </summary>
	public static IDefaultEventParameters WithDefaultParameters()
		=> new DefaultEventParameters();

	private sealed class DefaultEventParameters : IDefaultEventParameters
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => "WithDefaultParameters()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
