using System;
using System.Diagnostics;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq;
using Mockolate.Checks;
using Mockolate.Events;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Setup;
using static Mockolate.BaseClass;

namespace Mockolate;

/// <summary>
///     Provides a base class for creating and managing mock objects of a specified type, supporting setup, event raising,
///     and behavior configuration.
/// </summary>
[DebuggerDisplay("Setup: {Setup}, {_interactions.Count} interactions")]
public abstract class MockBase<T> : IMock
{
	private readonly Interactions.MockInteractions _interactions;
	private readonly MockBehavior _behavior;

	/// <inheritdoc cref="MockBase{T}" />
	protected MockBase(MockBehavior behavior)
	{
		_behavior = behavior;
		_interactions = new();
		Setup = new MockSetups<T>(this);
		Raise = new MockRaises<T>(Setup, _interactions);
		Check = new MockCheck(_interactions);
	}

	/// <summary>
	///     Additional checks on the mocked instance.
	/// </summary>
	public MockCheck Check { get; }

	/// <summary>
	///     Exposes the mocked object instance of type <typeparamref name="T" />.
	/// </summary>
	public abstract T Object { get; }

	/// <summary>
	///     Raise events on the mock for <typeparamref name="T" />.
	/// </summary>
	public MockRaises<T> Raise { get; }

	/// <summary>
	///     Sets up the mock for <typeparamref name="T" />.
	/// </summary>
	public MockSetups<T> Setup { get; }

	/// <summary>
	///     Implicitly converts the mock to the mocked object instance.
	/// </summary>
	/// <remarks>
	///     This does not work implicitly (but only with an explicit cast) for interfaces due to
	///     a limitation of the C# language.
	/// </remarks>
	public static implicit operator T(MockBase<T> mock) => mock.Object;

	/// <summary>
	///     Attempts to create an instance of the specified type using the provided constructor parameters.
	/// </summary>
#if NET8_0_OR_GREATER
	protected TObject Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TObject>(ConstructorParameters? constructorParameters)
#else
	protected TObject Create<TObject>(ConstructorParameters? constructorParameters)
#endif
	{
		if (constructorParameters?.Parameters.Length > 0)
		{
			try
			{
				return (TObject)Activator.CreateInstance(typeof(TObject),
					[this, .. constructorParameters.Parameters,])!;
			}
			catch
			{
				throw new MockException(
					$"Could not create an instance of '{typeof(TObject)}' with {constructorParameters.Parameters.Length} parameters ({string.Join(", ", constructorParameters.Parameters)}).");
			}
		}

		try
		{
			return (TObject)Activator.CreateInstance(typeof(TObject), this)!;
		}
		catch
		{
			throw new MockException(
				$"Could not create an instance of '{typeof(TObject)}' without constructor parameters.");
		}
	}

	#region IMock

	/// <summary>
	///     Gets the behavior settings used by this mock instance.
	/// </summary>
	MockBehavior IMock.Behavior => _behavior;

	/// <inheritdoc cref="IMock.Interactions" />
	MockInteractions IMock.Interactions
		=> _interactions;

	/// <inheritdoc cref="IMock.Setup" />
	IMockSetup IMock.Setup
		=> Setup;

	/// <inheritdoc cref="IMock.Raise" />
	IMockRaises IMock.Raise
		=> Raise;

	/// <inheritdoc cref="IMock.Execute{TResult}(string, object?[])" />
	MethodSetupResult<TResult> IMock.Execute<TResult>(string methodName, params object?[]? parameters)
	{
		MockInteractions interactions = ((IMock)this).Interactions;
		parameters ??= [null,];
		IInteraction invocation =
			interactions.RegisterInteraction(new MethodInvocation(interactions.GetNextIndex(), methodName, parameters));

		MethodSetup? matchingSetup = Setup.GetMethodSetup(invocation);
		if (matchingSetup is null)
		{
			if (_behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException(
					$"The method '{methodName}({string.Join(", ", parameters.Select(x => x?.GetType().ToString() ?? "<null>"))})' was invoked without prior setup.");
			}

			return new MethodSetupResult<TResult>(null, _behavior,
				_behavior.DefaultValueGenerator.Generate<TResult>());
		}

		return new MethodSetupResult<TResult>(matchingSetup, _behavior,
			matchingSetup.Invoke<TResult>(invocation, _behavior));
	}

