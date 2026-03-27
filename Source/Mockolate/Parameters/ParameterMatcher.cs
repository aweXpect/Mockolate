using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mockolate.Parameters;

/// <summary>
///     Non-generic base class for all typed parameter matchers.
///     Because this is a class (not an interface), it enables implicit conversion operators in <see cref="Param{T}" />.
/// </summary>
[DebuggerNonUserCode]
public abstract class ParameterMatcher : IParameter
{
	/// <inheritdoc cref="IParameter.Matches(object?)" />
	public abstract bool Matches(object? value);

	/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
	public abstract void InvokeCallbacks(object? value);

	internal static bool TryCast<T>(object? value, out T typedValue)
	{
		if (value is T castValue)
		{
			typedValue = castValue;
			return true;
		}

		if (value is null && default(T) is null)
		{
			typedValue = default!;
			return true;
		}

		typedValue = default!;
		return false;
	}
}

/// <summary>
///     Generic base class for typed parameter matchers.
///     Extends <see cref="ParameterMatcher" /> and implements <see cref="IParameter{T}" />.
/// </summary>
[DebuggerNonUserCode]
public abstract class ParameterMatcher<T> : ParameterMatcher, IParameter<T>
{
	private List<Action<T>>? _callbacks;

	/// <inheritdoc cref="IParameter.Matches(object?)" />
	public override bool Matches(object? value)
	{
		if (value is T typedValue)
		{
			return Matches(typedValue);
		}

		return value is null && Matches(default!);
	}

	/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
	public override void InvokeCallbacks(object? value)
	{
		if (TryCast(value, out T typedValue) && _callbacks is not null)
		{
			_callbacks.ForEach(a => a.Invoke(typedValue));
		}
	}

	/// <inheritdoc cref="IParameter{T}.Do(Action{T})" />
	public IParameter<T> Do(Action<T> callback)
	{
		_callbacks ??= [];
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Verifies the expectation for the <paramref name="value" />.
	/// </summary>
	protected abstract bool Matches(T value);
}

