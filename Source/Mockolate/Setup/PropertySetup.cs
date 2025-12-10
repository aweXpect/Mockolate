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
public abstract class PropertySetup : IInteractivePropertySetup
{
	/// <summary>
	///     The property name.
	/// </summary>
	public abstract string Name { get; }

	/// <inheritdoc cref="IInteractivePropertySetup.InvokeSetter(IInteraction, object?, MockBehavior)" />
	void IInteractivePropertySetup.InvokeSetter(IInteraction invocation, object? value, MockBehavior behavior)
		=> InvokeSetter(value, behavior);

	/// <inheritdoc cref="IInteractivePropertySetup.InvokeGetter{TResult}(IInteraction, MockBehavior, Func{TResult}?)" />
	TResult IInteractivePropertySetup.InvokeGetter<TResult>(IInteraction invocation, MockBehavior behavior,
		Func<TResult>? defaultValueGenerator)
		=> InvokeGetter(behavior, defaultValueGenerator);

	/// <inheritdoc cref="IInteractivePropertySetup.Matches(PropertyAccess)" />
	bool IInteractivePropertySetup.Matches(PropertyAccess propertyAccess)
		=> Matches(propertyAccess);

	/// <inheritdoc cref="IInteractivePropertySetup.CallBaseClass()" />
	bool? IInteractivePropertySetup.CallBaseClass()
		=> GetCallBaseClass();

	/// <inheritdoc cref="IInteractivePropertySetup.InitializeWith(object?)" />
	void IInteractivePropertySetup.InitializeWith(object? value)
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
	///     Checks if the <paramref name="propertyAccess" /> matches the setup.
	/// </summary>
	protected abstract bool Matches(PropertyAccess propertyAccess);

	/// <summary>
	///     Invokes the setter logic with the given <paramref name="value" />.
	/// </summary>
	protected abstract void InvokeSetter(object? value, MockBehavior behavior);

