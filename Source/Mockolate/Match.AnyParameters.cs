using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any argument combination passed to the method.
	/// </summary>
	/// <returns>
	///     An <see cref="IParameters" /> usable in generator-emitted <c>Setup.Method(Match.AnyParameters())</c>
	///     and <c>Verify.Method(Match.AnyParameters())</c> overloads.
	/// </returns>
	/// <remarks>
	///     Only available on methods whose name is unique on the mocked type (no overloads). For overloaded
	///     methods, combine per-parameter <see cref="It.IsAny{T}" /> matchers instead.
	/// </remarks>
	public static IParameters AnyParameters()
		=> new AnyParametersMatch();

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class AnyParametersMatch : IParameters
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => "Match.AnyParameters()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
