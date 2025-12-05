using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Marker interface for setups.
/// </summary>
public interface ISetup;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Sets up the mock for <typeparamref name="T" />.
/// </summary>
public interface IMockSetup<out T> : IInteractiveMock<T>
{
	/// <summary>
	///     The underlying mock subject.
	/// </summary>
	T Subject { get; }
}

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" />.
/// </summary>
public interface IMockMethodSetup<out T> : IInteractiveMock<T>;

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.Equals(object)" />
///     method.
/// </summary>
public interface IMockMethodSetupWithEquals<out T> : IMockMethodSetup<T>
{
	/// <summary>
	///     Setup for the method <see cref="object.Equals(object?)" /> with the given <paramref name="obj" />.
	/// </summary>
	ReturnMethodSetup<bool, object?> Equals(IParameter<object?> obj);
}

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.Equals(object)" />
///     and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithEqualsWithGetHashCode<out T> : IMockMethodSetupWithEquals<T>,
	IMockMethodSetupWithGetHashCode<T>;

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.GetHashCode()" />
///     method.
/// </summary>
public interface IMockMethodSetupWithGetHashCode<out T> : IMockMethodSetup<T>
{
	/// <summary>
	///     Setup for the method <see cref="object.GetHashCode()" />.
	/// </summary>
	ReturnMethodSetup<int> GetHashCode();
}

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" />
///     method.
/// </summary>
public interface IMockMethodSetupWithToString<out T> : IMockMethodSetup<T>
{
	/// <summary>
	///     Setup for the method <see cref="object.ToString()" />.
	/// </summary>
	ReturnMethodSetup<string> ToString();
}

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and
///     <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithEquals<out T> : IMockMethodSetupWithToString<T>,
	IMockMethodSetupWithEquals<T>;

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" />,
///     <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithEqualsWithGetHashCode<out T> : IMockMethodSetupWithToString<T>,
	IMockMethodSetupWithEquals<T>, IMockMethodSetupWithGetHashCode<T>;

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and
///     <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithGetHashCode<out T> : IMockMethodSetupWithToString<T>,
	IMockMethodSetupWithGetHashCode<T>;

/// <summary>
///     Sets up properties on the mock for <typeparamref name="T" />.
/// </summary>
public interface IMockPropertySetup<out T> : IInteractiveMock<T>;

/// <summary>
///     Sets up protected methods on the mock for <typeparamref name="T" />.
/// </summary>
public interface IProtectedMockMethodSetup<out T> : IInteractiveMock<T>;

/// <summary>
///     Sets up protected properties on the mock for <typeparamref name="T" />.
/// </summary>
public interface IProtectedMockPropertySetup<out T> : IInteractiveMock<T>;

/// <summary>
///     Sets up the protected elements of the mock for <typeparamref name="T" />.
/// </summary>
public interface IProtectedMockSetup<out T> : IInteractiveMock<T>;
#pragma warning restore S2326 // Unused type parameters should be removed
