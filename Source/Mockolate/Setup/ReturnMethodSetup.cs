using System;
using System.Diagnostics;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class ReturnMethodSetup<TReturn>
	: MethodSetup, IReturnMethodSetupCallbackBuilder<TReturn>, IReturnMethodSetupReturnBuilder<TReturn>
{
	private readonly MockRegistry _mockRegistry;
	private Callbacks<Action<int>>? _callbacks = [];
	private Callbacks<Func<int, TReturn>>? _returnCallbacks = [];
	private bool? _skipBaseClass;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}" />
	protected ReturnMethodSetup(MockRegistry mockRegistry, string name) : base(name)
	{
		_mockRegistry = mockRegistry;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.SkippingBaseClass(bool)" />
	IReturnMethodSetup<TReturn> IReturnMethodSetup<TReturn>.SkippingBaseClass(bool skipBaseClass)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.Do(Action)" />
	IReturnMethodSetupCallbackBuilder<TReturn> IReturnMethodSetup<TReturn>.Do(Action callback)
	{
		Callback<Action<int>> currentCallback = new(Delegate);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.Do(Action{int})" />
	IReturnMethodSetupCallbackBuilder<TReturn> IReturnMethodSetup<TReturn>.Do(Action<int> callback)
	{
		Callback<Action<int>> currentCallback = new(callback);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.TransitionTo(string)" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn> IReturnMethodSetup<TReturn>.TransitionTo(string scenario)
	{
		Callback<Action<int>> currentCallback = new(_ => _mockRegistry.TransitionTo(scenario));
		currentCallback.InParallel();
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.Returns(Func{TReturn})" />
	IReturnMethodSetupReturnBuilder<TReturn> IReturnMethodSetup<TReturn>.Returns(Func<TReturn> callback)
	{
		Callback<Func<int, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.Returns(TReturn)" />
	IReturnMethodSetupReturnBuilder<TReturn> IReturnMethodSetup<TReturn>.Returns(TReturn returnValue)
	{
		Callback<Func<int, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.Throws{TException}()" />
	IReturnMethodSetupReturnBuilder<TReturn> IReturnMethodSetup<TReturn>.Throws<TException>()
	{
		Callback<Func<int, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.Throws(Exception)" />
	IReturnMethodSetupReturnBuilder<TReturn> IReturnMethodSetup<TReturn>.Throws(Exception exception)
	{
		Callback<Func<int, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.Throws(Func{Exception})" />
	IReturnMethodSetupReturnBuilder<TReturn> IReturnMethodSetup<TReturn>.Throws(Func<Exception> callback)
	{
		Callback<Func<int, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn}.InParallel()" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn> IReturnMethodSetupCallbackBuilder<TReturn>.InParallel()
	{
		_callbacks?.Active?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupParallelCallbackBuilder{TReturn}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn> IReturnMethodSetupParallelCallbackBuilder<TReturn>.When(
		Func<int, bool> predicate)
	{
		_callbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn}.For(int)" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn> IReturnMethodSetupCallbackWhenBuilder<TReturn>.For(int times)
	{
		_callbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn}.Only(int)" />
	IReturnMethodSetup<TReturn> IReturnMethodSetupCallbackWhenBuilder<TReturn>.Only(int times)
	{
		_callbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn> IReturnMethodSetupReturnBuilder<TReturn>.When(
		Func<int, bool> predicate)
	{
		_returnCallbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn}.For(int)" />
	IReturnMethodSetupReturnWhenBuilder<TReturn> IReturnMethodSetupReturnWhenBuilder<TReturn>.For(int times)
	{
		_returnCallbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn}.Only(int)" />
	IReturnMethodSetup<TReturn> IReturnMethodSetupReturnWhenBuilder<TReturn>.Only(int times)
	{
		_returnCallbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.MatchesInteraction(IMethodInteraction)" />
	protected override bool MatchesInteraction(IMethodInteraction interaction)
		=> interaction is MethodInvocation;

	/// <summary>
	///     Flag indicating, if any return callbacks have been registered on this setup.
	/// </summary>
	public bool HasReturnCallbacks => _returnCallbacks is { Count: > 0, };

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	public bool SkipBaseClass(MockBehavior behavior)
		=> _skipBaseClass ?? behavior.SkipBaseClass;

	/// <summary>
	///     Tries to get the registered return value if any is set up.
	/// </summary>
	public bool TryGetReturnValue(out TReturn returnValue)
	{
		if (_returnCallbacks != null)
		{
			foreach (Callback<Func<int, TReturn>> _ in _returnCallbacks)
			{
				Callback<Func<int, TReturn>> returnCallback =
					_returnCallbacks[_returnCallbacks.CurrentIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _returnCallbacks.CurrentIndex, Callback, out TReturn? newValue))
				{
					returnValue = newValue;
					return true;
				}
			}
		}

		returnValue = default!;
		return false;

		[DebuggerNonUserCode]
		TReturn Callback(int invocationCount, Func<int, TReturn> @delegate)
		{
			return @delegate(invocationCount);
		}
	}

	/// <summary>
	///     Check if the setup matches the specified parameters.
	/// </summary>
	public abstract bool Matches();

	/// <summary>
	///     Triggers any configured parameter callbacks for the method setup without parameters.
	/// </summary>
	public virtual void TriggerCallbacks()
	{
		bool wasInvoked = false;
		if (_callbacks is not null)
		{
			int currentCallbacksIndex = _callbacks.CurrentIndex;
			for (int i = 0; i < _callbacks.Count; i++)
			{
				Callback<Action<int>> callback =
					_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
				if (callback.Invoke(wasInvoked, ref _callbacks.CurrentIndex, Callback))
				{
					wasInvoked = true;
				}
			}
		}

		[DebuggerNonUserCode]
		void Callback(int invocationCount, Action<int> @delegate)
		{
			@delegate(invocationCount);
		}
	}

	/// <summary>
	///     Setup for a method with one parameter matching against individual <see cref="IParameterMatch{T}" />.
	/// </summary>
	public class WithParameterCollection : ReturnMethodSetup<TReturn>
	{
		/// <inheritdoc cref="ReturnMethodSetup{TReturn}" />
		public WithParameterCollection(MockRegistry mockRegistry, string name)
			: base(mockRegistry, name)
		{
		}

		/// <inheritdoc cref="ReturnMethodSetup{TReturn}.Matches()" />
		public override bool Matches()
			=> true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{FormatType(typeof(TReturn))} {Name.SubstringAfterLast('.')}()";
	}
}

/// <summary>
///     Setup for a method with one parameter <typeparamref name="T1" /> returning <typeparamref name="TReturn" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class ReturnMethodSetup<TReturn, T1> : MethodSetup,
	IReturnMethodSetupCallbackBuilder<TReturn, T1>, IReturnMethodSetupReturnBuilder<TReturn, T1>
{
	private readonly MockRegistry _mockRegistry;
	private Callbacks<Action<int, T1>>? _callbacks = [];
	private Callbacks<Func<int, T1, TReturn>>? _returnCallbacks = [];
	private bool? _skipBaseClass;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}" />
	protected ReturnMethodSetup(MockRegistry mockRegistry, string name) : base(name)
	{
		_mockRegistry = mockRegistry;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.SkippingBaseClass(bool)" />
	IReturnMethodSetup<TReturn, T1> IReturnMethodSetup<TReturn, T1>.SkippingBaseClass(bool skipBaseClass)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Do(Action)" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Do(Action callback)
	{
		Callback<Action<int, T1>> currentCallback = new(Delegate);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Do(Action{T1})" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Do(Action<T1> callback)
	{
		Callback<Action<int, T1>> currentCallback = new(Delegate);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1)
		{
			callback(p1);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Do(Action{int, T1})" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Do(Action<int, T1> callback)
	{
		Callback<Action<int, T1>> currentCallback = new(callback);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.TransitionTo(string)" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.TransitionTo(
		string scenario)
	{
		Callback<Action<int, T1>> currentCallback = new((_, _) => _mockRegistry.TransitionTo(scenario));
		currentCallback.InParallel();
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Returns(Func{T1, TReturn})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Returns(Func<T1, TReturn> callback)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1)
		{
			return callback(p1);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Returns(Func{TReturn})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Returns(Func<TReturn> callback)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Returns(TReturn)" />
	IReturnMethodSetupReturnBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Returns(TReturn returnValue)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Throws{TException}()" />
	IReturnMethodSetupReturnBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Throws<TException>()
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Throws(Exception)" />
	IReturnMethodSetupReturnBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Throws(Exception exception)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Throws(Func{Exception})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Throws(Func<Exception> callback)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Throws(Func{T1, Exception})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1> IReturnMethodSetup<TReturn, T1>.Throws(Func<T1, Exception> callback)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1)
		{
			throw callback(p1);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1}.InParallel()" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn, T1> IReturnMethodSetupCallbackBuilder<TReturn, T1>.InParallel()
	{
		_callbacks?.Active?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupParallelCallbackBuilder{TReturn, T1}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1> IReturnMethodSetupParallelCallbackBuilder<TReturn, T1>.When(
		Func<int, bool> predicate)
	{
		_callbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1}.For(int)" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1>.For(int times)
	{
		_callbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1}.Only(int)" />
	IReturnMethodSetup<TReturn, T1> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1>.Only(int times)
	{
		_callbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn, T1}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1> IReturnMethodSetupReturnBuilder<TReturn, T1>.When(
		Func<int, bool> predicate)
	{
		_returnCallbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1}.For(int)" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1> IReturnMethodSetupReturnWhenBuilder<TReturn, T1>.For(int times)
	{
		_returnCallbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1}.Only(int)" />
	IReturnMethodSetup<TReturn, T1> IReturnMethodSetupReturnWhenBuilder<TReturn, T1>.Only(int times)
	{
		_returnCallbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.MatchesInteraction(IMethodInteraction)" />
	protected override bool MatchesInteraction(IMethodInteraction interaction)
	{
		if (interaction is MethodInvocation<T1> invocation)
		{
			return Matches(invocation.ParameterName1, invocation.Parameter1);
		}

		return false;
	}

	/// <summary>
	///     Flag indicating, if any return callbacks have been registered on this setup.
	/// </summary>
	public bool HasReturnCallbacks => _returnCallbacks is { Count: > 0, };

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	public bool SkipBaseClass(MockBehavior behavior)
		=> _skipBaseClass ?? behavior.SkipBaseClass;

	/// <summary>
	///     Tries to get the registered return value if any is set up for the specified parameter value <paramref name="p1" />.
	/// </summary>
	public bool TryGetReturnValue(T1 p1, out TReturn returnValue)
	{
		if (_returnCallbacks != null)
		{
			foreach (Callback<Func<int, T1, TReturn>> _ in _returnCallbacks)
			{
				Callback<Func<int, T1, TReturn>> returnCallback =
					_returnCallbacks[_returnCallbacks.CurrentIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _returnCallbacks.CurrentIndex, Callback, out TReturn? newValue))
				{
					returnValue = newValue;
					return true;
				}
			}
		}

		returnValue = default!;
		return false;

		[DebuggerNonUserCode]
		TReturn Callback(int invocationCount, Func<int, T1, TReturn> @delegate)
		{
			return @delegate(invocationCount, p1);
		}
	}

	/// <summary>
	///     Check if the setup matches the specified parameter value <paramref name="p1Value" /> for the parameter with the
	///     specified name <paramref name="p1Name" />.
	/// </summary>
	public abstract bool Matches(string p1Name, T1 p1Value);

	/// <summary>
	///     Triggers any configured parameter callbacks for the method setup with the specified <paramref name="parameter1" />.
	/// </summary>
	public virtual void TriggerCallbacks(T1 parameter1)
	{
		if (_callbacks is not null)
		{
			bool wasInvoked = false;
			int currentCallbacksIndex = _callbacks.CurrentIndex;
			for (int i = 0; i < _callbacks.Count; i++)
			{
				Callback<Action<int, T1>> callback =
					_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
				if (callback.Invoke(wasInvoked, ref _callbacks.CurrentIndex, Callback))
				{
					wasInvoked = true;
				}
			}
		}

		[DebuggerNonUserCode]
		void Callback(int invocationCount, Action<int, T1> @delegate)
		{
			@delegate(invocationCount, parameter1);
		}
	}

	/// <summary>
	///     Setup for a method with one parameter matching against <see cref="IParameters" />.
	/// </summary>
	public class WithParameters : ReturnMethodSetup<TReturn, T1>
	{
		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}" />
		public WithParameters(MockRegistry mockRegistry, string name, IParameters parameters)
			: base(mockRegistry, name)
		{
			Parameters = parameters;
		}

		/// <summary>
		///     The parameters match for the method.
		/// </summary>
		private IParameters Parameters { get; }

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}.Matches(string, T1)" />
		public override bool Matches(string p1Name, T1 p1Value)
			=> Parameters switch
			{
				IParametersMatch m => m.Matches([p1Value,]),
				INamedParametersMatch m => m.Matches([(p1Name, p1Value),]),
				_ => true,
			};

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{FormatType(typeof(TReturn))} {Name.SubstringAfterLast('.')}({Parameters})";
	}

	/// <summary>
	///     Setup for a method with one parameter matching against individual <see cref="IParameterMatch{T}" />.
	/// </summary>
	public class WithParameterCollection : ReturnMethodSetup<TReturn, T1>,
		IReturnMethodSetupParameterIgnorer<TReturn, T1>
	{
		private bool _matchAnyParameters;

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}" />
		public WithParameterCollection(MockRegistry mockRegistry, string name, IParameterMatch<T1> parameter1)
			: base(mockRegistry, name)
		{
			Parameter1 = parameter1;
		}

		/// <summary>
		///     The single parameter of the method.
		/// </summary>
		public IParameterMatch<T1> Parameter1 { get; }

		/// <inheritdoc cref="IReturnMethodSetupParameterIgnorer{TReturn, T1}.AnyParameters()" />
		IReturnMethodSetup<TReturn, T1> IReturnMethodSetupParameterIgnorer<TReturn, T1>.AnyParameters()
		{
			_matchAnyParameters = true;
			return this;
		}

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}.Matches(string, T1)" />
		public override bool Matches(string p1Name, T1 p1Value)
			=> _matchAnyParameters || Parameter1.Matches(p1Value);

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}.TriggerCallbacks(T1)" />
		public override void TriggerCallbacks(T1 parameter1)
		{
			Parameter1.InvokeCallbacks(parameter1);
			base.TriggerCallbacks(parameter1);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{FormatType(typeof(TReturn))} {Name.SubstringAfterLast('.')}({Parameter1})";
	}
}

/// <summary>
///     Setup for a method with two parameters <typeparamref name="T1" /> and <typeparamref name="T2" /> returning
///     <typeparamref name="TReturn" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class ReturnMethodSetup<TReturn, T1, T2> : MethodSetup,
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2>, IReturnMethodSetupReturnBuilder<TReturn, T1, T2>
{
	private readonly MockRegistry _mockRegistry;
	private Callbacks<Action<int, T1, T2>>? _callbacks = [];
	private Callbacks<Func<int, T1, T2, TReturn>>? _returnCallbacks = [];
	private bool? _skipBaseClass;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}" />
	protected ReturnMethodSetup(MockRegistry mockRegistry, string name) : base(name)
	{
		_mockRegistry = mockRegistry;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.SkippingBaseClass(bool)" />
	IReturnMethodSetup<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.SkippingBaseClass(bool skipBaseClass)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Do(Action)" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Do(Action callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new(Delegate);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Do(Action{T1, T2})" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Do(Action<T1, T2> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new(Delegate);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2)
		{
			callback(p1, p2);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Do(Action{int, T1, T2})" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Do(
		Action<int, T1, T2> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new(callback);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.TransitionTo(string)" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.TransitionTo(
		string scenario)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, _, _) => _mockRegistry.TransitionTo(scenario));
		currentCallback.InParallel();
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Returns(Func{T1, T2, TReturn})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Returns(
		Func<T1, T2, TReturn> callback)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2)
		{
			return callback(p1, p2);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Returns(Func{TReturn})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Returns(Func<TReturn> callback)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Returns(TReturn)" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Returns(TReturn returnValue)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Throws{TException}()" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Throws<TException>()
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Throws(Exception)" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Throws(Exception exception)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Throws(Func{Exception})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Throws(
		Func<Exception> callback)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.Throws(Func{T1, T2, Exception})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> IReturnMethodSetup<TReturn, T1, T2>.Throws(
		Func<T1, T2, Exception> callback)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2)
		{
			throw callback(p1, p2);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1, T2}.InParallel()" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn, T1, T2> IReturnMethodSetupCallbackBuilder<TReturn, T1, T2>.InParallel()
	{
		_callbacks?.Active?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupParallelCallbackBuilder{TReturn, T1, T2}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2> IReturnMethodSetupParallelCallbackBuilder<TReturn, T1, T2>.When(
		Func<int, bool> predicate)
	{
		_callbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2}.For(int)" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2>.
		For(int times)
	{
		_callbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2}.Only(int)" />
	IReturnMethodSetup<TReturn, T1, T2> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2>.Only(int times)
	{
		_callbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2> IReturnMethodSetupReturnBuilder<TReturn, T1, T2>.When(
		Func<int, bool> predicate)
	{
		_returnCallbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1, T2}.For(int)" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2> IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2>.
		For(int times)
	{
		_returnCallbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1, T2}.Only(int)" />
	IReturnMethodSetup<TReturn, T1, T2> IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2>.Only(int times)
	{
		_returnCallbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.MatchesInteraction(IMethodInteraction)" />
	protected override bool MatchesInteraction(IMethodInteraction interaction)
	{
		if (interaction is MethodInvocation<T1, T2> invocation)
		{
			return Matches(invocation.ParameterName1, invocation.Parameter1, invocation.ParameterName2, invocation.Parameter2);
		}

		return false;
	}

	/// <summary>
	///     Flag indicating, if any return callbacks have been registered on this setup.
	/// </summary>
	public bool HasReturnCallbacks => _returnCallbacks is { Count: > 0, };

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	public bool SkipBaseClass(MockBehavior behavior)
		=> _skipBaseClass ?? behavior.SkipBaseClass;

	/// <summary>
	///     Tries to get the registered return value if any is set up for the specified parameter values.
	/// </summary>
	public bool TryGetReturnValue(T1 p1, T2 p2, out TReturn returnValue)
	{
		if (_returnCallbacks != null)
		{
			foreach (Callback<Func<int, T1, T2, TReturn>> _ in _returnCallbacks)
			{
				Callback<Func<int, T1, T2, TReturn>> returnCallback =
					_returnCallbacks[_returnCallbacks.CurrentIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _returnCallbacks.CurrentIndex, Callback, out TReturn? newValue))
				{
					returnValue = newValue;
					return true;
				}
			}
		}

		returnValue = default!;
		return false;

		[DebuggerNonUserCode]
		TReturn Callback(int invocationCount, Func<int, T1, T2, TReturn> @delegate)
		{
			return @delegate(invocationCount, p1, p2);
		}
	}

	/// <summary>
	///     Check if the setup matches the specified parameter values <paramref name="p1Value" /> and
	///     <paramref name="p2Value" />.
	/// </summary>
	public abstract bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value);

	/// <summary>
	///     Triggers any configured parameter callbacks for the method setup with the specified parameters.
	/// </summary>
	public virtual void TriggerCallbacks(T1 parameter1, T2 parameter2)
	{
		if (_callbacks is not null)
		{
			bool wasInvoked = false;
			int currentCallbacksIndex = _callbacks.CurrentIndex;
			for (int i = 0; i < _callbacks.Count; i++)
			{
				Callback<Action<int, T1, T2>> callback =
					_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
				if (callback.Invoke(wasInvoked, ref _callbacks.CurrentIndex, Callback))
				{
					wasInvoked = true;
				}
			}
		}

		[DebuggerNonUserCode]
		void Callback(int invocationCount, Action<int, T1, T2> @delegate)
		{
			@delegate(invocationCount, parameter1, parameter2);
		}
	}

	/// <summary>
	///     Setup for a method with two parameters matching against <see cref="IParameters" />.
	/// </summary>
	public class WithParameters : ReturnMethodSetup<TReturn, T1, T2>
	{
		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}" />
		public WithParameters(MockRegistry mockRegistry, string name, IParameters parameters)
			: base(mockRegistry, name)
		{
			Parameters = parameters;
		}

		/// <summary>
		///     The parameters match for the method.
		/// </summary>
		private IParameters Parameters { get; }

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}.Matches(string, T1, string, T2)" />
		public override bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value)
			=> Parameters switch
			{
				IParametersMatch m => m.Matches([p1Value, p2Value,]),
				INamedParametersMatch m => m.Matches([(p1Name, p1Value), (p2Name, p2Value),]),
				_ => true,
			};

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{FormatType(typeof(TReturn))} {Name.SubstringAfterLast('.')}({Parameters})";
	}

	/// <summary>
	///     Setup for a method with two parameters matching against individual typed parameters.
	/// </summary>
	public class WithParameterCollection : ReturnMethodSetup<TReturn, T1, T2>,
		IReturnMethodSetupParameterIgnorer<TReturn, T1, T2>
	{
		private bool _matchAnyParameters;

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}" />
		public WithParameterCollection(MockRegistry mockRegistry, string name, IParameterMatch<T1> parameter1,
			IParameterMatch<T2> parameter2)
			: base(mockRegistry, name)
		{
			Parameter1 = parameter1;
			Parameter2 = parameter2;
		}

		/// <summary>
		///     The first parameter of the method.
		/// </summary>
		public IParameterMatch<T1> Parameter1 { get; }

		/// <summary>
		///     The second parameter of the method.
		/// </summary>
		public IParameterMatch<T2> Parameter2 { get; }

		/// <inheritdoc cref="IReturnMethodSetupParameterIgnorer{TReturn, T1, T2}.AnyParameters()" />
		IReturnMethodSetup<TReturn, T1, T2> IReturnMethodSetupParameterIgnorer<TReturn, T1, T2>.AnyParameters()
		{
			_matchAnyParameters = true;
			return this;
		}

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}.Matches(string, T1, string, T2)" />
		public override bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value)
			=> _matchAnyParameters ||
			   (Parameter1.Matches(p1Value) &&
			    Parameter2.Matches(p2Value));

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}.TriggerCallbacks(T1, T2)" />
		public override void TriggerCallbacks(T1 parameter1, T2 parameter2)
		{
			Parameter1.InvokeCallbacks(parameter1);
			Parameter2.InvokeCallbacks(parameter2);
			base.TriggerCallbacks(parameter1, parameter2);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{FormatType(typeof(TReturn))} {Name.SubstringAfterLast('.')}({Parameter1}, {Parameter2})";
	}
}

