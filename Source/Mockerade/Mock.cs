using System;
using System.Linq;
using System.Reflection;
using Mockerade.Checks;
using Mockerade.Events;
using Mockerade.Exceptions;
using Mockerade.Monitor;
using Mockerade.Setup;
using static Mockerade.BaseClass;

namespace Mockerade;

/// <summary>
///     A mock for type <typeparamref name="T" />.
/// </summary>
public abstract class Mock<T> : IMock
{
	private readonly MockBehavior _behavior;

	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior)
	{
		_behavior = behavior;
		Setup = new MockSetups<T>(this);
		Accessed = new MockAccessed<T>(((IMock)this).Invocations);
		Event = new MockEvent<T>(((IMock)this).Invocations);
		Invoked = new MockInvoked<T>(((IMock)this).Invocations);
		Raise = new MockRaises<T>(Setup, ((IMock)this).Invocations);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockAccessed<T> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockEvent<T> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockInvoked<T> Invoked { get; }

	/// <summary>
	///     Exposes the mocked object instance of type <typeparamref name="T"/>.
	/// </summary>
	public abstract T Object { get; }

	/// <summary>
	///     Raise events on the mock for <typeparamref name="T"/>.
	/// </summary>
	public MockRaises<T> Raise { get; }

	/// <summary>
	///     Sets up the mock for <typeparamref name="T"/>.
	/// </summary>
	public MockSetups<T> Setup { get; }

	#region IMock

	/// <summary>
	/// Gets the behavior settings used by this mock instance.
	/// </summary>
	MockBehavior IMock.Behavior => _behavior;

	/// <inheritdoc cref="IMock.Check" />
	IMockInvoked IMock.Check
		=> Invoked;

	/// <inheritdoc cref="IMock.Raise" />
	IMockRaises IMock.Raise
		=> Raise;

	/// <inheritdoc cref="IMock.Setup" />
	IMockSetup IMock.Setup
		=> Setup;

	/// <inheritdoc cref="IMock.Invocations" />
	MockInvocations IMock.Invocations { get; } = new();

	/// <inheritdoc cref="IMock.Execute{TResult}(string, object?[])" />
	MethodSetupResult<TResult> IMock.Execute<TResult>(string methodName, params object?[]? parameters)
	{
		parameters ??= [null];
		Invocation invocation = ((IMock)this).Invocations.RegisterInvocation(new MethodInvocation(methodName, parameters));

		MethodSetup? matchingSetup = Setup.GetMethodSetup(invocation);
		if (matchingSetup is null)
		{
			if (_behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException($"The method '{methodName}({string.Join(",", parameters.Select(x => x?.GetType()))})' was invoked without prior setup.");
			}

			return new MethodSetupResult<TResult>(matchingSetup, _behavior, _behavior.DefaultValueGenerator.Generate<TResult>());
		}

		return new MethodSetupResult<TResult>(matchingSetup, _behavior, matchingSetup.Invoke<TResult>(invocation, _behavior));
	}

	/// <inheritdoc cref="IMock.Execute(string, object?[])" />
	MethodSetupResult IMock.Execute(string methodName, params object?[]? parameters)
	{
		parameters ??= [null];
		Invocation invocation = ((IMock)this).Invocations.RegisterInvocation(new MethodInvocation(methodName, parameters));

		MethodSetup? matchingSetup = Setup.GetMethodSetup(invocation);
		if (matchingSetup is null && _behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException($"The method '{methodName}({string.Join(",", parameters.Select(x => x?.GetType()))})' was invoked without prior setup.");
		}

		matchingSetup?.Invoke(invocation, _behavior);
		return new MethodSetupResult(matchingSetup, _behavior);
	}

	/// <inheritdoc cref="IMock.Set(string, object?)" />
	void IMock.Set(string propertyName, object? value)
	{
		Invocation invocation = ((IMock)this).Invocations.RegisterInvocation(new PropertySetterInvocation(propertyName, value));
		PropertySetup matchingSetup = Setup.GetPropertySetup(propertyName);
		matchingSetup.InvokeSetter(invocation, value);
	}

	/// <inheritdoc cref="IMock.Get{TResult}(string)" />
	TResult IMock.Get<TResult>(string propertyName)
	{
		Invocation invocation = ((IMock)this).Invocations.RegisterInvocation(new PropertyGetterInvocation(propertyName));
		PropertySetup matchingSetup = Setup.GetPropertySetup(propertyName);
		return matchingSetup.InvokeGetter<TResult>(invocation);
	}

	#endregion IMock

	/// <summary>
	///     Implicitly converts the mock to the mocked object instance.
	/// </summary>
	/// <remarks>
	///     This does not work implicitly (but only with an explicit cast) for interfaces due to
	///     a limitation of the C# language.
	/// </remarks>
	public static implicit operator T(Mock<T> mock)
	{
		return mock.Object;
	}

	/// <summary>
	///     Attempts to create an instance of the specified type using the provided constructor parameters.
	/// </summary>
	protected TObject TryCreate<TObject>(BaseClass.ConstructorParameters? constructorParameters)
	{
		if (constructorParameters is null)
		{
			try
			{
				return (TObject)Activator.CreateInstance(typeof(TObject), [this,])!;
			}
			catch
			{
				throw new MockException($"Could not create an instance of '{typeof(TObject)}' without constructor parameters.");
			}
		}
		else
		{
			try
			{
				return (TObject)Activator.CreateInstance(typeof(TObject), [this, .. constructorParameters.Parameters])!;
			}
			catch
			{
				throw new MockException($"Could not create an instance of '{typeof(TObject)}' with {constructorParameters.Parameters.Length} parameters ({string.Join(", ", constructorParameters.Parameters)}).");
			}
		}
	}
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interface <typeparamref name="T2"/>.
/// </summary>
public abstract class Mock<T1, T2> : Mock<T1>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}
	}
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2"/> and <typeparamref name="T3"/>.
/// </summary>
public abstract class Mock<T1, T2, T3> : Mock<T1, T2>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}
	}
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2"/>, <typeparamref name="T3"/> and <typeparamref name="T4"/>.
/// </summary>
public abstract class Mock<T1, T2, T3, T4> : Mock<T1, T2, T3>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}
	}
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/> and <typeparamref name="T5"/>.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5> : Mock<T1, T2, T3, T4>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}
	}
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/> and <typeparamref name="T6"/>.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6> : Mock<T1, T2, T3, T4, T5>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T6).IsInterface)
		{
			throw new MockException($"The sixth generic type argument '{typeof(T6)}' is no interface.");
		}
	}
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/> and <typeparamref name="T7"/>.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6, T7> : Mock<T1, T2, T3, T4, T5, T6>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T7).IsInterface)
		{
			throw new MockException($"The seventh generic type argument '{typeof(T7)}' is no interface.");
		}
	}
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>, <typeparamref name="T7"/> and <typeparamref name="T8"/>.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6, T7, T8> : Mock<T1, T2, T3, T4, T5, T6, T7>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T8).IsInterface)
		{
			throw new MockException($"The eighth generic type argument '{typeof(T8)}' is no interface.");
		}
	}
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>, <typeparamref name="T7"/>, <typeparamref name="T8"/> and <typeparamref name="T9"/>.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9> : Mock<T1, T2, T3, T4, T5, T6, T7, T8>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
		if (!typeof(T9).IsInterface)
		{
			throw new MockException($"The ninth generic type argument '{typeof(T9)}' is no interface.");
		}
	}
}
