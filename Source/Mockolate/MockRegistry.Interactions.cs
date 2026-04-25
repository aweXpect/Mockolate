using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     Gets the collection of interactions recorded by the mock object.
	/// </summary>
	public IMockInteractions Interactions { get; }

	/// <summary>
	///     Clears all interactions recorded by the mock object.
	/// </summary>
	public void ClearAllInteractions()
		=> Interactions.Clear();

	/// <summary>
	///     Optional pre-sizing hook for the mock's internal indexer value-storage array. When the total number
	///     of distinct indexer signatures is known up front, calling this once avoids the lazy-grow allocation
	///     on first access. Safe to skip — storage grows on demand otherwise.
	/// </summary>
	/// <param name="indexerCount">The number of distinct indexer signatures. Must be non-negative.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="indexerCount" /> is negative.</exception>
	public void InitializeStorage(int indexerCount)
	{
		if (indexerCount < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(indexerCount), indexerCount,
				"Indexer count must be non-negative.");
		}

		Setup.Indexers.InitializeStorageCount(indexerCount);
	}

	/// <summary>
	///     Returns the current snapshot of default-scope method setups registered under the generator-emitted
	///     <paramref name="memberId" />, or <see langword="null" /> when no setup has been registered.
	/// </summary>
	/// <remarks>
	///     The returned array is immutable — callers may iterate it without a lock. Setups are appended in
	///     registration order, so callers interested in latest-registered-first should walk the array in reverse.
	///     This accessor only sees setups registered via the <c>SetupMethod(int, ...)</c> overloads; scenario-scoped
	///     and legacy string-keyed registrations are retrieved via <see cref="GetMethodSetups{T}(string)" />.
	/// </remarks>
	/// <param name="memberId">The generator-emitted member id.</param>
	/// <returns>The setups array for the member, or <see langword="null" /> when none are registered.</returns>
	public MethodSetup[]? GetMethodSetupSnapshot(int memberId)
	{
		MethodSetup[]?[]? table = Volatile.Read(ref _setupsByMemberId);
		if (table is null || (uint)memberId >= (uint)table.Length)
		{
			return null;
		}

		return table[memberId];
	}

	/// <summary>
	///     Enumerates method setups of type <typeparamref name="T" /> matching <paramref name="methodName" />
	///     in latest-registered-first order, scenario-scoped setups before default-scope setups.
	/// </summary>
	/// <remarks>
	///     The caller iterates and evaluates each setup's matcher on the stack (passing ref-struct
	///     values where applicable), so no predicate closure is captured.
	///     Scenario-scoped results come first; the caller is expected to stop on the first match so
	///     scenarios override the default scope.
	/// </remarks>
	/// <typeparam name="T">The concrete <see cref="MethodSetup" /> subtype to return.</typeparam>
	/// <param name="methodName">The simple method name.</param>
	/// <returns>A lazy stream of matching setups, scenario-scoped first.</returns>
	public IEnumerable<T> GetMethodSetups<T>(string methodName) where T : MethodSetup
	{
		if (!string.IsNullOrEmpty(Scenario) &&
		    Setup.TryGetScenario(Scenario, out MockScenarioSetup? scopedBucket))
		{
			foreach (T setup in scopedBucket.Methods.EnumerateByName<T>(methodName))
			{
				yield return setup;
			}
		}

		foreach (T setup in Setup.Methods.EnumerateByName<T>(methodName))
		{
			yield return setup;
		}
	}

	/// <summary>
	///     Returns the latest registered indexer setup of type <typeparamref name="T" /> that satisfies
	///     <paramref name="predicate" />, or <see langword="null" /> when no setup matches. Scenario-scoped setups
	///     take precedence over default-scope setups.
	/// </summary>
	/// <typeparam name="T">The concrete <see cref="IndexerSetup" /> subtype to return.</typeparam>
	/// <param name="predicate">Argument matcher applied to each candidate setup.</param>
	/// <returns>The matching setup, or <see langword="null" /> when none was found.</returns>
	public T? GetIndexerSetup<T>(Func<T, bool> predicate) where T : IndexerSetup
	{
		// Stryker disable once String : TryGetScenario returns the root bucket when given a null or empty name, so replacing the IsNullOrEmpty guard with a null/empty comparison just re-searches the same global Indexers and produces the same result.
		if (!string.IsNullOrEmpty(Scenario) &&
		    Setup.TryGetScenario(Scenario, out MockScenarioSetup? scopedBucket))
		{
			T? scoped = scopedBucket.Indexers.GetMatching(predicate);
			if (scoped is not null)
			{
				return scoped;
			}
		}

		return Setup.Indexers.GetMatching(predicate);
	}

	/// <summary>
	///     Returns the latest registered indexer setup of type <typeparamref name="T" /> that matches the
	///     <paramref name="access" />, or <see langword="null" /> when no setup matches. Scenario-scoped setups take
	///     precedence over default-scope setups.
	/// </summary>
	/// <typeparam name="T">The concrete <see cref="IndexerSetup" /> subtype to return.</typeparam>
	/// <param name="access">The indexer access whose argument values must be matched.</param>
	/// <returns>The matching setup, or <see langword="null" /> when none was found.</returns>
	public T? GetIndexerSetup<T>(IndexerAccess access) where T : IndexerSetup
	{
		if (!string.IsNullOrEmpty(Scenario) &&
		    Setup.TryGetScenario(Scenario, out MockScenarioSetup? scopedBucket))
		{
			T? scoped = scopedBucket.Indexers.GetMatching<T>(access);
			if (scoped is not null)
			{
				return scoped;
			}
		}

		return Setup.Indexers.GetMatching<T>(access);
	}

	/// <summary>
	///     Stores <paramref name="value" /> in the indexer value slot identified by <paramref name="access" />.
	/// </summary>
	/// <typeparam name="TResult">The indexer's value type.</typeparam>
	/// <param name="access">The indexer access whose arguments identify the slot.</param>
	/// <param name="value">The value to store.</param>
	/// <param name="signatureIndex">Index into the mock's indexer-signature table (emitted by the source generator).</param>
	public void SetIndexerValue<TResult>(IndexerAccess access, TResult value, int signatureIndex)
	{
		access.AttachStorage(Setup.Indexers.GetOrCreateStorage<TResult>(signatureIndex));
		access.StoreValue(value);
	}

	/// <summary>
	///     Handles the no-matching-setup fallback for an indexer getter: returns any previously stored value, throws
	///     when <see cref="MockBehavior.ThrowWhenNotSetup" /> is <see langword="true" />, or otherwise stores and
	///     returns the <see cref="MockBehavior.DefaultValue" />.
	/// </summary>
	/// <typeparam name="TResult">The indexer's value type.</typeparam>
	/// <param name="access">The indexer access whose arguments identify the slot.</param>
	/// <param name="signatureIndex">Index into the mock's indexer-signature table (emitted by the source generator).</param>
	/// <returns>The stored value, or the freshly generated default.</returns>
	/// <exception cref="MockNotSetupException"><see cref="MockBehavior.ThrowWhenNotSetup" /> is <see langword="true" /> and no value was previously stored for this access.</exception>
	public TResult GetIndexerFallback<TResult>(IndexerAccess access, int signatureIndex)
	{
		access.AttachStorage(Setup.Indexers.GetOrCreateStorage<TResult>(signatureIndex));
		if (access.TryFindStoredValue(out TResult stored))
		{
			return stored;
		}

		if (Behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException($"{access} was accessed without prior setup.");
		}

		TResult value = Behavior.DefaultValue.GenerateTyped<TResult>();
		access.StoreValue(value);
		return value;
	}

	/// <summary>
	///     Invokes the getter flow of the given <paramref name="setup" /> for the given <paramref name="access" />,
	///     ensuring the indexer value storage is wired up before dispatching.
	/// </summary>
	/// <typeparam name="TResult">The indexer's value type.</typeparam>
	/// <param name="access">The indexer access whose arguments identify the slot.</param>
	/// <param name="setup">The previously matched indexer setup.</param>
	/// <param name="signatureIndex">Index into the mock's indexer-signature table (emitted by the source generator).</param>
	/// <returns>The value produced by the setup.</returns>
	public TResult ApplyIndexerSetup<TResult>(IndexerAccess access, IndexerSetup setup, int signatureIndex)
	{
		access.AttachStorage(Setup.Indexers.GetOrCreateStorage<TResult>(signatureIndex));
		return setup.GetResult<TResult>(access, Behavior);
	}

	/// <summary>
	///     Applies an indexer getter using a pre-computed <paramref name="baseValue" /> and an optional matching
	///     <paramref name="setup" />. Returns the final getter result.
	/// </summary>
	/// <remarks>
	///     When <paramref name="setup" /> is <see langword="null" />, returns any previously stored value or the
	///     <paramref name="baseValue" /> (which is then stored).
	/// </remarks>
	/// <typeparam name="TResult">The indexer's value type.</typeparam>
	/// <param name="access">The indexer access whose arguments identify the slot.</param>
	/// <param name="setup">The matching indexer setup, or <see langword="null" /> to fall through to stored/base value.</param>
	/// <param name="baseValue">Value returned by the base-class indexer (or a caller-supplied default).</param>
	/// <param name="signatureIndex">Index into the mock's indexer-signature table (emitted by the source generator).</param>
	/// <returns>The final getter result.</returns>
	public TResult ApplyIndexerGetter<TResult>(IndexerAccess access, IndexerSetup? setup, TResult baseValue,
		int signatureIndex)
	{
		access.AttachStorage(Setup.Indexers.GetOrCreateStorage<TResult>(signatureIndex));
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
	/// <typeparam name="TResult">The indexer's value type.</typeparam>
	/// <param name="access">The indexer access whose arguments identify the slot.</param>
	/// <param name="setup">The matching indexer setup, or <see langword="null" /> to fall through to stored/default value.</param>
	/// <param name="defaultValueGenerator">Lazy producer of the default value &#8212; only invoked when a default is actually needed.</param>
	/// <param name="signatureIndex">Index into the mock's indexer-signature table (emitted by the source generator).</param>
	/// <returns>The final getter result.</returns>
	/// <exception cref="MockNotSetupException"><paramref name="setup" /> is <see langword="null" />, <see cref="MockBehavior.ThrowWhenNotSetup" /> is <see langword="true" />, and no value was previously stored.</exception>
	public TResult ApplyIndexerGetter<TResult>(IndexerAccess access, IndexerSetup? setup,
		Func<TResult> defaultValueGenerator, int signatureIndex)
	{
		access.AttachStorage(Setup.Indexers.GetOrCreateStorage<TResult>(signatureIndex));
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
	///     Applies an indexer setter for the given <paramref name="access" /> with the given <paramref name="value" />
	///     and optional matching <paramref name="setup" />.
	/// </summary>
	/// <typeparam name="TResult">The indexer's value type.</typeparam>
	/// <param name="access">The indexer access whose arguments identify the slot.</param>
	/// <param name="setup">The matching indexer setup, or <see langword="null" /> to simply store the value.</param>
	/// <param name="value">The value being assigned.</param>
	/// <param name="signatureIndex">Index into the mock's indexer-signature table (emitted by the source generator).</param>
	/// <returns><see langword="true" /> when the base-class setter should be skipped.</returns>
	public bool ApplyIndexerSetter<TResult>(IndexerAccess access, IndexerSetup? setup, TResult value,
		int signatureIndex)
	{
		access.AttachStorage(Setup.Indexers.GetOrCreateStorage<TResult>(signatureIndex));
		if (setup is null)
		{
			access.StoreValue(value);
			return Behavior.SkipBaseClass;
		}

		setup.SetResult(access, Behavior, value);
		return setup.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Appends <paramref name="interaction" /> to the mock's recorded interactions.
	/// </summary>
	/// <remarks>
	///     Has no effect when <see cref="MockBehavior.SkipInteractionRecording" /> is <see langword="true" />.
	/// </remarks>
	/// <param name="interaction">The recorded interaction to append.</param>
	public void RegisterInteraction(IInteraction interaction)
	{
		if (!Behavior.SkipInteractionRecording)
		{
			Interactions.RegisterInteraction(interaction);
		}
	}

	private IEnumerable<EventSetup> GetEventSetupsByName(string name)
	{
		if (!string.IsNullOrEmpty(Scenario) &&
		    Setup.TryGetScenario(Scenario, out MockScenarioSetup? scopedBucket))
		{
			bool hasScoped = false;
			foreach (EventSetup setup in scopedBucket.Events.GetByName(name))
			{
				hasScoped = true;
				yield return setup;
			}

			if (hasScoped)
			{
				yield break;
			}
		}

		foreach (EventSetup setup in Setup.Events.GetByName(name))
		{
			yield return setup;
		}
	}

	/// <summary>
	///     Executes the getter flow for the property named <paramref name="propertyName" />, honoring configured
	///     setups and the active <see cref="MockBehavior" />.
	/// </summary>
	/// <typeparam name="TResult">The property's value type.</typeparam>
	/// <param name="propertyName">The simple property name.</param>
	/// <param name="defaultValueGenerator">Producer of the default value when no setup supplies one.</param>
	/// <param name="baseValueAccessor">Optional accessor for the base-class getter; when <see langword="null" /> only the default/initial value is considered.</param>
	/// <returns>The resolved getter value.</returns>
	/// <exception cref="MockNotSetupException">No setup exists for the property and <see cref="MockBehavior.ThrowWhenNotSetup" /> is <see langword="true" />.</exception>
	public TResult GetProperty<TResult>(string propertyName, Func<TResult> defaultValueGenerator,
		Func<TResult>? baseValueAccessor)
	{
		IInteraction? interaction = null;
		if (!Behavior.SkipInteractionRecording)
		{
			interaction = Interactions.RegisterInteraction(
				new PropertyGetterAccess(propertyName));
		}

		return ResolveGetterInternal(propertyName, defaultValueGenerator, baseValueAccessor, interaction);
	}

	/// <summary>
	///     Member-id-keyed overload of <see cref="GetProperty{TResult}(string, Func{TResult}, Func{TResult}?)" /> that
	///     records via the typed <see cref="FastPropertyGetterBuffer" /> when the mock is wired to a
	///     <see cref="FastMockInteractions" />, falling back to the legacy list otherwise.
	/// </summary>
	/// <typeparam name="TResult">The property's value type.</typeparam>
	/// <param name="memberId">The generator-emitted member id for the property getter.</param>
	/// <param name="propertyName">The simple property name.</param>
	/// <param name="defaultValueGenerator">Producer of the default value when no setup supplies one.</param>
	/// <param name="baseValueAccessor">Optional accessor for the base-class getter; when <see langword="null" /> only the default/initial value is considered.</param>
	/// <returns>The resolved getter value.</returns>
	/// <exception cref="MockNotSetupException">No setup exists for the property and <see cref="MockBehavior.ThrowWhenNotSetup" /> is <see langword="true" />.</exception>
	public TResult GetProperty<TResult>(int memberId, string propertyName, Func<TResult> defaultValueGenerator,
		Func<TResult>? baseValueAccessor)
	{
		if (!Behavior.SkipInteractionRecording)
		{
			RecordPropertyGetter(memberId, propertyName);
		}

		return ResolveGetterInternal(propertyName, defaultValueGenerator, baseValueAccessor, null);
	}

	private void RecordPropertyGetter(int memberId, string propertyName)
	{
		if (Interactions is FastMockInteractions __fast)
		{
			IFastMemberBuffer?[] __buffers = __fast.Buffers;
			if ((uint)memberId < (uint)__buffers.Length &&
			    __buffers[memberId] is FastPropertyGetterBuffer __buffer)
			{
				__buffer.Append(propertyName);
				return;
			}
		}

		Interactions.RegisterInteraction(new PropertyGetterAccess(propertyName));
	}

	private TResult ResolveGetterInternal<TResult>(string propertyName, Func<TResult> defaultValueGenerator,
		Func<TResult>? baseValueAccessor, IInteraction? interaction)
	{
		PropertySetup matchingSetup;
		if (baseValueAccessor is null)
		{
			// Stryker disable once Boolean : forceReinit only matters when a base class accessor exists; on this branch there is none, so flipping it to true triggers an extra InitializeWith call that is gated by _isUserInitialized || _isInitialized and becomes a no-op.
			matchingSetup = ResolvePropertySetup(propertyName, defaultValueGenerator, null, false);

			return ((IInteractivePropertySetup)matchingSetup).InvokeGetter(interaction, Behavior,
				defaultValueGenerator);
		}
		ExceptionDispatchInfo? capturedBaseException = null;
		Func<TResult> safeBaseValueAccessor = () =>
			{
				try
				{
					return baseValueAccessor();
				}
				catch (Exception ex)
				{
					capturedBaseException = ExceptionDispatchInfo.Capture(ex);
					return default!;
				}
			};

		matchingSetup = ResolvePropertySetup(
			propertyName, defaultValueGenerator, safeBaseValueAccessor, true);

		TResult result = ((IInteractivePropertySetup)matchingSetup).InvokeGetter(interaction, Behavior,
			defaultValueGenerator);

		capturedBaseException?.Throw();
		return result;
	}

	/// <summary>
	///     Executes the setter flow for the property named <paramref name="propertyName" />, honoring configured
	///     setups and the active <see cref="MockBehavior" />.
	/// </summary>
	/// <remarks>
	///     Returns a flag indicating whether the base-class setter should be skipped.
	/// </remarks>
	/// <typeparam name="T">The property's value type.</typeparam>
	/// <param name="propertyName">The simple property name.</param>
	/// <param name="value">The value being assigned.</param>
	/// <returns><see langword="true" /> when the base-class setter should be skipped.</returns>
	public bool SetProperty<T>(string propertyName, T value)
	{
		IInteraction? interaction = null;
		if (!Behavior.SkipInteractionRecording)
		{
			interaction = Interactions.RegisterInteraction(
				new PropertySetterAccess<T>(propertyName, value));
		}

		PropertySetup matchingSetup = ResolvePropertySetup<T>(propertyName, null, null, false);

		((IInteractivePropertySetup)matchingSetup).InvokeSetter(interaction, value, Behavior);
		return ((IInteractivePropertySetup)matchingSetup).SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Member-id-keyed overload of <see cref="SetProperty{T}(string, T)" /> that records via the typed
	///     <see cref="FastPropertySetterBuffer{T}" /> when the mock is wired to a
	///     <see cref="FastMockInteractions" />, falling back to the legacy list otherwise.
	/// </summary>
	/// <typeparam name="T">The property's value type.</typeparam>
	/// <param name="memberId">The generator-emitted member id for the property setter.</param>
	/// <param name="propertyName">The simple property name.</param>
	/// <param name="value">The value being assigned.</param>
	/// <returns><see langword="true" /> when the base-class setter should be skipped.</returns>
	public bool SetProperty<T>(int memberId, string propertyName, T value)
	{
		if (!Behavior.SkipInteractionRecording)
		{
			RecordPropertySetter(memberId, propertyName, value);
		}

		PropertySetup matchingSetup = ResolvePropertySetup<T>(propertyName, null, null, false);

		((IInteractivePropertySetup)matchingSetup).InvokeSetter(null, value, Behavior);
		return ((IInteractivePropertySetup)matchingSetup).SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	private void RecordPropertySetter<T>(int memberId, string propertyName, T value)
	{
		if (Interactions is FastMockInteractions __fast)
		{
			IFastMemberBuffer?[] __buffers = __fast.Buffers;
			if ((uint)memberId < (uint)__buffers.Length &&
			    __buffers[memberId] is FastPropertySetterBuffer<T> __buffer)
			{
				__buffer.Append(propertyName, value);
				return;
			}
		}

		Interactions.RegisterInteraction(new PropertySetterAccess<T>(propertyName, value));
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
		    Setup.TryGetScenario(Scenario, out MockScenarioSetup? scopedBucket) &&
		    scopedBucket.Properties.TryGetValue(propertyName, out PropertySetup? scopedSetup))
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
	///     Records a subscription to the event named <paramref name="name" /> and fires registered
	///     <c>OnSubscribed</c> callbacks.
	/// </summary>
	/// <param name="name">The simple event name.</param>
	/// <param name="target">The subscribing handler's target (<see langword="null" /> for static methods).</param>
	/// <param name="method">The subscribing handler's method.</param>
	/// <exception cref="MockException"><paramref name="method" /> is <see langword="null" />.</exception>
	public void AddEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event subscription may not be null.");
		}

		if (!Behavior.SkipInteractionRecording)
		{
			Interactions.RegisterInteraction(new EventSubscription(name, target, method));
		}

		foreach (EventSetup setup in GetEventSetupsByName(name))
		{
			setup.InvokeSubscribed(target, method);
		}
	}

	/// <summary>
	///     Member-id-keyed overload of <see cref="AddEvent(string, object?, MethodInfo?)" /> that records via the
	///     typed <see cref="FastEventBuffer" /> when the mock is wired to a <see cref="FastMockInteractions" />,
	///     falling back to the legacy list otherwise.
	/// </summary>
	/// <param name="memberId">The generator-emitted member id for the event subscribe.</param>
	/// <param name="name">The simple event name.</param>
	/// <param name="target">The subscribing handler's target (<see langword="null" /> for static methods).</param>
	/// <param name="method">The subscribing handler's method.</param>
	/// <exception cref="MockException"><paramref name="method" /> is <see langword="null" />.</exception>
	public void AddEvent(int memberId, string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event subscription may not be null.");
		}

		if (!Behavior.SkipInteractionRecording)
		{
			RecordEvent(memberId, name, target, method, isSubscribe: true);
		}

		foreach (EventSetup setup in GetEventSetupsByName(name))
		{
			setup.InvokeSubscribed(target, method);
		}
	}

	/// <summary>
	///     Records an unsubscription from the event named <paramref name="name" /> and fires registered
	///     <c>OnUnsubscribed</c> callbacks.
	/// </summary>
	/// <param name="name">The simple event name.</param>
	/// <param name="target">The unsubscribing handler's target (<see langword="null" /> for static methods).</param>
	/// <param name="method">The unsubscribing handler's method.</param>
	/// <exception cref="MockException"><paramref name="method" /> is <see langword="null" />.</exception>
	public void RemoveEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event unsubscription may not be null.");
		}

		if (!Behavior.SkipInteractionRecording)
		{
			Interactions.RegisterInteraction(new EventUnsubscription(name, target, method));
		}

		foreach (EventSetup setup in GetEventSetupsByName(name))
		{
			setup.InvokeUnsubscribed(target, method);
		}
	}

	/// <summary>
	///     Member-id-keyed overload of <see cref="RemoveEvent(string, object?, MethodInfo?)" /> that records via
	///     the typed <see cref="FastEventBuffer" /> when the mock is wired to a <see cref="FastMockInteractions" />,
	///     falling back to the legacy list otherwise.
	/// </summary>
	/// <param name="memberId">The generator-emitted member id for the event unsubscribe.</param>
	/// <param name="name">The simple event name.</param>
	/// <param name="target">The unsubscribing handler's target (<see langword="null" /> for static methods).</param>
	/// <param name="method">The unsubscribing handler's method.</param>
	/// <exception cref="MockException"><paramref name="method" /> is <see langword="null" />.</exception>
	public void RemoveEvent(int memberId, string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event unsubscription may not be null.");
		}

		if (!Behavior.SkipInteractionRecording)
		{
			RecordEvent(memberId, name, target, method, isSubscribe: false);
		}

		foreach (EventSetup setup in GetEventSetupsByName(name))
		{
			setup.InvokeUnsubscribed(target, method);
		}
	}

	private void RecordEvent(int memberId, string name, object? target, MethodInfo method, bool isSubscribe)
	{
		if (Interactions is FastMockInteractions __fast)
		{
			IFastMemberBuffer?[] __buffers = __fast.Buffers;
			if ((uint)memberId < (uint)__buffers.Length &&
			    __buffers[memberId] is FastEventBuffer __buffer)
			{
				__buffer.Append(name, target, method);
				return;
			}
		}

		Interactions.RegisterInteraction(isSubscribe
			? new EventSubscription(name, target, method)
			: (IInteraction)new EventUnsubscription(name, target, method));
	}
}
