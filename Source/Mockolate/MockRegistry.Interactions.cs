using System;
using System.Diagnostics;
using System.Reflection;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     Gets the collection of interactions recorded by the mock object.
	/// </summary>
	public MockInteractions Interactions { get; }

	/// <summary>
	///     Clears all interactions recorded by the mock object.
	/// </summary>
	public void ClearAllInteractions()
		=> Interactions.Clear();

	/// <summary>
	///     Get the latest method setup matching the given <paramref name="methodName" /> and <paramref name="predicate" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	public T? GetMethodSetup<T>(string methodName, Func<T, bool> predicate) where T : MethodSetup
		=> Setup.Methods.GetMatching(methodName, predicate);

	/// <summary>
	///     Register an <paramref name="interaction" /> with the mock.
	/// </summary>
	public void RegisterInteraction(IInteraction interaction)
		=> ((IMockInteractions)Interactions).RegisterInteraction(interaction);

	/// <summary>
	///     Accesses the getter of the property with <paramref name="propertyName" />.
	/// </summary>
	public TResult GetProperty<TResult>(string propertyName, Func<TResult> defaultValueGenerator,
		Func<TResult>? baseValueAccessor)
	{
		IInteraction interaction =
			((IMockInteractions)Interactions).RegisterInteraction(new PropertyGetterAccess(propertyName));

		PropertySetup matchingSetup = ResolvePropertySetup(
			propertyName, defaultValueGenerator, baseValueAccessor,
			baseValueAccessor is not null);

		return ((IInteractivePropertySetup)matchingSetup).InvokeGetter(interaction, Behavior,
			defaultValueGenerator);
	}

	/// <summary>
	///     Accesses the setter of the property with <paramref name="propertyName" /> and the matching
	///     <paramref name="value" />.
	/// </summary>
	/// <remarks>
	///     Returns a flag, indicating whether the base class implementation should be skipped.
	/// </remarks>
	public bool SetProperty<T>(string propertyName, T value)
	{
		IInteraction interaction =
			((IMockInteractions)Interactions).RegisterInteraction(new PropertySetterAccess(propertyName,
				new NamedParameterValue<T>("value", value)));

		PropertySetup matchingSetup = ResolvePropertySetup<T>(propertyName, null, null, false);

		((IInteractivePropertySetup)matchingSetup).InvokeSetter(interaction, value, Behavior);
		return ((IInteractivePropertySetup)matchingSetup).SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	private PropertySetup ResolvePropertySetup<TResult>(
		string propertyName,
		Func<TResult>? defaultValueGenerator,
		Func<TResult>? baseValueAccessor,
		bool forceReinitWhenFound)
	{
		if (!Setup.Properties.TryGetValue(propertyName, out PropertySetup? existingSetup))
		{
			if (Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException(
					$"The property '{propertyName}' was accessed without prior setup.");
			}

			TResult initialValue = defaultValueGenerator is null
				? default!
				: Behavior.SkipBaseClass || baseValueAccessor is null
					? defaultValueGenerator()
					: baseValueAccessor.Invoke();
			PropertySetup setup = new PropertySetup.Default<TResult>(propertyName, initialValue);
			Setup.Properties.Add(setup);
			return setup;
		}

		if (forceReinitWhenFound || !existingSetup.IsValueInitialized)
		{
			object? initialValue;
			if (defaultValueGenerator is null)
			{
				initialValue = null;
			}
			else
			{
				bool skipBase = ((IInteractivePropertySetup)existingSetup).SkipBaseClass() ?? Behavior.SkipBaseClass;
				initialValue = skipBase || baseValueAccessor is null
					? defaultValueGenerator()
					: baseValueAccessor.Invoke();
			}

			((IInteractivePropertySetup)existingSetup).InitializeWith(initialValue);
		}

		return existingSetup;
	}

	/// <summary>
	///     Gets the value from the indexer with one typed parameter.
	///     Matching runs against the typed value directly; <see cref="Parameters.NamedParameterValue{T}" /> is only
	///     allocated for recording.
	/// </summary>
	public IndexerSetupResult<TResult> GetIndexer<TResult, T1>(string p1Name, T1 p1)
	{
		IndexerSetup? matchingSetup = GetIndexerSetupTyped(p1Name, p1);
		INamedParameterValue[] parameters = [new NamedParameterValue<T1>(p1Name, p1),];
		IndexerGetterAccess interaction = new(parameters);
		((IMockInteractions)Interactions).RegisterInteraction(interaction);
		return new IndexerSetupResult<TResult>(matchingSetup, interaction, Behavior, GetIndexerValue,
			Setup.Indexers.UpdateValue);
	}

	/// <summary>
	///     Gets the value from the indexer with two typed parameters.
	///     Matching runs against the typed values directly; <see cref="Parameters.NamedParameterValue{T}" /> is only
	///     allocated for recording.
	/// </summary>
	public IndexerSetupResult<TResult> GetIndexer<TResult, T1, T2>(string p1Name, T1 p1, string p2Name, T2 p2)
	{
		IndexerSetup? matchingSetup = GetIndexerSetupTyped(p1Name, p1, p2Name, p2);
		INamedParameterValue[] parameters =
		[
			new NamedParameterValue<T1>(p1Name, p1),
			new NamedParameterValue<T2>(p2Name, p2),
		];
		IndexerGetterAccess interaction = new(parameters);
		((IMockInteractions)Interactions).RegisterInteraction(interaction);
		return new IndexerSetupResult<TResult>(matchingSetup, interaction, Behavior, GetIndexerValue,
			Setup.Indexers.UpdateValue);
	}

	/// <summary>
	///     Gets the value from the indexer with three typed parameters.
	///     Matching runs against the typed values directly; <see cref="Parameters.NamedParameterValue{T}" /> is only
	///     allocated for recording.
	/// </summary>
	public IndexerSetupResult<TResult> GetIndexer<TResult, T1, T2, T3>(
		string p1Name, T1 p1, string p2Name, T2 p2, string p3Name, T3 p3)
	{
		IndexerSetup? matchingSetup = GetIndexerSetupTyped(p1Name, p1, p2Name, p2, p3Name, p3);
		INamedParameterValue[] parameters =
		[
			new NamedParameterValue<T1>(p1Name, p1),
			new NamedParameterValue<T2>(p2Name, p2),
			new NamedParameterValue<T3>(p3Name, p3),
		];
		IndexerGetterAccess interaction = new(parameters);
		((IMockInteractions)Interactions).RegisterInteraction(interaction);
		return new IndexerSetupResult<TResult>(matchingSetup, interaction, Behavior, GetIndexerValue,
			Setup.Indexers.UpdateValue);
	}

	/// <summary>
	///     Gets the value from the indexer with four typed parameters.
	///     Matching runs against the typed values directly; <see cref="Parameters.NamedParameterValue{T}" /> is only
	///     allocated for recording.
	/// </summary>
	public IndexerSetupResult<TResult> GetIndexer<TResult, T1, T2, T3, T4>(
		string p1Name, T1 p1, string p2Name, T2 p2, string p3Name, T3 p3, string p4Name, T4 p4)
	{
		IndexerSetup? matchingSetup = GetIndexerSetupTyped(p1Name, p1, p2Name, p2, p3Name, p3, p4Name, p4);
		INamedParameterValue[] parameters =
		[
			new NamedParameterValue<T1>(p1Name, p1),
			new NamedParameterValue<T2>(p2Name, p2),
			new NamedParameterValue<T3>(p3Name, p3),
			new NamedParameterValue<T4>(p4Name, p4),
		];
		IndexerGetterAccess interaction = new(parameters);
		((IMockInteractions)Interactions).RegisterInteraction(interaction);
		return new IndexerSetupResult<TResult>(matchingSetup, interaction, Behavior, GetIndexerValue,
			Setup.Indexers.UpdateValue);
	}

	/// <summary>
	///     Gets the value from the indexer with the given parameters.
	/// </summary>
	public IndexerSetupResult<TResult> GetIndexer<TResult>(params INamedParameterValue[] parameters)
	{
		IndexerGetterAccess interaction = new(parameters);
		((IMockInteractions)Interactions).RegisterInteraction(interaction);

		IndexerSetup? matchingSetup = Setup.Indexers.GetLatestOrDefault(interaction);
		return new IndexerSetupResult<TResult>(matchingSetup, interaction, Behavior, GetIndexerValue,
			Setup.Indexers.UpdateValue);
	}

	/// <summary>
	///     Sets the value of the indexer with one typed parameter.
	///     Matching runs against the typed value directly; <see cref="Parameters.NamedParameterValue{T}" /> is only
	///     allocated for recording.
	/// </summary>
	/// <remarks>
	///     Returns a flag, indicating whether the base class implementation should be skipped.
	/// </remarks>
	public bool SetIndexer<TResult, T1>(TResult value, string p1Name, T1 p1)
	{
		IndexerSetup? matchingSetup = GetIndexerSetupTyped(p1Name, p1);
		INamedParameterValue[] parameters = [new NamedParameterValue<T1>(p1Name, p1),];
		IndexerSetterAccess interaction = new(parameters, new NamedParameterValue<TResult>("value", value));
		((IMockInteractions)Interactions).RegisterInteraction(interaction);

		Setup.Indexers.UpdateValue(parameters, value);
		matchingSetup?.InvokeSetter(interaction, value, Behavior);
		return (matchingSetup as IInteractiveIndexerSetup)?.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Sets the value of the indexer with two typed parameters.
	///     Matching runs against the typed values directly; <see cref="Parameters.NamedParameterValue{T}" /> is only
	///     allocated for recording.
	/// </summary>
	/// <remarks>
	///     Returns a flag, indicating whether the base class implementation should be skipped.
	/// </remarks>
	public bool SetIndexer<TResult, T1, T2>(TResult value, string p1Name, T1 p1, string p2Name, T2 p2)
	{
		IndexerSetup? matchingSetup = GetIndexerSetupTyped(p1Name, p1, p2Name, p2);
		INamedParameterValue[] parameters =
		[
			new NamedParameterValue<T1>(p1Name, p1),
			new NamedParameterValue<T2>(p2Name, p2),
		];
		IndexerSetterAccess interaction = new(parameters, new NamedParameterValue<TResult>("value", value));
		((IMockInteractions)Interactions).RegisterInteraction(interaction);

		Setup.Indexers.UpdateValue(parameters, value);
		matchingSetup?.InvokeSetter(interaction, value, Behavior);
		return (matchingSetup as IInteractiveIndexerSetup)?.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Sets the value of the indexer with three typed parameters.
	///     Matching runs against the typed values directly; <see cref="Parameters.NamedParameterValue{T}" /> is only
	///     allocated for recording.
	/// </summary>
	/// <remarks>
	///     Returns a flag, indicating whether the base class implementation should be skipped.
	/// </remarks>
	public bool SetIndexer<TResult, T1, T2, T3>(TResult value, string p1Name, T1 p1, string p2Name, T2 p2,
		string p3Name, T3 p3)
	{
		IndexerSetup? matchingSetup = GetIndexerSetupTyped(p1Name, p1, p2Name, p2, p3Name, p3);
		INamedParameterValue[] parameters =
		[
			new NamedParameterValue<T1>(p1Name, p1),
			new NamedParameterValue<T2>(p2Name, p2),
			new NamedParameterValue<T3>(p3Name, p3),
		];
		IndexerSetterAccess interaction = new(parameters, new NamedParameterValue<TResult>("value", value));
		((IMockInteractions)Interactions).RegisterInteraction(interaction);

		Setup.Indexers.UpdateValue(parameters, value);
		matchingSetup?.InvokeSetter(interaction, value, Behavior);
		return (matchingSetup as IInteractiveIndexerSetup)?.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Sets the value of the indexer with four typed parameters.
	///     Matching runs against the typed values directly; <see cref="Parameters.NamedParameterValue{T}" /> is only
	///     allocated for recording.
	/// </summary>
	/// <remarks>
	///     Returns a flag, indicating whether the base class implementation should be skipped.
	/// </remarks>
	public bool SetIndexer<TResult, T1, T2, T3, T4>(TResult value, string p1Name, T1 p1, string p2Name, T2 p2,
		string p3Name, T3 p3, string p4Name, T4 p4)
	{
		IndexerSetup? matchingSetup = GetIndexerSetupTyped(p1Name, p1, p2Name, p2, p3Name, p3, p4Name, p4);
		INamedParameterValue[] parameters =
		[
			new NamedParameterValue<T1>(p1Name, p1),
			new NamedParameterValue<T2>(p2Name, p2),
			new NamedParameterValue<T3>(p3Name, p3),
			new NamedParameterValue<T4>(p4Name, p4),
		];
		IndexerSetterAccess interaction = new(parameters, new NamedParameterValue<TResult>("value", value));
		((IMockInteractions)Interactions).RegisterInteraction(interaction);

		Setup.Indexers.UpdateValue(parameters, value);
		matchingSetup?.InvokeSetter(interaction, value, Behavior);
		return (matchingSetup as IInteractiveIndexerSetup)?.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Sets the value of the indexer with the given parameters.
	/// </summary>
	/// <remarks>
	///     Returns a flag, indicating whether the base class implementation should be skipped.
	/// </remarks>
	public bool SetIndexer<TResult>(TResult value, params INamedParameterValue[] parameters)
	{
		IndexerSetterAccess interaction = new(parameters, new NamedParameterValue<TResult>("value", value));
		((IMockInteractions)Interactions).RegisterInteraction(interaction);

		Setup.Indexers.UpdateValue(parameters, value);
		IndexerSetup? matchingSetup = Setup.Indexers.GetLatestOrDefault(interaction);
		matchingSetup?.InvokeSetter(interaction, value, Behavior);
		return (matchingSetup as IInteractiveIndexerSetup)?.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Associates the specified event <paramref name="method" /> on the <paramref name="target" /> with the event
	///     identified by the given <paramref name="name" />.
	/// </summary>
	public void AddEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event subscription may not be null.");
		}

		((IMockInteractions)Interactions).RegisterInteraction(new EventSubscription(name, target, method));
		foreach (EventSetup setup in Setup.Events.GetByName(name))
		{
			setup.InvokeSubscribed(target, method);
		}
	}

	/// <summary>
	///     Removes the specified event <paramref name="method" /> on the <paramref name="target" /> from the event identified
	///     by the given <paramref name="name" />.
	/// </summary>
	public void RemoveEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event unsubscription may not be null.");
		}

		((IMockInteractions)Interactions).RegisterInteraction(new EventUnsubscription(name, target, method));
		foreach (EventSetup setup in Setup.Events.GetByName(name))
		{
			setup.InvokeUnsubscribed(target, method);
		}
	}

	/// <summary>
	///     Retrieves the latest indexer setup that matches the single typed parameter,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private IndexerSetup? GetIndexerSetupTyped<T1>(string n1, T1 v1)
	{
		IndexerGetterAccess? fallback = null;
		return Setup.GetScenario(Scenario).Indexers.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(IndexerSetup setup)
		{
			if (setup is ITypedIndexerMatch typed)
			{
				return typed.MatchesTyped(n1, v1);
			}

			fallback ??= new IndexerGetterAccess([new NamedParameterValue<T1>(n1, v1),]);
			return ((IInteractiveIndexerSetup)setup).Matches(fallback);
		}
	}

	/// <summary>
	///     Retrieves the latest indexer setup that matches the two typed parameters,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private IndexerSetup? GetIndexerSetupTyped<T1, T2>(string n1, T1 v1, string n2, T2 v2)
	{
		IndexerGetterAccess? fallback = null;
		return Setup.GetScenario(Scenario).Indexers.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(IndexerSetup setup)
		{
			if (setup is ITypedIndexerMatch typed)
			{
				return typed.MatchesTyped(n1, v1, n2, v2);
			}

			fallback ??= new IndexerGetterAccess([
				new NamedParameterValue<T1>(n1, v1),
				new NamedParameterValue<T2>(n2, v2),
			]);
			return ((IInteractiveIndexerSetup)setup).Matches(fallback);
		}
	}

	/// <summary>
	///     Retrieves the latest indexer setup that matches the three typed parameters,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private IndexerSetup? GetIndexerSetupTyped<T1, T2, T3>(string n1, T1 v1, string n2, T2 v2, string n3, T3 v3)
	{
		IndexerGetterAccess? fallback = null;
		return Setup.GetScenario(Scenario).Indexers.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(IndexerSetup setup)
		{
			if (setup is ITypedIndexerMatch typed)
			{
				return typed.MatchesTyped(n1, v1, n2, v2, n3, v3);
			}

			fallback ??= new IndexerGetterAccess([
				new NamedParameterValue<T1>(n1, v1),
				new NamedParameterValue<T2>(n2, v2),
				new NamedParameterValue<T3>(n3, v3),
			]);
			return ((IInteractiveIndexerSetup)setup).Matches(fallback);
		}
	}

	/// <summary>
	///     Retrieves the latest indexer setup that matches the four typed parameters,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private IndexerSetup? GetIndexerSetupTyped<T1, T2, T3, T4>(
		string n1, T1 v1, string n2, T2 v2, string n3, T3 v3, string n4, T4 v4)
	{
		IndexerGetterAccess? fallback = null;
		return Setup.GetScenario(Scenario).Indexers.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(IndexerSetup setup)
		{
			if (setup is ITypedIndexerMatch typed)
			{
				return typed.MatchesTyped(n1, v1, n2, v2, n3, v3, n4, v4);
			}

			fallback ??= new IndexerGetterAccess([
				new NamedParameterValue<T1>(n1, v1),
				new NamedParameterValue<T2>(n2, v2),
				new NamedParameterValue<T3>(n3, v3),
				new NamedParameterValue<T4>(n4, v4),
			]);
			return ((IInteractiveIndexerSetup)setup).Matches(fallback);
		}
	}

	/// <summary>
	///     Gets the indexer value for the given <paramref name="parameters" />.
	/// </summary>
	private TValue GetIndexerValue<TValue>(IInteractiveIndexerSetup? setup, Func<TValue> defaultValueGenerator,
		INamedParameterValue[] parameters)
		=> Setup.GetScenario(Scenario).Indexers.GetOrAddValue(parameters, defaultValueGenerator);
}
