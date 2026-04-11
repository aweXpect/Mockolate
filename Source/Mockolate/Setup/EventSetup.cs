using System;
using System.Collections.Generic;
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
public class EventSetup(string name) : IEventSetup,
	IEventSubscriptionSetup, IEventUnsubscriptionSetup,
	IEventSetupCallbackBuilder, IEventSetupCallbackWhenBuilder
{
	private Callback? _currentCallback;
	private int _currentSubscribedCallbacksIndex;
	private int _currentUnsubscribedCallbacksIndex;
	private List<Callback<Action<int, object?, MethodInfo>>>? _subscribedCallbacks;
	private List<Callback<Action<int, object?, MethodInfo>>>? _unsubscribedCallbacks;

	/// <summary>
	///     The fully-qualified name of the event.
	/// </summary>
	public string Name => name;

	/// <inheritdoc cref="IEventSetup.OnSubscribed" />
	public IEventSubscriptionSetup OnSubscribed => this;

	/// <inheritdoc cref="IEventSetup.OnUnsubscribed" />
	public IEventUnsubscriptionSetup OnUnsubscribed => this;

	/// <inheritdoc cref="IEventSetupCallbackBuilder.When(Func{int, bool})" />
	IEventSetupCallbackWhenBuilder IEventSetupCallbackBuilder.When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IEventSetupCallbackBuilder.InParallel()" />
	IEventSetupCallbackWhenBuilder IEventSetupCallbackBuilder.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IEventSetupCallbackWhenBuilder.For(int)" />
	IEventSetupCallbackWhenBuilder IEventSetupCallbackWhenBuilder.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IEventSetupCallbackWhenBuilder.Only(int)" />
	IEventSetup IEventSetupCallbackWhenBuilder.Only(int times)
	{
		_currentCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IEventSubscriptionSetup.Do(Action)" />
	IEventSetupCallbackBuilder IEventSubscriptionSetup.Do(Action callback)
	{
		Callback<Action<int, object?, MethodInfo>> item = new(Delegate);
		_currentCallback = item;
		(_subscribedCallbacks ??= []).Add(item);
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
		_currentCallback = item;
		(_subscribedCallbacks ??= []).Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, object? target, MethodInfo method)
		{
			callback(target, method);
		}
	}

	/// <inheritdoc cref="IEventUnsubscriptionSetup.Do(Action)" />
	IEventSetupCallbackBuilder IEventUnsubscriptionSetup.Do(Action callback)
	{
		Callback<Action<int, object?, MethodInfo>> item = new(Delegate);
		_currentCallback = item;
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
		_currentCallback = item;
		(_unsubscribedCallbacks ??= []).Add(item);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, object? target, MethodInfo method)
		{
			callback(target, method);
		}
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
		int currentIndex = _currentSubscribedCallbacksIndex;
		for (int i = 0; i < _subscribedCallbacks.Count; i++)
		{
			Callback<Action<int, object?, MethodInfo>> callback =
				_subscribedCallbacks[(currentIndex + i) % _subscribedCallbacks.Count];
			if (callback.Invoke(wasInvoked, ref _currentSubscribedCallbacksIndex, Dispatch))
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
		int currentIndex = _currentUnsubscribedCallbacksIndex;
		for (int i = 0; i < _unsubscribedCallbacks.Count; i++)
		{
			Callback<Action<int, object?, MethodInfo>> callback =
				_unsubscribedCallbacks[(currentIndex + i) % _unsubscribedCallbacks.Count];
			if (callback.Invoke(wasInvoked, ref _currentUnsubscribedCallbacksIndex, Dispatch))
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
