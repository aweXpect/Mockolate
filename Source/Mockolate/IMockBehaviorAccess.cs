using System.Diagnostics.CodeAnalysis;

namespace Mockolate;

/// <summary>
///     Gives access to the initializations and constructor parameters of a <see cref="MockBehavior" />.
/// </summary>
public interface IMockBehaviorAccess
{
	/// <summary>
	///     Returns a new <see cref="MockBehavior" /> with <paramref name="value" /> stored under the key
	///     <typeparamref name="T" />. Any existing entry for <typeparamref name="T" /> is replaced.
	/// </summary>
	/// <typeparam name="T">The storage key &#8212; typically the runtime type of <paramref name="value" />.</typeparam>
	/// <param name="value">The value to store.</param>
	/// <returns>A new <see cref="MockBehavior" /> with the value attached; the current instance is unchanged.</returns>
	MockBehavior Set<T>(T value);

	/// <summary>
	///     Retrieves the value previously stored under key <typeparamref name="T" />.
	/// </summary>
	/// <typeparam name="T">The storage key to look up.</typeparam>
	/// <param name="value">When this method returns <see langword="true" />, contains the stored value; otherwise <see langword="default" />.</param>
	/// <returns><see langword="true" /> when a value is found; <see langword="false" /> otherwise.</returns>
	bool TryGet<T>([NotNullWhen(true)] out T value);

	/// <summary>
	///     Tries to get the constructor parameters configured for a mock of type <typeparamref name="T" />.
	/// </summary>
	/// <typeparam name="T">The mocked type whose constructor parameters are being looked up.</typeparam>
	/// <param name="parameters">When this method returns <see langword="true" />, contains the configured parameter array; otherwise <see langword="null" />.</param>
	/// <returns><see langword="false" /> when no matching constructor parameters are found; <see langword="true" /> otherwise.</returns>
	bool TryGetConstructorParameters<T>([NotNullWhen(true)] out object?[]? parameters);
}
