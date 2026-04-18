using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Setup;

/// <summary>
///     Base class for property setups.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
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

	/// <inheritdoc cref="IInteractivePropertySetup.InvokeSetter{T}(IInteraction, T, MockBehavior)" />
	void IInteractivePropertySetup.InvokeSetter<T>(IInteraction invocation, T value, MockBehavior behavior)
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
	protected abstract void InvokeSetter<TValue>(TValue value, MockBehavior behavior);

	/// <summary>
	///     Invokes the getter logic and returns the value of type <typeparamref name="TResult" />.
	/// </summary>
	protected abstract TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult> defaultValueGenerator);

#if !DEBUG
	[DebuggerNonUserCode]
#endif
	internal abstract class Default(string name) : PropertySetup
	{
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
	}

#if !DEBUG
	[DebuggerNonUserCode]
#endif
	internal sealed class Default<T>(string name, T initialValue)
		: Default(name)
	{
		private T _value = initialValue;

		/// <inheritdoc cref="PropertySetup.InvokeSetter{TValue}(TValue, MockBehavior)" />
		protected override void InvokeSetter<TValue>(TValue value, MockBehavior behavior)
		{
			if (typeof(TValue) == typeof(T))
			{
				_value = Unsafe.As<TValue, T>(ref value);
			}
			else if (value is null && default(T) is null)
			{
				_value = default!;
			}
			else
			{
				throw new MockException(
					$"The property value only supports '{typeof(T).FormatType()}', but is '{typeof(TValue).FormatType()}'.");
			}
		}

		/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}(MockBehavior, Func{TResult})" />
		protected override TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult> defaultValueGenerator)
		{
			if (typeof(TResult) == typeof(T))
			{
				return Unsafe.As<T, TResult>(ref _value);
			}

			if (_value is TResult typedValue)
			{
				return typedValue;
			}

			return defaultValueGenerator.Invoke();
		}
	}
}

