using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any parameter of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>Also matches, if the method parameter is <see langword="null" />.</remarks>
	public static IParameter<T> Any<T>()
		=> new AnyParameterMatch<T>();

	private sealed class AnyParameterMatch<T> : TypedMatch<T>
	{
		protected override bool Matches(T value) => true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Any<{typeof(T).FormatType()}>()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
