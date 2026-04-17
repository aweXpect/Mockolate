using System;
using System.Collections.Generic;
using System.Reflection;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     Gets the collection of interactions recorded by the mock object.
	/// </summary>
	public MockInteractions Interactions { get; }

	private ValueStorage IndexerStorage
		=> Setup.Indexers.ValueStorage;

	/// <summary>
	///     Clears all interactions recorded by the mock object.
	/// </summary>
	public void ClearAllInteractions()
		=> Interactions.Clear();

	/// <summary>
	///     Get the latest method setup matching the given <paramref name="methodName" /> and <paramref name="predicate" />,
	///     or returns <see langword="null" /> if no matching setup is found. Scenario setups take precedence over
	///     default-scope setups.
	/// </summary>
	public T? GetMethodSetup<T>(string methodName, Func<T, bool> predicate) where T : MethodSetup
	{
		if (!string.IsNullOrEmpty(Scenario))
		{
			T? scoped = Setup.GetScenario(Scenario).Methods.GetMatching(methodName, predicate);
			if (scoped is not null)
			{
				return scoped;
			}
		}

		return Setup.Methods.GetMatching(methodName, predicate);
	}

	/// <summary>
	///     Get the latest indexer setup matching the given <paramref name="predicate" />,
	///     or returns <see langword="null" /> if no matching setup is found. Scenario setups take precedence over
	///     default-scope setups.
	/// </summary>
	public T? GetIndexerSetup<T>(Func<T, bool> predicate) where T : IndexerSetup
	{
		if (!string.IsNullOrEmpty(Scenario))
		{
			T? scoped = Setup.GetScenario(Scenario).Indexers.GetMatching(predicate);
			if (scoped is not null)
			{
				return scoped;
			}
		}

		return Setup.Indexers.GetMatching(predicate);
	}

	/// <summary>
	///     Stores the given <paramref name="value" /> for the given indexer <paramref name="access" />.
	/// </summary>
	public void SetIndexerValue<TResult>(IndexerAccess access, TResult value)
	{
		access.Storage = IndexerStorage;
		access.StoreValue(value);
	}

	/// <summary>
	///     Applies an indexer getter using a pre-computed <paramref name="baseValue" /> and an optional matching
	///     <paramref name="setup" />. Returns the final getter result.
	/// </summary>
	/// <remarks>
	///     When <paramref name="setup" /> is <see langword="null" />, returns any previously stored value or the
	///     <paramref name="baseValue" /> (which is then stored).
	/// </remarks>
	public TResult ApplyIndexerGetter<TResult>(IndexerAccess access, IndexerSetup? setup, TResult baseValue)
	{
		access.Storage = IndexerStorage;
		if (setup is null)
		{
			if (access.TryFindStoredValue(out TResult stored))
			{
				return stored;
			}

			access.StoreValue(baseValue);
			return baseValue;
		}

		return setup.GetResult(access, Behavior, baseValue);
	}

	/// <summary>
	///     Applies an indexer getter using a lazy <paramref name="defaultValueGenerator" /> and an optional matching
	///     <paramref name="setup" />. Returns the final getter result.
	/// </summary>
	/// <remarks>
	///     When <paramref name="setup" /> is <see langword="null" />, returns any previously stored value, or (if
	///     <see cref="MockBehavior.ThrowWhenNotSetup" /> is <see langword="true" />) throws a
	///     <see cref="MockNotSetupException" />, otherwise stores and returns the <paramref name="defaultValueGenerator" />'s
	///     value.
	/// </remarks>
	public TResult ApplyIndexerGetter<TResult>(IndexerAccess access, IndexerSetup? setup,
		Func<TResult> defaultValueGenerator)
	{
		access.Storage = IndexerStorage;
		if (setup is null)
		{
			if (access.TryFindStoredValue(out TResult stored))
			{
				return stored;
			}

			if (Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException($"{access} was accessed without prior setup.");
			}

			TResult value = defaultValueGenerator();
			access.StoreValue(value);
			return value;
		}

		return setup.GetResult(access, Behavior, defaultValueGenerator);
	}

	/// <summary>
	///     Applies an indexer setter for the given <paramref name="access" /> with the given <paramref name="value" /> and
	///     optional matching <paramref name="setup" />. Returns whether the base class implementation should be skipped.
	/// </summary>
	public bool ApplyIndexerSetter<TResult>(IndexerAccess access, IndexerSetup? setup, TResult value)
	{
		if (setup is null)
		{
			SetIndexerValue(access, value);
			return Behavior.SkipBaseClass;
		}

		access.Storage = IndexerStorage;
		setup.SetResult(access, Behavior, value);
		return setup.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Register an <paramref name="interaction" /> with the mock.
	/// </summary>
	public void RegisterInteraction(IInteraction interaction)
		=> ((IMockInteractions)Interactions).RegisterInteraction(interaction);

	private IEnumerable<EventSetup> GetEventSetupsByName(string name)
	{
		if (!string.IsNullOrEmpty(Scenario))
		{
			foreach (EventSetup setup in Setup.GetScenario(Scenario).Events.GetByName(name))
			{
				yield return setup;
			}
		}

		foreach (EventSetup setup in Setup.Events.GetByName(name))
		{
			yield return setup;
		}
	}

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
			((IMockInteractions)Interactions).RegisterInteraction(new PropertySetterAccess<T>(propertyName, value));

		PropertySetup matchingSetup = ResolvePropertySetup<T>(propertyName, null, null, false);

		((IInteractivePropertySetup)matchingSetup).InvokeSetter(interaction, value, Behavior);
		return ((IInteractivePropertySetup)matchingSetup).SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
	private PropertySetup ResolvePropertySetup<TResult>(
		string propertyName,
		Func<TResult>? defaultValueGenerator,
		Func<TResult>? baseValueAccessor,
		bool forceReinitWhenFound)
	{
		PropertySetup? existingSetup = null;
		if (!string.IsNullOrEmpty(Scenario) &&
		    Setup.GetScenario(Scenario).Properties.TryGetValue(propertyName, out PropertySetup? scopedSetup))
		{
			existingSetup = scopedSetup;
		}

		if (existingSetup is null && !Setup.Properties.TryGetValue(propertyName, out existingSetup))
		{
			if (Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException(
					$"The property '{propertyName}' was accessed without prior setup.");
			}

			TResult initialValue = GetInitialValue();
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

		TResult GetInitialValue()
		{
			if (defaultValueGenerator is null)
			{
				return default!;
			}

			return Behavior.SkipBaseClass || baseValueAccessor is null
				? defaultValueGenerator()
				: baseValueAccessor.Invoke();
		}
	}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high

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
		foreach (EventSetup setup in GetEventSetupsByName(name))
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
		foreach (EventSetup setup in GetEventSetupsByName(name))
		{
			setup.InvokeUnsubscribed(target, method);
		}
	}
}
