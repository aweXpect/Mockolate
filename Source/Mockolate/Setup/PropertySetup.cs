using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;

// ReSharper disable RedundantExtendsListEntry

namespace Mockolate.Setup;

/// <summary>
///     Base class for property setups.
/// </summary>
public abstract class PropertySetup : IPropertySetup
{
	void IPropertySetup.InvokeSetter(IInteraction invocation, object? value, MockBehavior behavior)
		=> InvokeSetter(value, behavior);

	TResult IPropertySetup.InvokeGetter<TResult>(IInteraction invocation, MockBehavior behavior)
		=> InvokeGetter<TResult>(behavior);

	bool? IPropertySetup.CallBaseClass()
		=> GetCallBaseClass();

	void IPropertySetup.InitializeWith(object? baseValue)
		=> InitializeValue(baseValue);

	/// <summary>
	///     Initializes the property with the <paramref name="baseValue" />.
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
		private bool _isInitialized;
		private object? _value = initialValue;

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
public class PropertySetup<T> : PropertySetup, IPropertySetup<T>, IPropertySetupCallbackBuilder<T>
{
	private readonly List<Callback<Action<int, T>>> _getterCallbacks = [];
	private readonly List<Func<T, T>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, T, T>>> _setterCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private int _currentReturnCallbackIndex = -1;
	private bool _isInitialized;
	private T _value = default!;

	/// <inheritdoc cref="IPropertySetupCallbackBuilder{T}.When(Func{int, bool})" />
	public IPropertySetupWhenBuilder<T> When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupWhenBuilder{T}.For(int)" />
	public IPropertySetup<T> For(int times)
	{
		_currentCallback?.For(x => x < times);
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

	/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}(MockBehavior)" />
	protected override TResult InvokeGetter<TResult>(MockBehavior behavior)
	{
		_getterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
			=> @delegate(invocationCount, _value)));
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
	public IPropertySetup<T> Returns(Func<T, T> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IPropertySetup<T> Returns(Func<T> callback)
	{
		_returnCallbacks.Add(_ => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this property.
	/// </summary>
	public IPropertySetup<T> Returns(T returnValue)
	{
		_returnCallbacks.Add(_ => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the property is read.
	/// </summary>
	public IPropertySetup<T> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add(_ => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the property is read.
	/// </summary>
	public IPropertySetup<T> Throws(Exception exception)
	{
		_returnCallbacks.Add(_ => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IPropertySetup<T> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add(_ => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IPropertySetup<T> Throws(Func<T, Exception> callback)
	{
		_returnCallbacks.Add(v => throw callback(v));
		return this;
	}

	#endregion IPropertySetup<T>
}
