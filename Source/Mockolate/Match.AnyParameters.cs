using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any parameter combination.
	/// </summary>
	public static IParameters AnyParameters()
		=> new AnyParametersMatch();

	private sealed class AnyParametersMatch : IParameters
	{
		/// <inheritdoc cref="IParameters.Matches(object?[])" />
		public bool Matches(object?[] values)
			=> true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => "Match.AnyParameters()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
