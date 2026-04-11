using System.Diagnostics;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any parameter that is not <see langword="null" />.
	/// </summary>
	public static IParameter<T> IsNotNull<T>(string? toString = null)
		=> new NotNullParameterMatch<T>(toString);

#if RELEASE
	[DebuggerNonUserCode]
#endif
	private sealed class NotNullParameterMatch<T>(string? toString) : TypedMatch<T>
	{
		/// <inheritdoc cref="TypedMatch{T}.Matches(T)" />
		protected override bool Matches(T value) => value is not null;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => toString ?? $"It.IsNotNull<{typeof(T).FormatType()}>()";
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
