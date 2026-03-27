using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
/// <summary>
///     Specify a matching condition for a parameter.
/// </summary>
[DebuggerNonUserCode]
public partial class It
{
	/// <summary>
	///     This class is intentionally not static to allow adding static extension methods on <see cref="It" />.
	/// </summary>
	[ExcludeFromCodeCoverage]
	private It()
	{
		// Prevent instantiation.
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
