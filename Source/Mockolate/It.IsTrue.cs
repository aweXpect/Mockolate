using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class It
{
	/// <summary>
	///     Matches any boolean parameter that is <see langword="true" />.
	/// </summary>
	public static IParameter<bool> IsTrue()
		=> new TrueParameterMatch();

	private sealed class TrueParameterMatch : TypedMatch<bool>
	{
		protected override bool Matches(bool value) => value;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => "It.IsTrue()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
