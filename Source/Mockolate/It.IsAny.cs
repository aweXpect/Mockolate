using System.Diagnostics;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any parameter of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>Also matches, if the method parameter is <see langword="null" />.</remarks>
	public static ParameterMatcher<T> IsAny<T>()
		=> new AnyParameterMatch<T>();

	[DebuggerNonUserCode]
	private sealed class AnyParameterMatch<T> : ParameterMatcher<T>
	{
		protected override bool Matches(T value) => true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"It.IsAny<{typeof(T).FormatType()}>()";
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
