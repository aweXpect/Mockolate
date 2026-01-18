using System;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     Gives access to the initializations and constructor parameters of a <see cref="MockBehavior" />.
/// </summary>
public interface IMockBehaviorAccess
{
	/// <summary>
	///     Tries to get the initialization setups for a mock of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     Returns <see langword="false" />, when no matching initialization is found.
	/// </remarks>
	bool TryInitialize<T>([NotNullWhen(true)] out Action<IMockSetup<T>>[]? setups);

	/// <summary>
	///     Tries to get the constructor parameters for a mock of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     Returns <see langword="false" />, when no matching constructor parameters are found.
	/// </remarks>
	bool TryGetConstructorParameters<T>([NotNullWhen(true)] out object?[]? parameters);
}
