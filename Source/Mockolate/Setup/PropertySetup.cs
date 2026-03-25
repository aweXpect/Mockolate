using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Setup;

/// <summary>
///     Base class for property setups.
/// </summary>
[DebuggerNonUserCode]
public abstract class PropertySetup : IInteractivePropertySetup
{
	/// <summary>
	///     The property name.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	///     Gets whether the property has already been initialized with a value.
	/// </summary>
	internal abstract bool IsValueInitialized { get; }

	/// <inheritdoc cref="IInteractivePropertySetup.InvokeSetter(IInteraction, object?, MockBehavior)" />
	void IInteractivePropertySetup.InvokeSetter(IInteraction invocation, object? value, MockBehavior behavior)
		=> InvokeSetter(value, behavior);

	/// <inheritdoc cref="IInteractivePropertySetup.InvokeGetter{TResult}(IInteraction, MockBehavior, Func{TResult}?)" />
	TResult IInteractivePropertySetup.InvokeGetter<TResult>(IInteraction invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		=> InvokeGetter(behavior, defaultValueGenerator);

	/// <inheritdoc cref="IInteractivePropertySetup.Matches(PropertyAccess)" />
	bool IInteractivePropertySetup.Matches(PropertyAccess propertyAccess)
		=> Matches(propertyAccess);

	/// <inheritdoc cref="IInteractivePropertySetup.SkipBaseClass()" />
	bool? IInteractivePropertySetup.SkipBaseClass()
		=> GetSkipBaseClass();

	/// <inheritdoc cref="IInteractivePropertySetup.InitializeWith(object?)" />
	void IInteractivePropertySetup.InitializeWith(object? value)
		=> InitializeValue(value);

	/// <summary>
	///     Initializes the property with the <paramref name="value" />.
	/// </summary>
	protected abstract void InitializeValue(object? value);

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	protected abstract bool? GetSkipBaseClass();

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
	protected abstract TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult> defaultValueGenerator);

	[DebuggerNonUserCode]
	internal class Default(string name, object? initialValue) : PropertySetup
	{
		private object? _value = initialValue;

		/// <inheritdoc cref="PropertySetup.Name" />
		public override string Name => name;

		/// <inheritdoc cref="PropertySetup.IsValueInitialized" />
		internal override bool IsValueInitialized => true;

		/// <inheritdoc cref="PropertySetup.InitializeValue(object?)" />
		protected override void InitializeValue(object? value)
		{
			// Is always initialized from the constructor
		}

		/// <inheritdoc cref="PropertySetup.GetSkipBaseClass()" />
		protected override bool? GetSkipBaseClass()
			=> null;

		/// <inheritdoc cref="PropertySetup.Matches(PropertyAccess)" />
		protected override bool Matches(PropertyAccess propertyAccess)
			=> name.Equals(propertyAccess.Name);

		/// <inheritdoc cref="PropertySetup.InvokeSetter(object?, MockBehavior)" />
		protected override void InvokeSetter(object? value, MockBehavior behavior)
			=> _value = value;

		/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}(MockBehavior, Func{TResult})" />
		protected override TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult> defaultValueGenerator)
		{
			if (_value is TResult typedValue)
			{
				return typedValue;
			}

			TResult result = defaultValueGenerator.Invoke();
			_value = result;
			return result;
		}
	}
}

