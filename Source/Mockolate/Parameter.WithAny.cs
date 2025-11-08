using Mockolate.Internals;
using Mockolate.Match;

namespace Mockolate;

public partial class Parameter
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
		public override string ToString() => $"With.Any<{typeof(T).FormatType()}>()";
	}
}
