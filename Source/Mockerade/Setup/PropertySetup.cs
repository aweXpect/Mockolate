using System;
using System.Threading;
using Mockerade.Checks;

namespace Mockerade.Setup;

/// <summary>
///     Base class for property setups.
/// </summary>
public abstract class PropertySetup
{
	private int _getterInvocationCount;
	private int _setterInvocationCount;

	/// <summary>
	///     The number of matching invocations on the property setter.
	/// </summary>
	public int SetterInvocationCount => _setterInvocationCount;

	/// <summary>
	///     The number of matching invocations on the property getter.
	/// </summary>
	public int GetterInvocationCount => _getterInvocationCount;

	internal void InvokeSetter(Invocation invocation, object? value)
	{
		Interlocked.Increment(ref _setterInvocationCount);
		InvokeSetter(value);
	}

	internal TResult InvokeGetter<TResult>(Invocation invocation)
	{
		Interlocked.Increment(ref _getterInvocationCount);
		return InvokeGetter<TResult>();
	}

	/// <summary>
	///     Invokes the setter logic with the given <paramref name="value" />.
	/// </summary>
	protected abstract void InvokeSetter(object? value);

	/// <summary>
	///     Invokes the getter logic and returns the value of type <typeparamref name="TResult" />.
	/// </summary>
	protected abstract TResult InvokeGetter<TResult>();

	internal class Default : PropertySetup
	{
		private object? _value;

		/// <inheritdoc cref="PropertySetup.InvokeSetter(object?)" />
		protected override void InvokeSetter(object? value) => _value = value;

		/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}()" />
		protected override TResult InvokeGetter<TResult>()
		{
			if (_value is TResult typedValue)
			{
				return typedValue;
			}

			return default!;
		}
	}
}

/// <summary>
///     Sets up a property.
/// </summary>
public class PropertySetup<T> : PropertySetup
{
	private Func<T, T>? _getterCallback;
	private Func<T, T, T>? _setterCallback;
	private T _value = default!;

	/// <inheritdoc cref="PropertySetup.InvokeSetter(object?)" />
	protected override void InvokeSetter(object? value)
	{
		if (_setterCallback is not null)
		{
			_value = _setterCallback.Invoke(_value, value is T typedValue ? typedValue : default!);
		}
		else if (value is T typedValue)
		{
			_value = typedValue;
		}
		else
		{
			_value = default!;
		}
	}

	/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}()" />
	protected override TResult InvokeGetter<TResult>()
	{
		T value = _value;
		if (_getterCallback is not null)
		{
			value = _getterCallback(_value);
		}

		if (value is TResult typedValue)
		{
			return typedValue;
		}

		return default!;
	}

	/// <summary>
	///     Initializes the property with the given <paramref name="value" />.
	/// </summary>
	public PropertySetup<T> InitializeWith(T value)
	{
		_value = value;
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value is read. Only
	///     one callback can be registered; subsequent calls to this method will replace any previously set callback.
	/// </remarks>
	public PropertySetup<T> OnGet(Action callback)
	{
		_getterCallback = v =>
		{
			callback();
			return v;
		};
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set. The callback receives the new value being
	///     set.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value changes. Only
	///     one callback can be registered; subsequent calls to this method will replace any previously set callback.
	/// </remarks>
	public PropertySetup<T> OnSet(Action<T> callback)
	{
		_setterCallback = (_, newValue) =>
		{
			callback(newValue);
			return newValue;
		};
		return this;
	}
}
