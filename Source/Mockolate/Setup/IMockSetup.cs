using System.Collections.Generic;
using System.Reflection;

namespace Mockolate.Setup;

/// <summary>
///     Sets up the mock for <typeparamref name="T" />.
/// </summary>
public interface IMockSetup<T>
{

}
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" />.
/// </summary>
public interface IMockMethodSetup<T>
{
}
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> method.
/// </summary>
public interface IMockMethodSetupWithToString<T> : IMockMethodSetup<T>
{
	/// <summary>
	///     Setup for the method <see cref="object.ToString()"/>.
	/// </summary>
	ReturnMethodSetup<string> ToString();
}
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockMethodSetupWithEquals<T> : IMockMethodSetup<T>
{
	/// <summary>
	///     Setup for the method <see cref="object.Equals(object?)"/> with the given <paramref name="obj"/>.
	/// </summary>
	ReturnMethodSetup<bool, object?> Equals(With.Parameter<object?> obj);
}
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithGetHashCode<T> : IMockMethodSetup<T>
{
	/// <summary>
	///     Setup for the method <see cref="object.GetHashCode()"/>.
	/// </summary>
	ReturnMethodSetup<int> GetHashCode();
}
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" />, <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithEqualsWithGetHashCode<T> : IMockMethodSetupWithToString<T>, IMockMethodSetupWithEquals<T>, IMockMethodSetupWithGetHashCode<T>
{
}
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithEqualsWithGetHashCode<T> : IMockMethodSetupWithEquals<T>, IMockMethodSetupWithGetHashCode<T>
{
}
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithGetHashCode<T> : IMockMethodSetupWithToString<T>, IMockMethodSetupWithGetHashCode<T>
{
}
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithEquals<T> : IMockMethodSetupWithToString<T>, IMockMethodSetupWithEquals<T>
{
}
/// <summary>
///     Sets up properties on the mock for <typeparamref name="T" />.
/// </summary>
public interface IMockPropertySetup<T>
{

}
/// <summary>
///     Sets up the protected elements of the mock for <typeparamref name="T" />.
/// </summary>
public interface IProtectedMockSetup<T>
{

}
/// <summary>
///     Sets up protected methods on the mock for <typeparamref name="T" />.
/// </summary>
public interface IProtectedMockMethodSetup<T>
{

}
/// <summary>
///     Sets up protected properties on the mock for <typeparamref name="T" />.
/// </summary>
public interface IProtectedMockPropertySetup<T>
{

}

/// <summary>
///     Allows registration of <see cref="MethodSetup" /> in the mock.
/// </summary>
public interface IMockSetup
{
	/// <summary>
	///     Gets the underlying <see cref="IMock" /> instance.
	/// </summary>
	IMock Mock { get; }

	/// <summary>
	///     Registers the <paramref name="indexerSetup" /> in the mock.
	/// </summary>
	void RegisterIndexer(IndexerSetup indexerSetup);

	/// <summary>
	///     Sets the indexer for the given <paramref name="parameters" /> to the given <paramref name="value" />.
	/// </summary>
	void SetIndexerValue<TValue>(object?[] parameters, TValue value);

	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock.
	/// </summary>
	void RegisterMethod(MethodSetup methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock.
	/// </summary>
	void RegisterProperty(string propertyName, PropertySetup propertySetup);

	/// <summary>
	///     Gets all event handlers registered for the specified <paramref name="eventName" />.
	/// </summary>
	/// <param name="eventName"></param>
	/// <returns></returns>
	IEnumerable<(object?, MethodInfo)> GetEventHandlers(string eventName);

	/// <summary>
	///     Registers an event handler <paramref name="method" /> on <paramref name="target" /> for the specified
	///     <paramref name="eventName" />.
	/// </summary>
	void AddEvent(string eventName, object? target, MethodInfo method);

	/// <summary>
	///     Removes a previously registered event handler <paramref name="method" /> on <paramref name="target" /> for the
	///     specified <paramref name="eventName" />.
	/// </summary>
	void RemoveEvent(string eventName, object? target, MethodInfo method);
}
