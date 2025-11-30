using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Setup;

/// <summary>
///     Base class for property setups.
/// </summary>
public abstract class PropertySetup : IPropertySetup
{
	void IPropertySetup.InvokeSetter(IInteraction invocation, object? value, MockBehavior behavior)
		=> InvokeSetter(value, behavior);

	TResult IPropertySetup.InvokeGetter<TResult>(IInteraction invocation, MockBehavior behavior,
		Func<TResult>? defaultValueGenerator)
		=> InvokeGetter(behavior, defaultValueGenerator);

	bool? IPropertySetup.CallBaseClass()
		=> GetCallBaseClass();

	void IPropertySetup.InitializeWith(object? value)
		=> InitializeValue(value);

	/// <summary>
	///     Initializes the property with the <paramref name="value" />.
	/// </summary>
	protected abstract void InitializeValue(object? value);

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
	protected abstract TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult>? defaultValueGenerator);

	internal class Default(object? initialValue) : PropertySetup
	{
		private bool _isInitialized = true;
		private object? _value = initialValue;

		/// <inheritdoc cref="PropertySetup.InitializeValue(object?)" />
		protected override void InitializeValue(object? value)
		{
			if (!_isInitialized)
			{
				_isInitialized = true;
				_value = value;
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

		/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}(MockBehavior, Func{TResult})" />
		protected override TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult>? defaultValueGenerator)
		{
			if (_value is TResult typedValue)
			{
				return typedValue;
			}

			TResult result = default!;
			if (defaultValueGenerator is not null)
			{
				result = defaultValueGenerator.Invoke();
			}

			_value = result;
			return result;
		}
	}
}

/// <summary>
///     Sets up a property.
/// </summary>
public class PropertySetup<T> : PropertySetup, IPropertySetupCallbackBuilder<T>, IPropertySetupReturnBuilder<T>
{
	private readonly List<Callback<Action<int, T>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, T, T>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, T, T>>> _setterCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private bool _isInitialized;
	private T _value = default!;

	/// <inheritdoc cref="IPropertySetupCallbackBuilder{T}.When(Func{int, bool})" />
	IPropertySetupWhenBuilder<T> IPropertySetupCallbackBuilder<T>.When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupWhenBuilder{T}.For(int)" />
	IPropertySetup<T> IPropertySetupWhenBuilder<T>.For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupReturnBuilder{T}.When(Func{int, bool})" />
	IPropertySetupReturnWhenBuilder<T> IPropertySetupReturnBuilder<T>.When(Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupReturnWhenBuilder{T}.For(int)" />
	IPropertySetup<T> IPropertySetupReturnWhenBuilder<T>.For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="PropertySetup.InvokeSetter(object?, MockBehavior)" />
	protected override void InvokeSetter(object? value, MockBehavior behavior)
	{
		if (!TryCast(value, out T newValue, behavior))
		{
			throw new MockException(
				$"The property value only supports '{typeof(T).FormatType()}', but is '{value.GetType().FormatType()}'.");
		}

		_setterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
			=> @delegate(invocationCount, _value, newValue)));
		_value = newValue;
	}

	/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}(MockBehavior, Func{TResult})" />
	protected override TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult>? defaultValueGenerator)
	{
		_getterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
			=> @delegate(invocationCount, _value)));

		bool foundCallback = false;
		foreach (Callback<Func<int, T, T>> _ in _returnCallbacks)
		{
			Callback<Func<int, T, T>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, _value), out T? newValue))
			{
				foundCallback = true;
				_value = newValue ?? default!;
				_isInitialized = true;
				break;
			}
		}

		if (!foundCallback && _returnCallbacks.Count > 0)
		{
			if (defaultValueGenerator is null)
			{
				_value = default!;
			}
			else
			{
				_value = defaultValueGenerator() is T value ? value : default!;
			}

			_isInitialized = true;
		}

		if (!TryCast(_value, out TResult result, behavior))
		{
			throw new MockException(
				$"The property only supports '{typeof(T).FormatType()}' and not '{typeof(TResult).FormatType()}'.");
		}

		return result;
	}

	/// <inheritdoc cref="PropertySetup.InitializeValue(object?)" />
	protected override void InitializeValue(object? value)
	{
		if (_isInitialized)
		{
			return;
		}

		_isInitialized = value is T or null;
		if (value is T typedValue)
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

		result = default!;
		return value is null;
	}

	#region IPropertySetup<T>

	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	public IPropertySetup<T> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Initializes the property with the given <paramref name="value" />.
	/// </summary>
	public IPropertySetup<T> InitializeWith(T value)
	{
		_value = value;
		_isInitialized = true;
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value is read.
	/// </remarks>
	public IPropertySetupCallbackBuilder<T> OnGet(Action callback)
	{
		Callback<Action<int, T>> item = new((_, _) => callback());
		_currentCallback = item;
		_getterCallbacks.Add(item);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value is read.
	/// </remarks>
	public IPropertySetupCallbackBuilder<T> OnGet(Action<T> callback)
	{
		Callback<Action<int, T>> item = new((_, v) => callback(v));
		_currentCallback = item;
		_getterCallbacks.Add(item);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value is read.
	/// </remarks>
	public IPropertySetupCallbackBuilder<T> OnGet(Action<int, T> callback)
	{
		Callback<Action<int, T>> item = new(callback);
		_currentCallback = item;
		_getterCallbacks.Add(item);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set. The callback receives the new value being
	///     set.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value changes.
	/// </remarks>
	public IPropertySetupCallbackBuilder<T> OnSet(Action callback)
	{
		Callback<Action<int, T, T>> item = new((_, _, _) => callback());
		_currentCallback = item;
		_setterCallbacks.Add(item);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set.
	///     The callback receives the old and the new value being set.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value changes.
	/// </remarks>
	public IPropertySetupCallbackBuilder<T> OnSet(Action<T, T> callback)
	{
		Callback<Action<int, T, T>> item = new((_, oldValue, newValue) => callback(oldValue, newValue));
		_currentCallback = item;
		_setterCallbacks.Add(item);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set.
	///     The callback receives the old and the new value being set.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value changes.
	/// </remarks>
	public IPropertySetupCallbackBuilder<T> OnSet(Action<int, T, T> callback)
	{
		Callback<Action<int, T, T>> item = new(callback);
		_currentCallback = item;
		_setterCallbacks.Add(item);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IPropertySetupReturnBuilder<T> Returns(Func<T, T> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, p) => callback(p));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IPropertySetupReturnBuilder<T> Returns(Func<T> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this property.
	/// </summary>
	public IPropertySetupReturnBuilder<T> Returns(T returnValue)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the property is read.
	/// </summary>
	public IPropertySetupReturnBuilder<T> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the property is read.
	/// </summary>
	public IPropertySetupReturnBuilder<T> Throws(Exception exception)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IPropertySetupReturnBuilder<T> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IPropertySetupReturnBuilder<T> Throws(Func<T, Exception> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, p) => throw callback(p));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	#endregion IPropertySetup<T>
}
