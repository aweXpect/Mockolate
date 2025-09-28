using System.Linq;
using Mockerade.Events;
using Mockerade.Exceptions;
using Mockerade.Invocations;
using Mockerade.Setup;

namespace Mockerade;

/// <summary>
///     A mock for type <typeparamref name="T" />.
/// </summary>
public abstract class Mock<T> : IMock
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior)
	{
		Behavior = behavior;
		Invoked = new MockInvocations<T>();
		Setup = new MockSetup<T>(this);
		Raises = new MockRaises<T>(Setup);
	}

	/// <summary>
	/// Gets the behavior settings used by this mock instance.
	/// </summary>
	public MockBehavior Behavior { get; }

	/// <summary>
	///     The invocations of the mock.
	/// </summary>
	public MockInvocations<T> Invoked { get; }

	/// <summary>
	///     Exposes the mocked object instance.
	/// </summary>
	public abstract T Object { get; }

	/// <summary>
	///     Allows raising events on the mock.
	/// </summary>
	public MockRaises<T> Raises { get; }

	/// <inheritdoc cref="IMock.Raises" />
	IMockRaises IMock.Raises => Raises;

	/// <summary>
	///     Allows setting up the mock.
	/// </summary>
	public MockSetup<T> Setup { get; }

	/// <inheritdoc cref="IMock.Execute{TResult}(string, object?[])" />
	MethodSetupResult<TResult> IMock.Execute<TResult>(string methodName, params object?[] parameters)
	{
		Invocation invocation = Invoked.RegisterInvocation(new MethodInvocation(methodName, parameters));

		MethodSetup? matchingSetup = Setup.GetMethodSetup(invocation);
		if (matchingSetup is null)
		{
			if (Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException($"The method '{methodName}({string.Join(",", parameters.Select(x => x?.GetType()))})' was invoked without prior setup.");
			}

			return new MethodSetupResult<TResult>(matchingSetup, Behavior, Behavior.DefaultValueGenerator.Generate<TResult>());
		}

		return new MethodSetupResult<TResult>(matchingSetup, Behavior, matchingSetup.Invoke<TResult>(invocation, Behavior));
	}

	/// <inheritdoc cref="IMock.Execute(string, object?[])" />
	MethodSetupResult IMock.Execute(string methodName, params object?[] parameters)
	{
		Invocation invocation = Invoked.RegisterInvocation(new MethodInvocation(methodName, parameters));

		MethodSetup? matchingSetup = Setup.GetMethodSetup(invocation);
		if (matchingSetup is null && Behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException($"The method '{methodName}({string.Join(",", parameters.Select(x => x?.GetType()))})' was invoked without prior setup.");
		}

		matchingSetup?.Invoke(invocation);
		return new MethodSetupResult(matchingSetup, Behavior);
	}

	/// <inheritdoc cref="IMock.Set(string, object?)" />
	void IMock.Set(string propertyName, object? value)
	{
		Invocation invocation = Invoked.RegisterInvocation(new PropertySetterInvocation(propertyName, value));
		PropertySetup matchingSetup = Setup.GetPropertySetup(propertyName);
		matchingSetup.InvokeSetter(invocation, value);
	}

	/// <inheritdoc cref="IMock.Get{TResult}(string)" />
	TResult IMock.Get<TResult>(string propertyName)
	{
		Invocation invocation = Invoked.RegisterInvocation(new PropertyGetterInvocation(propertyName));
		PropertySetup matchingSetup = Setup.GetPropertySetup(propertyName);
		return matchingSetup.InvokeGetter<TResult>(invocation);
	}

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
