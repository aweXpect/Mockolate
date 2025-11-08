using Mockolate.Internals;
using Mockolate.Match;

namespace Mockolate;

public partial class Parameter
{
	/// <summary>
	///     Matches any parameter that is <see langword="null" />.
	/// </summary>
	public static IParameter<T> Null<T>()
		=> new NullParameter<T>();

	private sealed class NullParameter<T> : TypedParameter<T>
	{
		protected override bool Matches(T value) => value is null;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Null<{typeof(T).FormatType()}>()";
	}
}
