using Mockolate.Internals;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any parameter of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>Also matches, if the method parameter is <see langword="null" />.</remarks>
	public static IParameter<T> WithAny<T>()
		=> new AnyParameter<T>();

	private sealed class AnyParameter<T> : TypedParameter<T>
	{
		protected override bool Matches(T value) => true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"WithAny<{typeof(T).FormatType()}>()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
