using System.Collections.Generic;
using System.Linq;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches a parameter that is equal to one of the <paramref name="values" />.
	/// </summary>
	public static IParameter<T> IsOneOf<T>(params T[] values)
		=> new ParameterIsOneOfMatch<T>(values);

	private sealed class ParameterIsOneOfMatch<T>(T[] values) : TypedMatch<T>
	{
		/// <inheritdoc cref="TypedMatch{T}.Matches(T)" />
		protected override bool Matches(T value)
		{
			EqualityComparer<T> comparer = EqualityComparer<T>.Default;
			return values.Any(v => comparer.Equals(value, v));
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"It.IsOneOf({string.Join(", ", values.Select(v => v is string ? $"\"{v}\"" : v?.ToString() ?? "null"))})";
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
