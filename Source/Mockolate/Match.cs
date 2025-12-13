using System.Diagnostics.CodeAnalysis;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
/// <summary>
///     Specify a matching condition for a list of parameter.
/// </summary>
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
