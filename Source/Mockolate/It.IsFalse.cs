using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any boolean parameter that is <see langword="false" />.
	/// </summary>
	public static IParameter<bool> IsFalse()
		=> new FalseParameterMatch();

	private sealed class FalseParameterMatch : TypedMatch<bool>
	{
		protected override bool Matches(bool value) => !value;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => "It.IsFalse()";
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
