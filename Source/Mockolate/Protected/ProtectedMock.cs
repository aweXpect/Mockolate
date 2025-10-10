using Mockolate.Checks;
using Mockolate.Events;
using Mockolate.Interactions;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate.Protected;

/// <summary>
///     Provides protected access to mock setup, invocation, and event tracking features for the specified type parameter.
///     Enables inspection and configuration of protected members on the mocked instance <typeparamref name="TMock" />.
/// </summary>
/// <remarks>
///     Use this class to configure and verify protected methods, properties, and events on the mocked type.
///     This is particularly useful for testing scenarios where protected members need to be accessed or validated, which
///     is
///     not possible through standard public mocking APIs. All features exposed by this class operate on the provided mock
///     instance.
/// </remarks>
public class ProtectedMock<T, TMock>(IMock inner, MockInteractions interactions, TMock mock) : IMock
	where TMock : Mock<T>
{
	private readonly IMock _inner = inner;
	private readonly TMock _mock = mock;

	/// <summary>
	///     Verifies the protected interactions with the mocked subject of <typeparamref name="T"/>.
	/// </summary>
	public MockVerify<T, Mock<T>>.Protected Verify
		=> new(new MockVerify<T, Mock<T>>(interactions, _mock));

	/// <summary>
	///     Raise events on the mock for <typeparamref name="TMock" />.
	/// </summary>
	public MockRaises<T>.Protected Raise
		=> new(_mock.Raise, _mock.Setup, _inner.Interactions);

	/// <summary>
	///     Sets up the mock for <typeparamref name="TMock" />.
	/// </summary>
	public MockSetup<T>.Protected Setup
		=> new(_inner.Setup);

	#region IMock

	/// <summary>
	///     Gets the behavior settings used by this mock instance.
	/// </summary>
	MockBehavior IMock.Behavior
		=> _inner.Behavior;

	/// <inheritdoc cref="IMock.Interactions" />
	MockInteractions IMock.Interactions
		=> _inner.Interactions;

	/// <inheritdoc cref="IMock.Raise" />
	IMockRaises IMock.Raise
		=> Raise;

	/// <inheritdoc cref="IMock.Setup" />
	IMockSetup IMock.Setup
		=> _inner.Setup;

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

	/// <inheritdoc cref="IMock.GetIndexer{TResult}(object?[])" />
	public TResult GetIndexer<TResult>(params object?[] parameters)
		=> _inner.GetIndexer<TResult>(parameters);

	/// <inheritdoc cref="IMock.SetIndexer{TResult}(TResult, object?[])" />
	public void SetIndexer<TResult>(TResult value, params object?[] parameters)
		=> _inner.SetIndexer(value, parameters);

	#endregion IMock
}
