using System;

namespace Mockolate;

/// <summary>
///     Defines a mechanism for generating default values.
/// </summary>
public class DefaultValueFactory
{
	private readonly Func<Type, object?[], object?>? _generator;
	private readonly Func<Type, bool>? _predicate;

	/// <summary>
	///     This constructor is protected to allow inheritance.
	/// </summary>
	// ReSharper disable once UnusedMember.Global
	protected DefaultValueFactory()
	{
	}

	/// <summary>
	///     Creates a new default value factory for types that match the given <paramref name="predicate" />.
	/// </summary>
	public DefaultValueFactory(Func<Type, bool> predicate, Func<Type, object?[], object?> generator)
	{
		_predicate = predicate;
		_generator = generator;
	}

	/// <summary>
	///     Checks if the factory can generate a value for the specified <paramref name="type" />.
	/// </summary>
	public virtual bool CanGenerateValue(Type type)
		=> _predicate?.Invoke(type) ?? false;

	/// <summary>
	///     Generates a default value for <paramref name="type" /> using the registered generator delegate.
	/// </summary>
	/// <param name="type">The runtime type to produce a default value for.</param>
	/// <param name="parameters">
	///     The runtime arguments of the mocked invocation, forwarded so the factory can vary its result by
	///     context (for example, returning a cancelled task when a cancelled
	///     <see cref="System.Threading.CancellationToken" /> is present). See
	///     <see cref="IDefaultValueGenerator.GenerateValue(Type, object?[])" /> for details.
	/// </param>
	/// <returns>The generated value, or <see langword="null" /> when no generator has been configured.</returns>
	public virtual object? GenerateValue(Type type, params object?[] parameters)
		=> _generator?.Invoke(type, parameters);
}
