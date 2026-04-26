using System.Diagnostics.CodeAnalysis;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
/// <summary>
///     Specifies a matching condition across the full parameter list of a method, indexer or event. Complement to
///     <see cref="It" />, which targets a single parameter at a time.
/// </summary>
/// <remarks>
///     Reach for <see cref="Match" /> when you want to match the full argument tuple with a single predicate, or
///     when per-parameter matchers would be too verbose. Available helpers:
///     <list type="bullet">
///         <item>
///             <description><see cref="AnyParameters" /> - accept any argument list (equivalent to <c>It.IsAny&lt;T&gt;()</c> on every parameter).</description>
///         </item>
///         <item>
///             <description><c>Match.Parameters(predicate)</c> - match when the predicate applied to the raw <see langword="object" /><c>?[]</c> argument array returns <see langword="true" />.</description>
///         </item>
///         <item>
///             <description><c>Match.WithDefaultParameters()</c> - match an event invocation with the default <c>EventArgs</c>-like payload.</description>
///         </item>
///     </list>
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