	/// <inheritdoc cref="IMock.Execute(string, object?[])" />
	MethodSetupResult IMock.Execute(string methodName, params object?[]? parameters)
	{
		MockInteractions interactions = ((IMock)this).Interactions;
		parameters ??= [null,];
		IInteraction invocation =
			interactions.RegisterInteraction(new MethodInvocation(interactions.GetNextIndex(), methodName, parameters));

		MethodSetup? matchingSetup = Setup.GetMethodSetup(invocation);
		if (matchingSetup is null && _behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException(
				$"The method '{methodName}({string.Join(", ", parameters.Select(x => x?.GetType().ToString() ?? "<null>"))})' was invoked without prior setup.");
		}

		matchingSetup?.Invoke(invocation, _behavior);
		return new MethodSetupResult(matchingSetup, _behavior);
	}

	/// <inheritdoc cref="IMock.Set(string, object?)" />
	void IMock.Set(string propertyName, object? value)
	{
		MockInteractions interactions = ((IMock)this).Interactions;
		IInteraction invocation =
			interactions.RegisterInteraction(new PropertySetterAccess(interactions.GetNextIndex(), propertyName, value));
		PropertySetup matchingSetup = Setup.GetPropertySetup(propertyName);
		matchingSetup.InvokeSetter(invocation, value);
	}

	/// <inheritdoc cref="IMock.Get{TResult}(string)" />
	TResult IMock.Get<TResult>(string propertyName)
	{
		MockInteractions? checks = ((IMock)this).Interactions;
		IInteraction invocation =
			checks.RegisterInteraction(new PropertyGetterAccess(checks.GetNextIndex(), propertyName));
		PropertySetup matchingSetup = Setup.GetPropertySetup(propertyName);
		return matchingSetup.InvokeGetter<TResult>(invocation);
	}

	#endregion IMock
}

