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
	///     Generates a default value of the specified <paramref name="type" />, with
	///     the <paramref name="parameters" /> for context.
	/// </summary>
	public virtual object? GenerateValue(Type type, params object?[] parameters)
		=> _generator?.Invoke(type, parameters);
}