/// <summary>
///     Setup for a method with three parameters <typeparamref name="T1" />, <typeparamref name="T2" /> and
///     <typeparamref name="T3" /> returning <typeparamref name="TReturn" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class ReturnMethodSetup<TReturn, T1, T2, T3> : MethodSetup,
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3>, IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3>
{
	private readonly MockRegistry _mockRegistry;
	private Callbacks<Action<int, T1, T2, T3>>? _callbacks = [];
	private Callbacks<Func<int, T1, T2, T3, TReturn>>? _returnCallbacks = [];
	private bool? _skipBaseClass;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}" />
	protected ReturnMethodSetup(MockRegistry mockRegistry, string name) : base(name)
	{
		_mockRegistry = mockRegistry;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.SkippingBaseClass(bool)" />
	IReturnMethodSetup<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.SkippingBaseClass(
		bool skipBaseClass)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Do(Action)" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Do(Action callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new(Delegate);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Do(Action{T1, T2, T3})" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Do(
		Action<T1, T2, T3> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new(Delegate);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3)
		{
			callback(p1, p2, p3);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Do(Action{int, T1, T2, T3})" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Do(
		Action<int, T1, T2, T3> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new(callback);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.TransitionTo(string)" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.TransitionTo(
		string scenario)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, _, _, _) => _mockRegistry.TransitionTo(scenario));
		currentCallback.InParallel();
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Returns(Func{T1, T2, T3, TReturn})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Returns(
		Func<T1, T2, T3, TReturn> callback)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3)
		{
			return callback(p1, p2, p3);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Returns(Func{TReturn})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Returns(
		Func<TReturn> callback)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Returns(TReturn)" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Returns(
		TReturn returnValue)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Throws{TException}()" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Throws<TException>()
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Throws(Exception)" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Throws(
		Exception exception)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Throws(Func{Exception})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Throws(
		Func<Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.Throws(Func{T1, T2, T3, Exception})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> IReturnMethodSetup<TReturn, T1, T2, T3>.Throws(
		Func<T1, T2, T3, Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3)
		{
			throw callback(p1, p2, p3);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1, T2, T3}.InParallel()" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn, T1, T2, T3> IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3>.
		InParallel()
	{
		_callbacks?.Active?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupParallelCallbackBuilder{TReturn, T1, T2, T3}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3> IReturnMethodSetupParallelCallbackBuilder<TReturn, T1, T2, T3>.
		When(Func<int, bool> predicate)
	{
		_callbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2, T3}.For(int)" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3>
		IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3>.For(int times)
	{
		_callbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2, T3}.Only(int)" />
	IReturnMethodSetup<TReturn, T1, T2, T3> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3>.Only(int times)
	{
		_callbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3> IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3>.When(
		Func<int, bool> predicate)
	{
		_returnCallbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1, T2, T3}.For(int)" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3> IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3>.
		For(int times)
	{
		_returnCallbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1, T2, T3}.Only(int)" />
	IReturnMethodSetup<TReturn, T1, T2, T3> IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3>.Only(int times)
	{
		_returnCallbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.MatchesInteraction(IMethodInteraction)" />
	protected override bool MatchesInteraction(IMethodInteraction interaction)
	{
		if (interaction is MethodInvocation<T1, T2, T3> invocation)
		{
			return Matches(invocation.ParameterName1, invocation.Parameter1, invocation.ParameterName2, invocation.Parameter2, invocation.ParameterName3, invocation.Parameter3);
		}

		return false;
	}

	/// <summary>
	///     Flag indicating, if any return callbacks have been registered on this setup.
	/// </summary>
	public bool HasReturnCallbacks => _returnCallbacks is { Count: > 0, };

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	public bool SkipBaseClass(MockBehavior behavior)
		=> _skipBaseClass ?? behavior.SkipBaseClass;

	/// <summary>
	///     Tries to get the registered return value if any is set up for the specified parameter values.
	/// </summary>
	public bool TryGetReturnValue(T1 p1, T2 p2, T3 p3, out TReturn returnValue)
	{
		if (_returnCallbacks != null)
		{
			foreach (Callback<Func<int, T1, T2, T3, TReturn>> _ in _returnCallbacks)
			{
				Callback<Func<int, T1, T2, T3, TReturn>> returnCallback =
					_returnCallbacks[_returnCallbacks.CurrentIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _returnCallbacks.CurrentIndex, Callback, out TReturn? newValue))
				{
					returnValue = newValue;
					return true;
				}
			}
		}

		returnValue = default!;
		return false;

		[DebuggerNonUserCode]
		TReturn Callback(int invocationCount, Func<int, T1, T2, T3, TReturn> @delegate)
		{
			return @delegate(invocationCount, p1, p2, p3);
		}
	}

	/// <summary>
	///     Check if the setup matches the specified parameter values <paramref name="p1Value" />,
	///     <paramref name="p2Value" /> and <paramref name="p3Value" />.
	/// </summary>
	public abstract bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value, string p3Name, T3 p3Value);

	/// <summary>
	///     Triggers any configured parameter callbacks for the method setup with the specified parameters.
	/// </summary>
	public virtual void TriggerCallbacks(T1 parameter1, T2 parameter2, T3 parameter3)
	{
		if (_callbacks is not null)
		{
			bool wasInvoked = false;
			int currentCallbacksIndex = _callbacks.CurrentIndex;
			for (int i = 0; i < _callbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, T3>> callback =
					_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
				if (callback.Invoke(wasInvoked, ref _callbacks.CurrentIndex, Callback))
				{
					wasInvoked = true;
				}
			}
		}

		[DebuggerNonUserCode]
		void Callback(int invocationCount, Action<int, T1, T2, T3> @delegate)
		{
			@delegate(invocationCount, parameter1, parameter2, parameter3);
		}
	}

	/// <summary>
	///     Setup for a method with three parameters matching against <see cref="IParameters" />.
	/// </summary>
	public class WithParameters : ReturnMethodSetup<TReturn, T1, T2, T3>
	{
		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}" />
		public WithParameters(MockRegistry mockRegistry, string name, IParameters parameters)
			: base(mockRegistry, name)
		{
			Parameters = parameters;
		}

		/// <summary>
		///     The parameters match for the method.
		/// </summary>
		private IParameters Parameters { get; }

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}.Matches(string, T1, string, T2, string, T3)" />
		public override bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value, string p3Name, T3 p3Value)
			=> Parameters switch
			{
				IParametersMatch m => m.Matches([p1Value, p2Value, p3Value,]),
				INamedParametersMatch m => m.Matches([(p1Name, p1Value), (p2Name, p2Value), (p3Name, p3Value),]),
				_ => true,
			};

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{FormatType(typeof(TReturn))} {Name.SubstringAfterLast('.')}({Parameters})";
	}

	/// <summary>
	///     Setup for a method with three parameters matching against individual typed parameters.
	/// </summary>
	public class WithParameterCollection : ReturnMethodSetup<TReturn, T1, T2, T3>,
		IReturnMethodSetupParameterIgnorer<TReturn, T1, T2, T3>
	{
		private bool _matchAnyParameters;

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}" />
		public WithParameterCollection(
			MockRegistry mockRegistry,
			string name,
			IParameterMatch<T1> parameter1,
			IParameterMatch<T2> parameter2,
			IParameterMatch<T3> parameter3)
			: base(mockRegistry, name)
		{
			Parameter1 = parameter1;
			Parameter2 = parameter2;
			Parameter3 = parameter3;
		}

		/// <summary>
		///     The first parameter of the method.
		/// </summary>
		public IParameterMatch<T1> Parameter1 { get; }

		/// <summary>
		///     The second parameter of the method.
		/// </summary>
		public IParameterMatch<T2> Parameter2 { get; }

		/// <summary>
		///     The third parameter of the method.
		/// </summary>
		public IParameterMatch<T3> Parameter3 { get; }

		/// <inheritdoc cref="IReturnMethodSetupParameterIgnorer{TReturn, T1, T2, T3}.AnyParameters()" />
		IReturnMethodSetup<TReturn, T1, T2, T3> IReturnMethodSetupParameterIgnorer<TReturn, T1, T2, T3>.AnyParameters()
		{
			_matchAnyParameters = true;
			return this;
		}

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}.Matches(string, T1, string, T2, string, T3)" />
		public override bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value, string p3Name, T3 p3Value)
			=> _matchAnyParameters ||
			   (Parameter1.Matches(p1Value) &&
			    Parameter2.Matches(p2Value) &&
			    Parameter3.Matches(p3Value));

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}.TriggerCallbacks(T1, T2, T3)" />
		public override void TriggerCallbacks(T1 parameter1, T2 parameter2, T3 parameter3)
		{
			Parameter1.InvokeCallbacks(parameter1);
			Parameter2.InvokeCallbacks(parameter2);
			Parameter3.InvokeCallbacks(parameter3);
			base.TriggerCallbacks(parameter1, parameter2, parameter3);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{FormatType(typeof(TReturn))} {Name.SubstringAfterLast('.')}({Parameter1}, {Parameter2}, {Parameter3})";
	}
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Setup for a method with four parameters <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" /> returning <typeparamref name="TReturn" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class ReturnMethodSetup<TReturn, T1, T2, T3, T4> : MethodSetup,
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4>,
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4>
{
	private readonly MockRegistry _mockRegistry;
	private Callbacks<Action<int, T1, T2, T3, T4>>? _callbacks = [];
	private Callbacks<Func<int, T1, T2, T3, T4, TReturn>>? _returnCallbacks = [];
	private bool? _skipBaseClass;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}" />
	protected ReturnMethodSetup(MockRegistry mockRegistry, string name) : base(name)
	{
		_mockRegistry = mockRegistry;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.SkippingBaseClass(bool)" />
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.SkippingBaseClass(
		bool skipBaseClass)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Do(Action)" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.Do(
		Action callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new(Delegate);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Do(Action{T1, T2, T3, T4})" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.Do(
		Action<T1, T2, T3, T4> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new(Delegate);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			callback(p1, p2, p3, p4);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Do(Action{int, T1, T2, T3, T4})" />
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.Do(
		Action<int, T1, T2, T3, T4> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new(callback);
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.TransitionTo(string)" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.TransitionTo(
		string scenario)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, _, _, _, _) => _mockRegistry.TransitionTo(scenario));
		currentCallback.InParallel();
		_callbacks ??= [];
		_callbacks.Active = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Returns(Func{T1, T2, T3, T4, TReturn})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.Returns(
		Func<T1, T2, T3, T4, TReturn> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			return callback(p1, p2, p3, p4);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Returns(Func{TReturn})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.Returns(
		Func<TReturn> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Returns(TReturn)" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.Returns(
		TReturn returnValue)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Throws{TException}()" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.
		Throws<TException>()
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Throws(Exception)" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.Throws(
		Exception exception)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Throws(Func{Exception})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.Throws(
		Func<Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.Throws(Func{T1, T2, T3, T4, Exception})" />
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> IReturnMethodSetup<TReturn, T1, T2, T3, T4>.Throws(
		Func<T1, T2, T3, T4, Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new(Delegate);
		_returnCallbacks ??= [];
		_returnCallbacks.Active = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TReturn Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			throw callback(p1, p2, p3, p4);
		}
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1, T2, T3, T4}.InParallel()" />
	IReturnMethodSetupParallelCallbackBuilder<TReturn, T1, T2, T3, T4>
		IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4>.InParallel()
	{
		_callbacks?.Active?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupParallelCallbackBuilder{TReturn, T1, T2, T3, T4}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4>
		IReturnMethodSetupParallelCallbackBuilder<TReturn, T1, T2, T3, T4>.When(Func<int, bool> predicate)
	{
		_callbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2, T3, T4}.For(int)" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4>
		IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4>.
		For(int times)
	{
		_callbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2, T3, T4}.Only(int)" />
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4>.
		Only(int times)
	{
		_callbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3, T4}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4>
		IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4>.When(Func<int, bool> predicate)
	{
		_returnCallbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1, T2, T3, T4}.For(int)" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4>
		IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4>.
		For(int times)
	{
		_returnCallbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1, T2, T3, T4}.Only(int)" />
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4>.
		Only(int times)
	{
		_returnCallbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.MatchesInteraction(IMethodInteraction)" />
	protected override bool MatchesInteraction(IMethodInteraction interaction)
	{
		if (interaction is MethodInvocation<T1, T2, T3, T4> invocation)
		{
			return Matches(invocation.ParameterName1, invocation.Parameter1, invocation.ParameterName2, invocation.Parameter2, invocation.ParameterName3, invocation.Parameter3, invocation.ParameterName4, invocation.Parameter4);
		}

		return false;
	}

	/// <summary>
	///     Flag indicating, if any return callbacks have been registered on this setup.
	/// </summary>
	public bool HasReturnCallbacks => _returnCallbacks is { Count: > 0, };

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	public bool SkipBaseClass(MockBehavior behavior)
		=> _skipBaseClass ?? behavior.SkipBaseClass;

	/// <summary>
	///     Tries to get the registered return value if any is set up for the specified parameter values.
	/// </summary>
	public bool TryGetReturnValue(T1 p1, T2 p2, T3 p3, T4 p4, out TReturn returnValue)
	{
		if (_returnCallbacks != null)
		{
			foreach (Callback<Func<int, T1, T2, T3, T4, TReturn>> _ in _returnCallbacks)
			{
				Callback<Func<int, T1, T2, T3, T4, TReturn>> returnCallback =
					_returnCallbacks[_returnCallbacks.CurrentIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _returnCallbacks.CurrentIndex, Callback, out TReturn? newValue))
				{
					returnValue = newValue;
					return true;
				}
			}
		}

		returnValue = default!;
		return false;

		[DebuggerNonUserCode]
		TReturn Callback(int invocationCount, Func<int, T1, T2, T3, T4, TReturn> @delegate)
		{
			return @delegate(invocationCount, p1, p2, p3, p4);
		}
	}

	/// <summary>
	///     Check if the setup matches the specified parameter values <paramref name="p1Value" />,
	///     <paramref name="p2Value" />, <paramref name="p3Value" /> and <paramref name="p4Value" />.
	/// </summary>
	public abstract bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value, string p3Name, T3 p3Value,
		string p4Name, T4 p4Value);

	/// <summary>
	///     Triggers any configured parameter callbacks for the method setup with the specified parameters.
	/// </summary>
	public virtual void TriggerCallbacks(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
	{
		if (_callbacks is not null)
		{
			bool wasInvoked = false;
			int currentCallbacksIndex = _callbacks.CurrentIndex;
			for (int i = 0; i < _callbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, T3, T4>> callback =
					_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
				if (callback.Invoke(wasInvoked, ref _callbacks.CurrentIndex, Callback))
				{
					wasInvoked = true;
				}
			}
		}

		[DebuggerNonUserCode]
		void Callback(int invocationCount, Action<int, T1, T2, T3, T4> @delegate)
		{
			@delegate(invocationCount, parameter1, parameter2, parameter3, parameter4);
		}
	}

	/// <summary>
	///     Setup for a method with four parameters matching against <see cref="IParameters" />.
	/// </summary>
	public class WithParameters : ReturnMethodSetup<TReturn, T1, T2, T3, T4>
	{
		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}" />
		public WithParameters(MockRegistry mockRegistry, string name, IParameters parameters)
			: base(mockRegistry, name)
		{
			Parameters = parameters;
		}

		/// <summary>
		///     The parameters match for the method.
		/// </summary>
		private IParameters Parameters { get; }

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}.Matches(string, T1, string, T2, string, T3, string, T4)" />
		public override bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value, string p3Name, T3 p3Value,
			string p4Name, T4 p4Value)
			=> Parameters switch
			{
				IParametersMatch m => m.Matches([p1Value, p2Value, p3Value, p4Value,]),
				INamedParametersMatch m => m.Matches([(p1Name, p1Value), (p2Name, p2Value), (p3Name, p3Value), (p4Name, p4Value),]),
				_ => true,
			};

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{FormatType(typeof(TReturn))} {Name.SubstringAfterLast('.')}({Parameters})";
	}

	/// <summary>
	///     Setup for a method with four parameters matching against individual typed parameters.
	/// </summary>
	public class WithParameterCollection : ReturnMethodSetup<TReturn, T1, T2, T3, T4>,
		IReturnMethodSetupParameterIgnorer<TReturn, T1, T2, T3, T4>
	{
		private bool _matchAnyParameters;

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}" />
		public WithParameterCollection(
			MockRegistry mockRegistry,
			string name,
			IParameterMatch<T1> parameter1,
			IParameterMatch<T2> parameter2,
			IParameterMatch<T3> parameter3,
			IParameterMatch<T4> parameter4)
			: base(mockRegistry, name)
		{
			Parameter1 = parameter1;
			Parameter2 = parameter2;
			Parameter3 = parameter3;
			Parameter4 = parameter4;
		}

		/// <summary>
		///     The first parameter of the method.
		/// </summary>
		public IParameterMatch<T1> Parameter1 { get; }

		/// <summary>
		///     The second parameter of the method.
		/// </summary>
		public IParameterMatch<T2> Parameter2 { get; }

		/// <summary>
		///     The third parameter of the method.
		/// </summary>
		public IParameterMatch<T3> Parameter3 { get; }

		/// <summary>
		///     The fourth parameter of the method.
		/// </summary>
		public IParameterMatch<T4> Parameter4 { get; }

		/// <inheritdoc cref="IReturnMethodSetupParameterIgnorer{TReturn, T1, T2, T3, T4}.AnyParameters()" />
		IReturnMethodSetup<TReturn, T1, T2, T3, T4> IReturnMethodSetupParameterIgnorer<TReturn, T1, T2, T3, T4>.
			AnyParameters()
		{
			_matchAnyParameters = true;
			return this;
		}

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}.Matches(string, T1, string, T2, string, T3, string, T4)" />
		public override bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value, string p3Name, T3 p3Value,
			string p4Name, T4 p4Value)
			=> _matchAnyParameters ||
			   (Parameter1.Matches(p1Value) &&
			    Parameter2.Matches(p2Value) &&
			    Parameter3.Matches(p3Value) &&
			    Parameter4.Matches(p4Value));

		/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}.TriggerCallbacks(T1, T2, T3, T4)" />
		public override void TriggerCallbacks(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
		{
			Parameter1.InvokeCallbacks(parameter1);
			Parameter2.InvokeCallbacks(parameter2);
			Parameter3.InvokeCallbacks(parameter3);
			Parameter4.InvokeCallbacks(parameter4);
			base.TriggerCallbacks(parameter1, parameter2, parameter3, parameter4);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{FormatType(typeof(TReturn))} {Name.SubstringAfterLast('.')}({Parameter1}, {Parameter2}, {Parameter3}, {Parameter4})";
	}
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
