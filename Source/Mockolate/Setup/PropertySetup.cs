using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Setup;

/// <summary>
///     Base class for property setups.
/// </summary>
public abstract class PropertySetup
{
	internal void InvokeSetter(IInteraction invocation, object? value, MockBehavior behavior)
		=> InvokeSetter(value, behavior);

	internal TResult InvokeGetter<TResult>(IInteraction invocation, MockBehavior behavior)
		=> InvokeGetter<TResult>(behavior);

	internal bool? CallBaseClass()
		=> GetCallBaseClass();

	internal void InitializeWith(object? baseValue)
		=> InitializeValue(baseValue);

	/// <summary>
	/// Initializes the property with the <paramref name="baseValue" />.
	/// </summary>
	protected abstract void InitializeValue(object? baseValue);

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be called, and its return values
	///     used as default values.
	/// </summary>
	protected abstract bool? GetCallBaseClass();

	/// <summary>
	///     Invokes the setter logic with the given <paramref name="value" />.
	/// </summary>
	protected abstract void InvokeSetter(object? value, MockBehavior behavior);

	/// <summary>
	///     Invokes the getter logic and returns the value of type <typeparamref name="TResult" />.
	/// </summary>
	protected abstract TResult InvokeGetter<TResult>(MockBehavior behavior);

	internal class Default(object? initialValue) : PropertySetup
	{
		private object? _value = initialValue;
		private bool _isInitialized;

		/// <inheritdoc cref="PropertySetup.InitializeValue(object?)" />
		protected override void InitializeValue(object? baseValue)
		{
			if (!_isInitialized)
			{
				_isInitialized = true;
				_value = baseValue;
			}
		}

		/// <inheritdoc cref="PropertySetup.GetCallBaseClass()" />
		protected override bool? GetCallBaseClass()
			=> null;

		/// <inheritdoc cref="PropertySetup.InvokeSetter(object?, MockBehavior)" />
		protected override void InvokeSetter(object? value, MockBehavior behavior)
		{
			_isInitialized = true;
			_value = value;
		}

		/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}(MockBehavior)" />
		protected override TResult InvokeGetter<TResult>(MockBehavior behavior)
		{
			if (_value is TResult typedValue)
			{
				return typedValue;
			}

			return behavior.DefaultValue.Generate<TResult>();
		}
	}
}

/// <summary>
///     Sets up a property.
/// </summary>
public class PropertySetup<T> : PropertySetup
{
	private readonly List<Action<T>> _getterCallbacks = [];
	private readonly List<Func<T, T>> _returnCallbacks = [];
	private readonly List<Action<T, T>> _setterCallbacks = [];
	private bool? _callBaseClass;
	private int _currentReturnCallbackIndex = -1;
	private T _value = default!;
	private bool _isInitialized;

	/// <inheritdoc cref="PropertySetup.InvokeSetter(object?, MockBehavior)" />
	protected override void InvokeSetter(object? value, MockBehavior behavior)
	{
		if (!TryCast(value, out T parameter, behavior))
		{
			throw new MockException(
				$"The property value only supports '{typeof(T).FormatType()}', but is '{value.GetType().FormatType()}'.");
		}

		_setterCallbacks.ForEach(callback => callback.Invoke(_value, parameter));
		_value = parameter;
	}

	/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}(MockBehavior)" />
	protected override TResult InvokeGetter<TResult>(MockBehavior behavior)
	{
		_getterCallbacks.ForEach(callback => callback.Invoke(_value));
		if (_returnCallbacks.Count > 0)
		{
			int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
			Func<T, T> returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
			_value = returnCallback(_value);
		}

		if (!TryCast(_value, out TResult result, MockBehavior.Default))
		{
			throw new MockException(
				$"The property only supports '{typeof(T).FormatType()}' and not '{typeof(TResult).FormatType()}'.");
		}

		return result;
	}
	
	/// <inheritdoc cref="PropertySetup.InitializeValue(object?)" />
	protected override void InitializeValue(object? baseValue)
	{
		if (_isInitialized)
		{
			return;
		}

		_isInitialized = baseValue is T or null;
		if (baseValue is T typedValue)
		{
			_value = typedValue;
		}
		else
		{
			_value = default!;
		}
	}

	/// <inheritdoc cref="PropertySetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	public PropertySetup<T> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
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
	///     Use this method to perform custom logic or side effects whenever the property's value is read.
	/// </remarks>
	public PropertySetup<T> OnGet(Action callback)
	{
		_getterCallbacks.Add(_ => callback());
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value is read.
	/// </remarks>
	public PropertySetup<T> OnGet(Action<T> callback)
	{
		_getterCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public PropertySetup<T> Returns(Func<T, T> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public PropertySetup<T> Returns(Func<T> callback)
	{
		_returnCallbacks.Add(_ => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this property.
	/// </summary>
	public PropertySetup<T> Returns(T returnValue)
	{
		_returnCallbacks.Add(_ => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the property is read.
	/// </summary>
	public PropertySetup<T> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add(_ => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the property is read.
	/// </summary>
	public PropertySetup<T> Throws(Exception exception)
	{
		_returnCallbacks.Add(_ => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public PropertySetup<T> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add(_ => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public PropertySetup<T> Throws(Func<T, Exception> callback)
	{
		_returnCallbacks.Add(v => throw callback(v));
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set. The callback receives the new value being
	///     set.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value changes.
	/// </remarks>
	public PropertySetup<T> OnSet(Action callback)
	{
		_setterCallbacks.Add((_, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set. The callback receives the new value being
	///     set.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value changes.
	/// </remarks>
	public PropertySetup<T> OnSet(Action<T, T> callback)
	{
		_setterCallbacks.Add(callback);
		return this;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> typeof(T).FormatType();

	private static bool TryCast<TValue>([NotNullWhen(false)] object? value, out TValue result, MockBehavior behavior)
	{
		if (value is TValue typedValue)
		{
			result = typedValue;
			return true;
		}

		result = behavior.DefaultValue.Generate<TValue>();
		return value is null;
	}
}
