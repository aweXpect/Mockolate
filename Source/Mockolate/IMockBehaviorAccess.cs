using System.Diagnostics.CodeAnalysis;

namespace Mockolate;

/// <summary>
///     Gives access to the initializations and constructor parameters of a <see cref="MockBehavior" />.
/// </summary>
public interface IMockBehaviorAccess
{
	/// <summary>
	///     Stores the given <paramref name="value" /> under the <paramref name="key" /> in the <see cref="MockBehavior" />.
	/// </summary>
	MockBehavior Set<T>(string key, T value);

	/// <summary>
	///     Retrieves the <paramref name="value" /> under the <paramref name="key" /> from the <see cref="MockBehavior" />.
	/// </summary>
	bool TryGet<T>(string key, [NotNullWhen(true)] out T value);

	/// <summary>
	///     Tries to get the constructor parameters for a mock of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     Returns <see langword="false" />, when no matching constructor parameters are found.
	/// </remarks>
	bool TryGetConstructorParameters<T>([NotNullWhen(true)] out object?[]? parameters);
}
