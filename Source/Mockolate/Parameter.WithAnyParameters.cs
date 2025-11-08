using Mockolate.Match;

namespace Mockolate;

public partial class Parameter
{
	/// <summary>
	///     Matches any parameter combination.
	/// </summary>
	public static IParameters WithAnyParameters()
		=> new AnyParameters();

	private sealed class AnyParameters : IParameters
	{
		/// <inheritdoc cref="IParameters.Matches(object?[])" />
		public bool Matches(object?[] values)
			=> true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"WithAnyParameters()";
	}
}
