using System;
using System.Diagnostics;
using System.Reflection;
using Mockolate.Internals;

namespace Mockolate.Setup;

/// <summary>
///     Sets up event subscription and unsubscription callbacks.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class EventSetup(MockRegistry mockRegistry, string name)
	: IEventSubscriptionSetup,
		IEventUnsubscriptionSetup,
		IEventSetupCallbackBuilder
{
	private Callbacks<Action<int, object?, MethodInfo>>? _subscribedCallbacks;
	private Callbacks<Action<int, object?, MethodInfo>>? _unsubscribedCallbacks;

	/// <summary>
	///     The fully-qualified name of the event.
	/// </summary>
	public string Name => name;

	/// <inheritdoc cref="IEventSetup.OnSubscribed" />
	public IEventSubscriptionSetup OnSubscribed => this;

	/// <inheritdoc cref="IEventSetup.OnUnsubscribed" />
	public IEventUnsubscriptionSetup OnUnsubscribed => this;

	/// <inheritdoc cref="IEventSetupParallelCallbackBuilder.When(Func{int, bool})" />
	IEventSetupCallbackWhenBuilder IEventSetupParallelCallbackBuilder.When(Func<int, bool> predicate)
	{
		_subscribedCallbacks?.Active?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IEventSetupCallbackBuilder.InParallel()" />
	IEventSetupParallelCallbackBuilder IEventSetupCallbackBuilder.InParallel()
	{
		_subscribedCallbacks?.Active?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IEventSetupCallbackWhenBuilder.For(int)" />
	IEventSetupCallbackWhenBuilder IEventSetupCallbackWhenBuilder.For(int times)
	{
		_subscribedCallbacks?.Active?.For(times);
		return this;
	}

	/// <inheritdoc cref="IEventSetupCallbackWhenBuilder.Only(int)" />
	IEventSetup IEventSetupCallbackWhenBuilder.Only(int times)
	{
		_subscribedCallbacks?.Active?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IEventSubscriptionSetup.Do(Action)" />
	IEventSetupCallbackBuilder IEventSubscriptionSetup.Do(Action callback)
	{
		Callback<Action<int, object?, MethodInfo>> item = new(Delegate);
		_subscribedCallbacks ??= [];
		_subscribedCallbacks.Active = item;
		_subscribedCallbacks.Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, object? _target, MethodInfo _method)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IEventSubscriptionSetup.Do(Action{object?, MethodInfo})" />
	IEventSetupCallbackBuilder IEventSubscriptionSetup.Do(Action<object?, MethodInfo> callback)
	{
		Callback<Action<int, object?, MethodInfo>> item = new(Delegate);
		_subscribedCallbacks ??= [];
		_subscribedCallbacks.Active = item;
		_subscribedCallbacks.Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, object? target, MethodInfo method)
		{
			callback(target, method);
		}
	}

	IEventSetupParallelCallbackBuilder IEventSubscriptionSetup.TransitionTo(string scenario)
	{
		Callback<Action<int, object?, MethodInfo>> item = new((_, _, _) => mockRegistry.TransitionTo(scenario));
		item.InParallel();
		_subscribedCallbacks ??= [];
		_subscribedCallbacks.Active = item;
		_subscribedCallbacks.Add(item);
		return this;
	}

	/// <inheritdoc cref="IEventUnsubscriptionSetup.Do(Action)" />
	IEventSetupCallbackBuilder IEventUnsubscriptionSetup.Do(Action callback)
	{
		Callback<Action<int, object?, MethodInfo>> item = new(Delegate);
		_subscribedCallbacks ??= [];
		_subscribedCallbacks.Active = item;
		(_unsubscribedCallbacks ??= []).Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, object? _target, MethodInfo _method)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IEventUnsubscriptionSetup.Do(Action{object?, MethodInfo})" />
	IEventSetupCallbackBuilder IEventUnsubscriptionSetup.Do(Action<object?, MethodInfo> callback)
	{
		Callback<Action<int, object?, MethodInfo>> item = new(Delegate);
		_subscribedCallbacks ??= [];
		_subscribedCallbacks.Active = item;
		(_unsubscribedCallbacks ??= []).Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, object? target, MethodInfo method)
		{
			callback(target, method);
		}
	}

	IEventSetupParallelCallbackBuilder IEventUnsubscriptionSetup.TransitionTo(string scenario)
	{
		Callback<Action<int, object?, MethodInfo>> item = new((_, _, _) => mockRegistry.TransitionTo(scenario));
		item.InParallel();
		_subscribedCallbacks ??= [];
		_subscribedCallbacks.Active = item;
		(_unsubscribedCallbacks ??= []).Add(item);
		return this;
	}

	/// <summary>
	///     Invokes all registered subscription callbacks.
	/// </summary>
	internal void InvokeSubscribed(object? target, MethodInfo method)
	{
		if (_subscribedCallbacks is null)
		{
			return;
		}

		bool wasInvoked = false;
		int currentIndex = _subscribedCallbacks.CurrentIndex;
		for (int i = 0; i < _subscribedCallbacks.Count; i++)
		{
			Callback<Action<int, object?, MethodInfo>> callback =
				_subscribedCallbacks[(currentIndex + i) % _subscribedCallbacks.Count];
			if (callback.Invoke(wasInvoked, ref _subscribedCallbacks.CurrentIndex, Dispatch))
			{
				wasInvoked = true;
			}
		}

		[DebuggerNonUserCode]
		void Dispatch(int invocationCount, Action<int, object?, MethodInfo> @delegate)
		{
			@delegate(invocationCount, target, method);
		}
	}

	/// <summary>
	///     Invokes all registered unsubscription callbacks.
	/// </summary>
	internal void InvokeUnsubscribed(object? target, MethodInfo method)
	{
		if (_unsubscribedCallbacks is null)
		{
			return;
		}

		bool wasInvoked = false;
		int currentIndex = _unsubscribedCallbacks.CurrentIndex;
		for (int i = 0; i < _unsubscribedCallbacks.Count; i++)
		{
			Callback<Action<int, object?, MethodInfo>> callback =
				_unsubscribedCallbacks[(currentIndex + i) % _unsubscribedCallbacks.Count];
			if (callback.Invoke(wasInvoked, ref _unsubscribedCallbacks.CurrentIndex, Dispatch))
			{
				wasInvoked = true;
			}
		}

		[DebuggerNonUserCode]
		void Dispatch(int invocationCount, Action<int, object?, MethodInfo> @delegate)
		{
			@delegate(invocationCount, target, method);
		}
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => name.SubstringAfterLast('.');
}
