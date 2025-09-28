using System.Linq;
using Mockerade.Checks;
using Mockerade.Events;
using Mockerade.Exceptions;
using Mockerade.Setup;

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
		Invoked = new MockInvoked<T>(((IMock)this).Invocations);
		Accessed = new MockAccessed<T>(((IMock)this).Invocations);
		Setup = new MockSetups<T>(this);
		Raise = new MockRaises<T>(Setup);
	}

	/// <summary>
	/// Gets the behavior settings used by this mock instance.
	/// </summary>
	MockBehavior IMock.Behavior => _behavior;

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockInvoked<T> Invoked { get; }

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockAccessed<T> Accessed { get; }

	/// <summary>
	///     Exposes the mocked object instance of type <typeparamref name="T"/>.
	/// </summary>
	public abstract T Object { get; }

	/// <summary>
	///     Raise events on the mock for <typeparamref name="T"/>.
	/// </summary>
	public MockRaises<T> Raise { get; }

	/// <summary>
	///     Setup the mock for <typeparamref name="T"/>.
	/// </summary>
	public MockSetups<T> Setup { get; }

	#region IMock

	/// <inheritdoc cref="IMock.Check" />
	IMockInvoked IMock.Check => Invoked;

	/// <inheritdoc cref="IMock.Raise" />
	IMockRaises IMock.Raise => Raise;

	/// <inheritdoc cref="IMock.Setup" />
	IMockSetup IMock.Setup => Setup;

	/// <inheritdoc cref="IMock.Invocations" />
	MockInvocations IMock.Invocations { get; } = new();

	/// <inheritdoc cref="IMock.Execute{TResult}(string, object?[])" />
	MethodSetupResult<TResult> IMock.Execute<TResult>(string methodName, params object?[] parameters)
	{
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
	MethodSetupResult IMock.Execute(string methodName, params object?[] parameters)
	{
		Invocation invocation = ((IMock)this).Invocations.RegisterInvocation(new MethodInvocation(methodName, parameters));

		MethodSetup? matchingSetup = Setup.GetMethodSetup(invocation);
		if (matchingSetup is null && _behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException($"The method '{methodName}({string.Join(",", parameters.Select(x => x?.GetType()))})' was invoked without prior setup.");
		}

		matchingSetup?.Invoke(invocation);
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
}

/// <summary>
///     A mock for type <typeparamref name="T" /> that also implements interface <typeparamref name="T2"/>.
/// </summary>
public abstract class Mock<T, T2> : Mock<T>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior) : base(behavior)
	{
	}
}