/// <summary>
///     Sets up a property.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class PropertySetup<T> : PropertySetup,
	IPropertySetupCallbackBuilder<T>, IPropertySetupReturnBuilder<T>,
	IPropertyGetterSetup<T>, IPropertySetterSetup<T>
{
	private readonly MockRegistry _mockRegistry;
	private readonly string _name;
	private Callbacks<Action<int, T>>? _getterCallbacks;
	private bool _isInitialized;
	private Callbacks<Func<int, T, T>>? _returnCallbacks;
	private Callbacks<Action<int, T>>? _setterCallbacks;
	private bool? _skipBaseClass;
	private T _value = default!;

	/// <summary>
	///     Sets up a property.
	/// </summary>
	public PropertySetup(MockRegistry mockRegistry, string name)
	{
		_mockRegistry = mockRegistry;
		_name = name;
	}

	/// <inheritdoc cref="PropertySetup.Name" />
	public override string Name => _name;

	/// <inheritdoc cref="PropertySetup.IsValueInitialized" />
	internal override bool IsValueInitialized => _isInitialized;

	/// <inheritdoc cref="IPropertySetupParallelCallbackBuilder{T}.When(Func{int, bool})" />
	IPropertySetupCallbackWhenBuilder<T> IPropertySetupParallelCallbackBuilder<T>.When(Func<int, bool> predicate)
	{
		_getterCallbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupCallbackBuilder{T}.InParallel()" />
	IPropertySetupParallelCallbackBuilder<T> IPropertySetupCallbackBuilder<T>.InParallel()
	{
		_getterCallbacks?.Active?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IPropertySetupCallbackWhenBuilder{T}.For(int)" />
	IPropertySetupCallbackWhenBuilder<T> IPropertySetupCallbackWhenBuilder<T>.For(int times)
	{
		_getterCallbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupCallbackWhenBuilder{T}.Only(int)" />
	IPropertySetup<T> IPropertySetupCallbackWhenBuilder<T>.Only(int times)
	{
		_getterCallbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupReturnBuilder{T}.When(Func{int, bool})" />
	IPropertySetupReturnWhenBuilder<T> IPropertySetupReturnBuilder<T>.When(Func<int, bool> predicate)
	{
		_returnCallbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupReturnWhenBuilder{T}.For(int)" />
	IPropertySetupReturnWhenBuilder<T> IPropertySetupReturnWhenBuilder<T>.For(int times)
	{
		_returnCallbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IPropertySetupReturnWhenBuilder{T}.Only(int)" />
	IPropertySetup<T> IPropertySetupReturnWhenBuilder<T>.Only(int times)
	{
		_returnCallbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="PropertySetup.Matches(PropertyAccess)" />
	protected override bool Matches(PropertyAccess propertyAccess)
		=> _name.Equals(propertyAccess.Name);

	/// <inheritdoc cref="PropertySetup.InvokeGetter{TResult}(MockBehavior, Func{TResult})" />
	protected override TResult InvokeGetter<TResult>(MockBehavior behavior, Func<TResult> defaultValueGenerator)
	{
		if (_getterCallbacks is not null)
		{
			bool wasInvoked = false;
			int currentGetterCallbacksIndex = _getterCallbacks.CurrentIndex;
			for (int i = 0; i < _getterCallbacks.Count; i++)
			{
				Callback<Action<int, T>> getterCallback =
					_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
				if (getterCallback.Invoke(wasInvoked, ref _getterCallbacks.CurrentIndex, Callback))
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
					_returnCallbacks[_returnCallbacks.CurrentIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _returnCallbacks.CurrentIndex, ReturnCallback, out T? newValue))
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

		if (_value is null && default(TResult) is null)
		{
			return default!;
		}

		throw new MockException(
			$"The property only supports '{typeof(T).FormatType()}' and not '{typeof(TResult).FormatType()}'.");

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

	/// <inheritdoc cref="PropertySetup.InvokeSetter{TValue}(TValue, MockBehavior)" />
	protected override void InvokeSetter<TValue>(TValue value, MockBehavior behavior)
	{
		T newValue;
		if (value is T v)
		{
			newValue = v;
		}
		else if (value is null && default(T) is null)
		{
			newValue = default!;
		}
		else
		{
			throw new MockException(
				$"The property value only supports '{typeof(T).FormatType()}', but is '{typeof(TValue).FormatType()}'.");
		}

		if (_setterCallbacks is not null)
		{
			bool wasInvoked = false;
			int currentSetterCallbacksIndex = _setterCallbacks.CurrentIndex;
			for (int i = 0; i < _setterCallbacks.Count; i++)
			{
				Callback<Action<int, T>> setterCallback =
					_setterCallbacks[(currentSetterCallbacksIndex + i) % _setterCallbacks.Count];
				if (setterCallback.Invoke(wasInvoked, ref _setterCallbacks.CurrentIndex, Callback))
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
		=> $"{typeof(T).FormatType()} {_name.SubstringAfterLast('.')}";

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
		_getterCallbacks ??= [];
		_getterCallbacks.Active = item;
		_getterCallbacks.Add(item);
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
		_getterCallbacks ??= [];
		_getterCallbacks.Active = item;
		_getterCallbacks.Add(item);
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
		_getterCallbacks ??= [];
		_getterCallbacks.Active = item;
		_getterCallbacks.Add(item);
		return this;
	}

	IPropertySetupParallelCallbackBuilder<T> IPropertySetterSetup<T>.TransitionTo(string scenario)
	{
		Callback<Action<int, T>> item = new((_, _) => _mockRegistry.TransitionTo(scenario));
		item.InParallel();
		_getterCallbacks ??= [];
		_getterCallbacks.Active = item;
		(_setterCallbacks ??= []).Add(item);
		return this;
	}

	IPropertySetupParallelCallbackBuilder<T> IPropertyGetterSetup<T>.TransitionTo(string scenario)
	{
		Callback<Action<int, T>> item = new((_, _) => _mockRegistry.TransitionTo(scenario));
		item.InParallel();
		_getterCallbacks ??= [];
		_getterCallbacks.Active = item;
		_getterCallbacks.Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.OnSet" />
	public IPropertySetterSetup<T> OnSet
		=> this;

	/// <inheritdoc cref="IPropertySetterSetup{T}.Do(Action)" />
	IPropertySetupCallbackBuilder<T> IPropertySetterSetup<T>.Do(Action callback)
	{
		Callback<Action<int, T>> item = new(Delegate);
		_getterCallbacks ??= [];
		_getterCallbacks.Active = item;
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
		_getterCallbacks ??= [];
		_getterCallbacks.Active = item;
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
		_getterCallbacks ??= [];
		_getterCallbacks.Active = item;
		(_setterCallbacks ??= []).Add(item);
		return this;
	}

	/// <inheritdoc cref="IPropertySetup{T}.Returns(T)" />
	public IPropertySetupReturnBuilder<T> Returns(T returnValue)
	{
		Callback<Func<int, T, T>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
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
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
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
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
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
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
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
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
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
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
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
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		T Delegate(int _, T p)
		{
			throw callback(p);
		}
	}

	#endregion IPropertySetup<T>
}
