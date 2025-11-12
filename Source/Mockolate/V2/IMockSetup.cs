using System.Collections.Generic;
using System.Reflection;
using Mockolate.Setup;

namespace Mockolate.V2;

/// <summary>
///     Allows registration of <see cref="MethodSetup" /> in the mock.
/// </summary>
public interface IMockSetup
{
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

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Sets up the mock for <typeparamref name="T" />.
/// </summary>
public interface IMockSetup<T>
{
}

/// <summary>
///     Provides methods for managing events on a mock object, including raising events and associating or dissociating
///     event handlers.
/// </summary>
public interface IMockRaises<T>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
