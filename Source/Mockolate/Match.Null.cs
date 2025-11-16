using Mockolate.Internals;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any parameter that is <see langword="null" />.
	/// </summary>
	public static IParameter<T> Null<T>()
		=> new NullParameterMatch<T>();

	private sealed class NullParameterMatch<T> : TypedMatch<T>
	{
		protected override bool Matches(T value) => value is null;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Null<{typeof(T).FormatType()}>()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
