using Mockolate.Checks;
using Mockolate.Events;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     Provides protected access to mock setup, invocation, and event tracking features for the specified type parameter.
///     Enables inspection and configuration of protected members on the mocked instance of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
///     Use this class to configure and verify protected methods, properties, and events on the mocked type.
///     This is particularly useful for testing scenarios where protected members need to be accessed or validated, which is
///     not possible through standard public mocking APIs. All features exposed by this class operate on the provided mock instance.
/// </remarks>
public class ProtectedMock<T>(Mock<T> mock) : IMock
{
	private readonly Mock<T> _mock = mock;
	private readonly IMock _inner = mock;

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockInvoked<T>.Protected Invoked
		=> new MockInvoked<T>.Protected(_mock.Invoked, _inner.Invocations);

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockAccessed<T>.Protected Accessed
		=> new MockAccessed<T>.Protected(_mock.Accessed, _inner.Invocations);

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockEvent<T>.Protected Event
		=> new MockEvent<T>.Protected(_mock.Event, _inner.Invocations);

	/// <summary>
	///     Raise events on the mock for <typeparamref name="T"/>.
	/// </summary>
	public MockRaises<T>.Protected Raise
		=> new MockRaises<T>.Protected(_mock.Raise, _mock.Setup, _inner.Invocations);

	/// <summary>
	///     Sets up the mock for <typeparamref name="T"/>.
	/// </summary>
	public MockSetups<T>.Protected Setup
		=> new MockSetups<T>.Protected(_inner.Setup);

	#region IMock

	/// <summary>
	/// Gets the behavior settings used by this mock instance.
	/// </summary>
	MockBehavior IMock.Behavior
		=> _inner.Behavior;

	/// <inheritdoc cref="IMock.Check" />
	IMockInvoked IMock.Check
		=> _inner.Check;

	/// <inheritdoc cref="IMock.Raise" />
	IMockRaises IMock.Raise
		=> _inner.Raise;

	/// <inheritdoc cref="IMock.Setup" />
	IMockSetup IMock.Setup
		=> _inner.Setup;

	/// <inheritdoc cref="IMock.Invocations" />
	MockInvocations IMock.Invocations
		=> _inner.Invocations;

	/// <inheritdoc cref="IMock.Execute{TResult}(string, object?[])" />
	MethodSetupResult<TResult> IMock.Execute<TResult>(string methodName, params object?[]? parameters)
		=> _inner.Execute<TResult>(methodName, parameters);

	/// <inheritdoc cref="IMock.Execute(string, object?[])" />
	MethodSetupResult IMock.Execute(string methodName, params object?[]? parameters)
		=> _inner.Execute(methodName, parameters);

	/// <inheritdoc cref="IMock.Set(string, object?)" />
	void IMock.Set(string propertyName, object? value)
		=> _inner.Set(propertyName, value);

	/// <inheritdoc cref="IMock.Get{TResult}(string)" />
	TResult IMock.Get<TResult>(string propertyName)
		=> _inner.Get<TResult>(propertyName);

	#endregion IMock
}
