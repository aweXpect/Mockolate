using System.Diagnostics.CodeAnalysis;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
/// <summary>
///     Specifies a matching condition across the full parameter list of a method or indexer. Complement to
///     <see cref="It" />, which targets a single parameter at a time.
/// </summary>
/// <remarks>
///     Reach for <see cref="Match" /> when you want to match by a predicate over all parameters at once, or when
///     per-parameter matchers would be too verbose. Use <see cref="AnyParameters" /> to accept any arguments, or
///     <c>Match.Parameters(predicate)</c> to match against a custom predicate.
///     <para />
///     Because the generator cannot disambiguate which overload a <see cref="Match" />-based setup targets,
///     <see cref="Match" /> overloads are only emitted on unambiguous method names. For overloaded methods, use
///     <see cref="It" /> matchers per parameter instead.
/// </remarks>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public partial class Match
{
	/// <summary>
	///     This class is intentionally not static to allow adding static extension methods on <see cref="Match" />.
	/// </summary>
	[ExcludeFromCodeCoverage]
	private Match()
	{
		// Prevent instantiation.
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
