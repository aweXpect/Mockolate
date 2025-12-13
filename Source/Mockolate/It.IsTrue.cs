using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any boolean parameter that is <see langword="true" />.
	/// </summary>
	public static IParameter<bool> IsTrue()
		=> new TrueParameterMatch();

	private sealed class TrueParameterMatch : TypedMatch<bool>
	{
		/// <inheritdoc cref="TypedMatch{T}.Matches(T)" />
		protected override bool Matches(bool value) => value;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => "It.IsTrue()";
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
