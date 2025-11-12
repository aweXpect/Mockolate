using System;
using System.Reflection;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.V2;

public interface IMockRegistration {

	/// <summary>
	///     Gets the prefix string used to identify or categorize items within the context.
	/// </summary>
	string Prefix { get; }

	/// <summary>
	///     Gets the behavior settings used by this mock instance.
	/// </summary>
	MockBehavior Behavior { get; }

	/// <summary>
	///     Gets the collection of interactions recorded by the mock object.
	/// </summary>
	MockInteractions Interactions { get; }

	/// <summary>
	///     Executes the method with <paramref name="methodName" /> and the matching <paramref name="parameters" /> and gets
	///     the setup return value.
	/// </summary>
	MethodSetupResult<TResult> Execute<TResult>(string methodName, params object?[]? parameters);

	/// <summary>
	///     Executes the method with <paramref name="methodName" /> and the matching <paramref name="parameters" /> returning
	///     <see langword="void" />.
	/// </summary>
	MethodSetupResult Execute(string methodName, params object?[]? parameters);

	/// <summary>
	///     Accesses the getter of the property with <paramref name="propertyName" />.
	/// </summary>
	TResult Get<TResult>(string propertyName, Func<TResult>? defaultValueGenerator = null);

	/// <summary>
	///     Accesses the setter of the property with <paramref name="propertyName" /> and the matching
	///     <paramref name="value" />.
	/// </summary>
	void Set(string propertyName, object? value);

	/// <summary>
	///     Gets the value from the indexer with the given parameters.
	/// </summary>
	TResult GetIndexer<TResult>(Func<TResult>? defaultValueGenerator, params object?[] parameters);

	/// <summary>
	///     Sets the value of the indexer with the given parameters.
	/// </summary>
	void SetIndexer<TResult>(TResult value, params object?[] parameters);
	
	
	/// <summary>
	///     Raises the event with <paramref name="eventName" /> and the given <paramref name="parameters" />.
	/// </summary>
	void Raise(string eventName, params object?[] parameters);

	/// <summary>
	///     Associates the specified event <paramref name="method" /> on the <paramref name="target" /> with the event
	///     identified by the given <paramref name="name" />.
	/// </summary>
	void AddEvent(string name, object? target, MethodInfo? method);

	/// <summary>
	///     Removes the specified event <paramref name="method" /> on the <paramref name="target" /> from the event identified
	///     by the given <paramref name="name" />.
	/// </summary>
	void RemoveEvent(string name, object? target, MethodInfo? method);
	
	
	
	
	
	/// <summary>
	///     Registers the <paramref name="indexerSetup" /> in the mock.
	/// </summary>
	void SetupIndexer(IndexerSetup indexerSetup);

	/// <summary>
	///     Sets the indexer for the given <paramref name="parameters" /> to the given <paramref name="value" />.
	/// </summary>
	void SetupIndexerValue<TValue>(object?[] parameters, TValue value);

	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock.
	/// </summary>
	void SetupMethod(MethodSetup methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock.
	/// </summary>
	void SetupProperty(string propertyName, PropertySetup propertySetup);
}