/// <summary>
///     A mock for type <typeparamref name="T" />.
/// </summary>
public abstract class Mock<T> : MockBase<T>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		MockInteractions? checks = ((IMock)this).Interactions;
		Accessed = new MockAccessed<T, Mock<T>>(checks, this);
		Event = new MockEvent<T, Mock<T>>(checks, this);
		Invoked = new MockInvoked<T, Mock<T>>(checks, this);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public MockAccessed<T, Mock<T>> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public MockEvent<T, Mock<T>> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public MockInvoked<T, Mock<T>> Invoked { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interface <typeparamref name="T2" />.
/// </summary>
public abstract class Mock<T1, T2> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}
		
		MockInteractions? checks = ((IMock)this).Interactions;
		Accessed = new MockAccessed<T1, Mock<T1, T2>>(checks, this);
		Event = new MockEvent<T1, Mock<T1, T2>>(checks, this);
		Invoked = new MockInvoked<T1, Mock<T1, T2>>(checks, this);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockAccessed<T1, Mock<T1, T2>> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockEvent<T1, Mock<T1, T2>> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockInvoked<T1, Mock<T1, T2>> Invoked { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" /> and
///     <typeparamref name="T3" />.
/// </summary>
public abstract class Mock<T1, T2, T3> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}
		
		MockInteractions? checks = ((IMock)this).Interactions;
		Accessed = new MockAccessed<T1, Mock<T1, T2, T3>>(checks, this);
		Event = new MockEvent<T1, Mock<T1, T2, T3>>(checks, this);
		Invoked = new MockInvoked<T1, Mock<T1, T2, T3>>(checks, this);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockAccessed<T1, Mock<T1, T2, T3>> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockEvent<T1, Mock<T1, T2, T3>> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockInvoked<T1, Mock<T1, T2, T3>> Invoked { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Accessed = new MockAccessed<T1, Mock<T1, T2, T3, T4>>(checks, this);
		Event = new MockEvent<T1, Mock<T1, T2, T3, T4>>(checks, this);
		Invoked = new MockInvoked<T1, Mock<T1, T2, T3, T4>>(checks, this);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockAccessed<T1, Mock<T1, T2, T3, T4>> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockEvent<T1, Mock<T1, T2, T3, T4>> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockInvoked<T1, Mock<T1, T2, T3, T4>> Invoked { get; }
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" /> and <typeparamref name="T5" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Accessed = new MockAccessed<T1, Mock<T1, T2, T3, T4, T5>>(checks, this);
		Event = new MockEvent<T1, Mock<T1, T2, T3, T4, T5>>(checks, this);
		Invoked = new MockInvoked<T1, Mock<T1, T2, T3, T4, T5>>(checks, this);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockAccessed<T1, Mock<T1, T2, T3, T4, T5>> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockEvent<T1, Mock<T1, T2, T3, T4, T5>> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockInvoked<T1, Mock<T1, T2, T3, T4, T5>> Invoked { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" /> and <typeparamref name="T6" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5, T6}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		if (!typeof(T6).IsInterface)
		{
			throw new MockException($"The sixth generic type argument '{typeof(T6)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Accessed = new MockAccessed<T1, Mock<T1, T2, T3, T4, T5, T6>>(checks, this);
		Event = new MockEvent<T1, Mock<T1, T2, T3, T4, T5, T6>>(checks, this);
		Invoked = new MockInvoked<T1, Mock<T1, T2, T3, T4, T5, T6>>(checks, this);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockAccessed<T1, Mock<T1, T2, T3, T4, T5, T6>> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockEvent<T1, Mock<T1, T2, T3, T4, T5, T6>> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockInvoked<T1, Mock<T1, T2, T3, T4, T5, T6>> Invoked { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" />, <typeparamref name="T6" /> and
///     <typeparamref name="T7" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6, T7> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5, T6, T7}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		if (!typeof(T6).IsInterface)
		{
			throw new MockException($"The sixth generic type argument '{typeof(T6)}' is no interface.");
		}

		if (!typeof(T7).IsInterface)
		{
			throw new MockException($"The seventh generic type argument '{typeof(T7)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Accessed = new MockAccessed<T1, Mock<T1, T2, T3, T4, T5, T6, T7>>(checks, this);
		Event = new MockEvent<T1, Mock<T1, T2, T3, T4, T5, T6, T7>>(checks, this);
		Invoked = new MockInvoked<T1, Mock<T1, T2, T3, T4, T5, T6, T7>>(checks, this);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockAccessed<T1, Mock<T1, T2, T3, T4, T5, T6, T7>> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockEvent<T1, Mock<T1, T2, T3, T4, T5, T6, T7>> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockInvoked<T1, Mock<T1, T2, T3, T4, T5, T6, T7>> Invoked { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" />, <typeparamref name="T6" />,
///     <typeparamref name="T7" /> and <typeparamref name="T8" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6, T7, T8> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5, T6, T7, T8}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		if (!typeof(T6).IsInterface)
		{
			throw new MockException($"The sixth generic type argument '{typeof(T6)}' is no interface.");
		}

		if (!typeof(T7).IsInterface)
		{
			throw new MockException($"The seventh generic type argument '{typeof(T7)}' is no interface.");
		}

		if (!typeof(T8).IsInterface)
		{
			throw new MockException($"The eighth generic type argument '{typeof(T8)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Accessed = new MockAccessed<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8>>(checks, this);
		Event = new MockEvent<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8>>(checks, this);
		Invoked = new MockInvoked<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8>>(checks, this);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockAccessed<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8>> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockEvent<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8>> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockInvoked<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8>> Invoked { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" />, <typeparamref name="T6" />,
///     <typeparamref name="T7" />, <typeparamref name="T8" /> and <typeparamref name="T9" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5, T6, T7, T8, T9}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		if (!typeof(T6).IsInterface)
		{
			throw new MockException($"The sixth generic type argument '{typeof(T6)}' is no interface.");
		}

		if (!typeof(T7).IsInterface)
		{
			throw new MockException($"The seventh generic type argument '{typeof(T7)}' is no interface.");
		}

		if (!typeof(T8).IsInterface)
		{
			throw new MockException($"The eighth generic type argument '{typeof(T8)}' is no interface.");
		}

		if (!typeof(T9).IsInterface)
		{
			throw new MockException($"The ninth generic type argument '{typeof(T9)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Accessed = new MockAccessed<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(checks, this);
		Event = new MockEvent<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(checks, this);
		Invoked = new MockInvoked<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(checks, this);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockAccessed<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9>> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockEvent<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9>> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T1" /> and
	///     <typeparamref name="T2" />.
	/// </summary>
	public MockInvoked<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9>> Invoked { get; }
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