/// <summary>
///     Sets up a property.
/// </summary>
[DebuggerNonUserCode]
public class PropertySetup<T>(string name) : PropertySetup,
	IPropertySetupCallbackBuilder<T>, IPropertySetupReturnBuilder<T>,
	IPropertyGetterSetup<T>, IPropertySetterSetup<T>
{
	private Callback? _currentCallback;
	private int _currentGetterCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private int _currentSetterCallbacksIndex;
	private List<Callback<Action<int, T>>>? _getterCallbacks;
	private bool _isInitialized;
	private List<Callback<Func<int, T, T>>>? _returnCallbacks;
	private List<Callback<Action<int, T>>>? _setterCallbacks;
	private bool? _skipBaseClass;
	private T _value = default!;

	/// <inheritdoc cref="PropertySetup.Name" />
	public override string Name => name;

	/// <inheritdoc cref="PropertySetup.IsValueInitialized" />
	internal override bool IsValueInitialized => _isInitialized;

	/// <inheritdoc cref="IPropertySetupCallbackBuilder{T}.When(Func{int, bool})" />
	IPropertySetupCallbackWhenBuilder<T> IPropertySetupCallbackBuilder<T>.When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupCallbackBuilder{T}.InParallel()" />
	IPropertySetupCallbackWhenBuilder<T> IPropertySetupCallbackBuilder<T>.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IPropertySetupCallbackWhenBuilder{T}.For(int)" />
	IPropertySetupCallbackWhenBuilder<T> IPropertySetupCallbackWhenBuilder<T>.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupCallbackWhenBuilder{T}.Only(int)" />
	IPropertySetup<T> IPropertySetupCallbackWhenBuilder<T>.Only(int times)
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
	protected override TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult> defaultValueGenerator)
	{
		if (_getterCallbacks is not null)
		{
			bool wasInvoked = false;
			int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
			for (int i = 0; i < _getterCallbacks.Count; i++)
			{
				Callback<Action<int, T>> getterCallback =
					_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
				if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, Callback))
				{
					wasInvoked = true;
				}
			}
		}

		if (_returnCallbacks is not null)
		{
			foreach (Callback<Func<int, T, T>> _ in _returnCallbacks)
			{
				Callback<Func<int, T, T>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, ReturnCallback, out T? newValue))
				{
					_value = newValue;
					_isInitialized = true;
					break;
				}
			}
		}

		if (typeof(TResult) == typeof(T))
		{
			return Unsafe.As<T, TResult>(ref _value);
		}

		if (!TryCast(_value, out TResult result))
		{
			throw new MockException(
				$"The property only supports '{typeof(T).FormatType()}' and not '{typeof(TResult).FormatType()}'.");
		}

		return result;

		[DebuggerNonUserCode]
		void Callback(int invocationCount, Action<int, T> @delegate)
		{
			@delegate(invocationCount, _value);
		}

		[DebuggerNonUserCode]
		T ReturnCallback(int invocationCount, Func<int, T, T> @delegate)
		{
			return @delegate(invocationCount, _value);
		}
	}

	/// <inheritdoc cref="PropertySetup.InvokeSetter(object?, MockBehavior)" />
	protected override void InvokeSetter(object? value, MockBehavior behavior)
	{
		if (!TryCast(value, out T newValue))
		{
			throw new MockException(
				$"The property value only supports '{typeof(T).FormatType()}', but is '{value.GetType().FormatType()}'.");
		}

		if (_setterCallbacks is not null)
		{
			bool wasInvoked = false;
			int currentSetterCallbacksIndex = _currentSetterCallbacksIndex;
			for (int i = 0; i < _setterCallbacks.Count; i++)
			{
				Callback<Action<int, T>> setterCallback =
					_setterCallbacks[(currentSetterCallbacksIndex + i) % _setterCallbacks.Count];
				if (setterCallback.Invoke(wasInvoked, ref _currentSetterCallbacksIndex, Callback))
				{
					wasInvoked = true;
				}
			}
		}

		_value = newValue;

		[DebuggerNonUserCode]
		void Callback(int invocationCount, Action<int, T> @delegate)
		{
			@delegate(invocationCount, newValue);
		}
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
	}

	/// <inheritdoc cref="PropertySetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

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

	/// <inheritdoc cref="IPropertySetup{T}.SkippingBaseClass(bool)" />
	public IPropertySetup<T> SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Register" />
	public IPropertySetup<T> Register()
		=> this;

	/// <inheritdoc cref="IPropertySetup{T}.InitializeWith(T)" />
	public IPropertySetup<T> InitializeWith(T value)
	{
		InitializeValue(value);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.OnGet" />
	public IPropertyGetterSetup<T> OnGet
		=> this;

	/// <inheritdoc cref="IPropertyGetterSetup{T}.Do(Action)" />
	IPropertySetupCallbackBuilder<T> IPropertyGetterSetup<T>.Do(Action callback)
	{
		Callback<Action<int, T>> item = new(Delegate);
		_currentCallback = item;
		(_getterCallbacks ??= []).Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T _currentValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IPropertyGetterSetup{T}.Do(Action{T})" />
	IPropertySetupCallbackBuilder<T> IPropertyGetterSetup<T>.Do(Action<T> callback)
	{
		Callback<Action<int, T>> item = new(Delegate);
		_currentCallback = item;
		(_getterCallbacks ??= []).Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T v)
		{
			callback(v);
		}
	}

	/// <inheritdoc cref="IPropertyGetterSetup{T}.Do(Action{int, T})" />
	IPropertySetupCallbackBuilder<T> IPropertyGetterSetup<T>.Do(Action<int, T> callback)
	{
		Callback<Action<int, T>> item = new(callback);
		_currentCallback = item;
		(_getterCallbacks ??= []).Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.OnSet" />
	public IPropertySetterSetup<T> OnSet
		=> this;

	/// <inheritdoc cref="IPropertySetterSetup{T}.Do(Action)" />
	IPropertySetupCallbackBuilder<T> IPropertySetterSetup<T>.Do(Action callback)
	{
		Callback<Action<int, T>> item = new(Delegate);
		_currentCallback = item;
		(_setterCallbacks ??= []).Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T newValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IPropertySetterSetup{T}.Do(Action{T})" />
	IPropertySetupCallbackBuilder<T> IPropertySetterSetup<T>.Do(Action<T> callback)
	{
		Callback<Action<int, T>> item = new(Delegate);
		_currentCallback = item;
		(_setterCallbacks ??= []).Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T newValue)
		{
			callback(newValue);
		}
	}

	/// <inheritdoc cref="IPropertySetterSetup{T}.Do(Action{int, T})" />
	IPropertySetupCallbackBuilder<T> IPropertySetterSetup<T>.Do(Action<int, T> callback)
	{
		Callback<Action<int, T>> item = new(callback);
		_currentCallback = item;
		(_setterCallbacks ??= []).Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Returns(T)" />
	public IPropertySetupReturnBuilder<T> Returns(T returnValue)
	{
		Callback<Func<int, T, T>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		(_returnCallbacks ??= []).Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		T Delegate(int _, T currentValue)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IPropertySetup{T}.Returns(Func{T})" />
	public IPropertySetupReturnBuilder<T> Returns(Func<T> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		(_returnCallbacks ??= []).Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		T Delegate(int _, T currentValue)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IPropertySetup{T}.Returns(Func{T, T})" />
	public IPropertySetupReturnBuilder<T> Returns(Func<T, T> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		(_returnCallbacks ??= []).Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		T Delegate(int _, T p)
		{
			return callback(p);
		}
	}

	/// <inheritdoc cref="IPropertySetup{T}.Throws{TException}()" />
	public IPropertySetupReturnBuilder<T> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T, T>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		(_returnCallbacks ??= []).Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		T Delegate(int _, T currentValue)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IPropertySetup{T}.Throws(Exception)" />
	public IPropertySetupReturnBuilder<T> Throws(Exception exception)
	{
		Callback<Func<int, T, T>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		(_returnCallbacks ??= []).Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		T Delegate(int _, T currentValue)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IPropertySetup{T}.Throws(Func{Exception})" />
	public IPropertySetupReturnBuilder<T> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		(_returnCallbacks ??= []).Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		T Delegate(int _, T currentValue)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IPropertySetup{T}.Throws(Func{T, Exception})" />
	public IPropertySetupReturnBuilder<T> Throws(Func<T, Exception> callback)
	{
		Callback<Func<int, T, T>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		(_returnCallbacks ??= []).Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		T Delegate(int _, T p)
		{
			throw callback(p);
		}
	}

	#endregion IPropertySetup<T>
}