	/// <summary>
	///     Invokes the getter logic and returns the value of type <typeparamref name="TResult" />.
	/// </summary>
	protected abstract TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult>? defaultValueGenerator);

	internal class Default(string name, object? initialValue) : PropertySetup
	{
		private bool _isInitialized = true;
		private object? _value = initialValue;

		/// <inheritdoc cref="PropertySetup.Name" />
		public override string Name => name;

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

		/// <inheritdoc cref="PropertySetup.Matches(PropertyAccess)" />
		protected override bool Matches(PropertyAccess propertyAccess)
			=> name.Equals(propertyAccess.Name);

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
public class PropertySetup<T>(string name) : PropertySetup,
	IPropertySetupCallbackBuilder<T>, IPropertySetupReturnBuilder<T>,
	IPropertyGetterSetup<T>, IPropertySetterSetup<T>
{
	private readonly List<Callback<Action<int, T>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, T, T>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, T>>> _setterCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private int _currentGetterCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private int _currentSetterCallbacksIndex;
	private bool _isInitialized;
	private T _value = default!;

	/// <inheritdoc cref="PropertySetup.Name" />
	public override string Name => name;

	/// <inheritdoc cref="IPropertySetupCallbackBuilder{T}.When(Func{int, bool})" />
	IPropertySetupWhenBuilder<T> IPropertySetupCallbackBuilder<T>.When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupCallbackBuilder{T}.InParallel()" />
	IPropertySetupWhenBuilder<T> IPropertySetupCallbackBuilder<T>.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IPropertySetupWhenBuilder{T}.For(int)" />
	IPropertySetupWhenBuilder<T> IPropertySetupWhenBuilder<T>.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupWhenBuilder{T}.Only(int)" />
	IPropertySetup<T> IPropertySetupWhenBuilder<T>.Only(int times)
	{
		_currentCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupReturnBuilder{T}.When(Func{int, bool})" />
	IPropertySetupReturnWhenBuilder<T> IPropertySetupReturnBuilder<T>.When(Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupReturnWhenBuilder{T}.For(int)" />
	IPropertySetupReturnWhenBuilder<T> IPropertySetupReturnWhenBuilder<T>.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupReturnWhenBuilder{T}.Only(int)" />
	IPropertySetup<T> IPropertySetupReturnWhenBuilder<T>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="PropertySetup.Matches(PropertyAccess)" />
	protected override bool Matches(PropertyAccess propertyAccess)
		=> name.Equals(propertyAccess.Name);

	/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}(MockBehavior, Func{TResult})" />
	protected override TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult>? defaultValueGenerator)
	{
		bool wasInvoked = false;
		int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
		for (int i = 0; i < _getterCallbacks.Count; i++)
		{
			Callback<Action<int, T>> getterCallback =
				_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
			if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, _value)))
			{
				wasInvoked = true;
			}
		}

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

		if (!TryCast(_value, out TResult result))
		{
			throw new MockException(
				$"The property only supports '{typeof(T).FormatType()}' and not '{typeof(TResult).FormatType()}'.");
		}

		return result;
	}

	/// <inheritdoc cref="PropertySetup.InvokeSetter(object?, MockBehavior)" />
	protected override void InvokeSetter(object? value, MockBehavior behavior)
	{
		if (!TryCast(value, out T newValue))
		{
			throw new MockException(
				$"The property value only supports '{typeof(T).FormatType()}', but is '{value.GetType().FormatType()}'.");
		}

		bool wasInvoked = false;
		int currentSetterCallbacksIndex = _currentSetterCallbacksIndex;
		for (int i = 0; i < _setterCallbacks.Count; i++)
		{
			Callback<Action<int, T>> setterCallback =
				_setterCallbacks[(currentSetterCallbacksIndex + i) % _setterCallbacks.Count];
			if (setterCallback.Invoke(wasInvoked, ref _currentSetterCallbacksIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, newValue)))
			{
				wasInvoked = true;
			}
		}

		_value = newValue;
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
		=> $"{typeof(T).FormatType()} {name}";

	private static bool TryCast<TValue>([NotNullWhen(false)] object? value, out TValue result)
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

	/// <inheritdoc cref="IPropertySetup{T}.CallingBaseClass(bool)" />
	public IPropertySetup<T> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.InitializeWith(T)" />
	public IPropertySetup<T> InitializeWith(T value)
	{
		_value = value;
		_isInitialized = true;
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.OnGet" />
	public IPropertyGetterSetup<T> OnGet
		=> this;

	/// <inheritdoc cref="IPropertyGetterSetup{T}.Do(Action)" />
	IPropertySetupCallbackBuilder<T> IPropertyGetterSetup<T>.Do(Action callback)
	{
		Callback<Action<int, T>> item = new((_, _) => callback());
		_currentCallback = item;
		_getterCallbacks.Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertyGetterSetup{T}.Do(Action{T})" />
	IPropertySetupCallbackBuilder<T> IPropertyGetterSetup<T>.Do(Action<T> callback)
	{
		Callback<Action<int, T>> item = new((_, v) => callback(v));
		_currentCallback = item;
		_getterCallbacks.Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertyGetterSetup{T}.Do(Action{int, T})" />
	IPropertySetupCallbackBuilder<T> IPropertyGetterSetup<T>.Do(Action<int, T> callback)
	{
		Callback<Action<int, T>> item = new(callback);
		_currentCallback = item;
		_getterCallbacks.Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.OnSet" />
	public IPropertySetterSetup<T> OnSet
		=> this;

	/// <inheritdoc cref="IPropertySetterSetup{T}.Do(Action)" />
	IPropertySetupCallbackBuilder<T> IPropertySetterSetup<T>.Do(Action callback)
	{
		Callback<Action<int, T>> item = new((_, _) => callback());
		_currentCallback = item;
		_setterCallbacks.Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertySetterSetup{T}.Do(Action{T})" />
	IPropertySetupCallbackBuilder<T> IPropertySetterSetup<T>.Do(Action<T> callback)
	{
		Callback<Action<int, T>> item = new((_, newValue) => callback(newValue));
		_currentCallback = item;
		_setterCallbacks.Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertySetterSetup{T}.Do(Action{int, T})" />
	IPropertySetupCallbackBuilder<T> IPropertySetterSetup<T>.Do(Action<int, T> callback)
	{
		Callback<Action<int, T>> item = new(callback);
		_currentCallback = item;
		_setterCallbacks.Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Returns(T)" />
	public IPropertySetupReturnBuilder<T> Returns(T returnValue)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Returns(Func{T})" />
	public IPropertySetupReturnBuilder<T> Returns(Func<T> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Returns(Func{T, T})" />
	public IPropertySetupReturnBuilder<T> Returns(Func<T, T> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, p) => callback(p));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Throws{TException}()" />
	public IPropertySetupReturnBuilder<T> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Throws(Exception)" />
	public IPropertySetupReturnBuilder<T> Throws(Exception exception)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Throws(Func{Exception})" />
	public IPropertySetupReturnBuilder<T> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Throws(Func{T, Exception})" />
	public IPropertySetupReturnBuilder<T> Throws(Func<T, Exception> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new((_, p) => throw callback(p));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	#endregion IPropertySetup<T>
}
