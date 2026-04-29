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
				// ReSharper disable once LocalizableElement
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
	///     Returns the current default-scope property setup registered under the generator-emitted
	///     <paramref name="memberId" />, or <see langword="null" /> when no setup has been registered.
	/// </summary>
	/// <remarks>
	///     Reads are lock-free. Only sees setups registered via the <c>SetupProperty(int, ...)</c> overloads;
	///     scenario-scoped registrations still go through the string-keyed fallback.
	/// </remarks>
	/// <param name="memberId">The generator-emitted member id.</param>
	/// <returns>The property setup for the member, or <see langword="null" /> when none is registered.</returns>
	public PropertySetup? GetPropertySetupSnapshot(int memberId)
	{
		PropertySetup?[]? table = Volatile.Read(ref _propertySetupsByMemberId);
		if (table is null || (uint)memberId >= (uint)table.Length)
		{
			return null;
		}

		return table[memberId];
	}

	/// <summary>
	///     Returns the current snapshot of default-scope indexer setups registered under the generator-emitted
	///     <paramref name="memberId" />, or <see langword="null" /> when no setup has been registered.
	/// </summary>
	/// <remarks>
	///     The returned array is immutable — callers may iterate it without a lock. Setups are appended in
	///     registration order, so callers walking for latest-registered-first should iterate in reverse.
	/// </remarks>
	/// <param name="memberId">The generator-emitted member id.</param>
	/// <returns>The setups array for the member, or <see langword="null" /> when none are registered.</returns>
	public IndexerSetup[]? GetIndexerSetupSnapshot(int memberId)
	{
		IndexerSetup[]?[]? table = Volatile.Read(ref _indexerSetupsByMemberId);
		if (table is null || (uint)memberId >= (uint)table.Length)
		{
			return null;
		}

		return table[memberId];
	}

	/// <summary>
	///     Returns the current snapshot of default-scope event setups registered under the generator-emitted
	///     subscribe-side <paramref name="memberId" />, or <see langword="null" /> when no setup has been registered.
	/// </summary>
	/// <remarks>
	///     The returned array is immutable — callers may iterate it without a lock. Setups are appended in
	///     registration order. Both subscribe and unsubscribe dispatch consult this snapshot under the
	///     subscribe-side member id; the bucket is shared because a single <see cref="EventSetup" /> wires both
	///     directions.
	/// </remarks>
	/// <param name="memberId">The generator-emitted subscribe-side member id.</param>
	/// <returns>The setups array for the event, or <see langword="null" /> when none are registered.</returns>
	public EventSetup[]? GetEventSetupSnapshot(int memberId)
	{
		EventSetup[]?[]? table = Volatile.Read(ref _eventSetupsByMemberId);
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
	///     Singleton-aware overload of <see cref="GetProperty{TResult}(string, Func{TResult}, Func{TResult}?)" />.
	///     Records the shared <paramref name="access" /> singleton emitted by the source generator
	///     (one static instance per non-indexer property), avoiding the per-call
	///     <see cref="PropertyGetterAccess" /> allocation. Sharing one reference across recorded
	///     accesses is safe because the only reference-keyed bookkeeping in the codebase tolerates
	///     it — the <c>_verified</c> filter is all-or-nothing per matched property, and
	///     <c>Then</c> walks the snapshot positionally rather than mapping interactions to a
	///     position via a dictionary.
	/// </summary>
	/// <typeparam name="TResult">The property's value type.</typeparam>
	/// <param name="access">The shared <see cref="PropertyGetterAccess" /> singleton for the property.</param>
	/// <param name="defaultValueGenerator">Producer of the default value when no setup supplies one.</param>
	/// <param name="baseValueAccessor">Optional accessor for the base-class getter; when <see langword="null" /> only the default/initial value is considered.</param>
	/// <returns>The resolved getter value.</returns>
	/// <exception cref="MockNotSetupException">No setup exists for the property and <see cref="MockBehavior.ThrowWhenNotSetup" /> is <see langword="true" />.</exception>
	public TResult GetProperty<TResult>(PropertyGetterAccess access, Func<TResult> defaultValueGenerator,
		Func<TResult>? baseValueAccessor)
	{
		IInteraction? interaction = null;
		if (!Behavior.SkipInteractionRecording)
		{
			interaction = Interactions.RegisterInteraction(access);
		}

		return ResolveGetterInternal(access.Name, defaultValueGenerator, baseValueAccessor, interaction);
	}

	/// <summary>
	///     Allocation-free fast-path overload of <see cref="GetProperty{TResult}(string, Func{TResult}, Func{TResult}?)" />.
	///     Avoids the per-call closure allocation by accepting a static <see cref="Func{T, TResult}" /> that takes the
	///     active <see cref="MockBehavior" /> as its argument — the source generator emits a <c>static</c> lambda so
	///     the C# compiler caches the delegate in a static field.
	/// </summary>
	/// <typeparam name="TResult">The property's value type.</typeparam>
	/// <param name="memberId">The generator-emitted member id for the property getter.</param>
	/// <param name="propertyName">The simple property name.</param>
	/// <param name="defaultValueGenerator">Cached factory invoked with the active <see cref="MockBehavior" /> when a default value is needed.</param>
	/// <param name="baseValueAccessor">Optional accessor for the base-class getter; when <see langword="null" /> only the default/initial value is considered. Pass <see langword="null" /> for the no-wrapping fast path.</param>
	/// <returns>The resolved getter value.</returns>
	/// <exception cref="MockNotSetupException">No setup exists for the property and <see cref="MockBehavior.ThrowWhenNotSetup" /> is <see langword="true" />.</exception>
	public TResult GetPropertyFast<TResult>(int memberId, string propertyName,
		Func<MockBehavior, TResult> defaultValueGenerator, Func<TResult>? baseValueAccessor = null)
	{
		if (!Behavior.SkipInteractionRecording)
		{
			RecordPropertyGetter(memberId, propertyName);
		}

		return ResolvePropertyFast(memberId, propertyName, defaultValueGenerator, baseValueAccessor);
	}

	/// <summary>
	///     Singleton-aware overload of
	///     <see cref="GetPropertyFast{TResult}(int, string, Func{MockBehavior, TResult}, Func{TResult}?)" />
	///     that records the shared <paramref name="access" /> singleton emitted by the source generator,
	///     avoiding the per-call <see cref="PropertyGetterAccess" /> allocation. The
	///     <see cref="FastPropertyGetterBuffer" /> stores only a sequence number per call and emits the
	///     same singleton for every recorded record on verification; the cold-path fall-through likewise
	///     registers the singleton.
	/// </summary>
	/// <typeparam name="TResult">The property's value type.</typeparam>
	/// <param name="memberId">The generator-emitted member id for the property getter.</param>
	/// <param name="access">The shared <see cref="PropertyGetterAccess" /> singleton for the property.</param>
	/// <param name="defaultValueGenerator">Cached factory invoked with the active <see cref="MockBehavior" /> when a default value is needed.</param>
	/// <param name="baseValueAccessor">Optional accessor for the base-class getter; when <see langword="null" /> only the default/initial value is considered. Pass <see langword="null" /> for the no-wrapping fast path.</param>
	/// <returns>The resolved getter value.</returns>
	/// <exception cref="MockNotSetupException">No setup exists for the property and <see cref="MockBehavior.ThrowWhenNotSetup" /> is <see langword="true" />.</exception>
	public TResult GetPropertyFast<TResult>(int memberId, PropertyGetterAccess access,
		Func<MockBehavior, TResult> defaultValueGenerator, Func<TResult>? baseValueAccessor = null)
	{
		if (!Behavior.SkipInteractionRecording)
		{
			RecordPropertyGetter(memberId, access);
		}

		return ResolvePropertyFast(memberId, access.Name, defaultValueGenerator, baseValueAccessor);
	}

	private TResult ResolvePropertyFast<TResult>(int memberId, string propertyName,
		Func<MockBehavior, TResult> defaultValueGenerator, Func<TResult>? baseValueAccessor)
	{
		// Hot path: setup registered via SetupProperty(int, ...), no scenario active, no base accessor.
		if (baseValueAccessor is null && string.IsNullOrEmpty(Scenario))
		{
			PropertySetup?[]? table = Volatile.Read(ref _propertySetupsByMemberId);
			if (table is not null && (uint)memberId < (uint)table.Length)
			{
				PropertySetup? snapshot = table[memberId];
				if (snapshot is not null)
				{
					if (!snapshot.IsValueInitialized)
					{
						// First read of a not-yet-initialized setup: seed it with the default value.
						// Mirrors ResolvePropertySetup's contract — without this the next read would
						// auto-init via the cold path and overwrite a value set by a prior setter.
						((IInteractivePropertySetup)snapshot).InitializeWith(defaultValueGenerator(Behavior));
					}

					return snapshot.InvokeGetterFast(Behavior, defaultValueGenerator);
				}
			}
		}

		// Cold path: scenario, uninitialized snapshot, base-class wrapping, or legacy SetupProperty overload.
		// Bridge to the existing closure-based resolver via a per-call adapter — this allocation is
		// acceptable because we only land here outside the hot path.
		MockBehavior behavior = Behavior;
		Func<TResult> bridge = () => defaultValueGenerator(behavior);
		return ResolveGetterInternal(propertyName, bridge, baseValueAccessor, null);
	}

	private void RecordPropertyGetter(int memberId, string propertyName)
	{
		if (Interactions is FastMockInteractions fast)
		{
			IFastMemberBuffer?[] buffers = fast.Buffers;
			if ((uint)memberId < (uint)buffers.Length &&
			    buffers[memberId] is FastPropertyGetterBuffer buffer)
			{
				buffer.Append(propertyName);
				return;
			}
		}

		Interactions.RegisterInteraction(new PropertyGetterAccess(propertyName));
	}

	private void RecordPropertyGetter(int memberId, PropertyGetterAccess access)
	{
		if (Interactions is FastMockInteractions fast)
		{
			IFastMemberBuffer?[] buffers = fast.Buffers;
			if ((uint)memberId < (uint)buffers.Length &&
			    buffers[memberId] is FastPropertyGetterBuffer buffer)
			{
				buffer.Append();
				return;
			}
		}

		Interactions.RegisterInteraction(access);
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

	/// <summary>
	///     Allocation-free fast-path overload of <see cref="SetProperty{T}(int, string, T)" /> that uses the
	///     member-id-keyed property setup snapshot for default-scope dispatch and bypasses the string-keyed
	///     dictionary scan when a snapshot setup is registered.
	/// </summary>
	/// <typeparam name="T">The property's value type.</typeparam>
	/// <param name="memberId">The generator-emitted member id for the property setter.</param>
	/// <param name="propertyName">The simple property name.</param>
	/// <param name="setterMemberId">The generator-emitted member id for the property setter accessor (for FastPropertySetterBuffer dispatch).</param>
	/// <param name="value">The value being assigned.</param>
	/// <returns><see langword="true" /> when the base-class setter should be skipped.</returns>
	public bool SetPropertyFast<T>(int memberId, int setterMemberId, string propertyName, T value)
	{
		if (!Behavior.SkipInteractionRecording)
		{
			RecordPropertySetter(setterMemberId, propertyName, value);
		}

		PropertySetup? matchingSetup = null;
		if (string.IsNullOrEmpty(Scenario))
		{
			PropertySetup?[]? table = Volatile.Read(ref _propertySetupsByMemberId);
			if (table is not null && (uint)memberId < (uint)table.Length)
			{
				matchingSetup = table[memberId];
			}
		}

		if (matchingSetup is not null)
		{
			if (!matchingSetup.IsValueInitialized)
			{
				// Mirror the OLD ResolvePropertySetup<T>(propertyName, null, null, false) call: mark the
				// setup as initialized without overwriting _value, so a subsequent reader's IsValueInitialized
				// check short-circuits to the value about to be written below.
				((IInteractivePropertySetup)matchingSetup).InitializeWith(null);
			}
		}
		else
		{
			matchingSetup = ResolvePropertySetup<T>(propertyName, null, null, false);
		}

		((IInteractivePropertySetup)matchingSetup).InvokeSetter(null, value, Behavior);
		return ((IInteractivePropertySetup)matchingSetup).SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	private void RecordPropertySetter<T>(int memberId, string propertyName, T value)
	{
		if (Interactions is FastMockInteractions fast)
		{
			IFastMemberBuffer?[] buffers = fast.Buffers;
			if ((uint)memberId < (uint)buffers.Length &&
			    buffers[memberId] is FastPropertySetterBuffer<T> buffer)
			{
				buffer.Append(propertyName, value);
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
			RecordEvent(memberId, name, target, method, true);
		}

		// Hot path: setups registered via SetupEvent(int, ...), no scenario active.
		// The snapshot is keyed off the subscribe-side member id (the value the proxy passes here),
		// so a non-null snapshot replaces the string-keyed dictionary scan entirely.
		if (string.IsNullOrEmpty(Scenario))
		{
			EventSetup[]? snapshot = GetEventSetupSnapshot(memberId);
			if (snapshot is not null)
			{
				for (int i = 0; i < snapshot.Length; i++)
				{
					snapshot[i].InvokeSubscribed(target, method);
				}

				return;
			}
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
			RecordEvent(memberId, name, target, method, false);
		}

		// Hot path: setups registered via SetupEvent(int, ...), no scenario active. The proxy passes the
		// unsubscribe-side member id here; a snapshot is only present when the source generator emitted the
		// fast SetupEvent(int, ...) overload AND keyed it under this same id. Today only the subscribe id
		// participates, so the dictionary fall-through still serves the unsubscribe path.
		if (string.IsNullOrEmpty(Scenario))
		{
			EventSetup[]? snapshot = GetEventSetupSnapshot(memberId);
			if (snapshot is not null)
			{
				for (int i = 0; i < snapshot.Length; i++)
				{
					snapshot[i].InvokeUnsubscribed(target, method);
				}

				return;
			}
		}

		foreach (EventSetup setup in GetEventSetupsByName(name))
		{
			setup.InvokeUnsubscribed(target, method);
		}
	}

	private void RecordEvent(int memberId, string name, object? target, MethodInfo method, bool isSubscribe)
	{
		if (Interactions is FastMockInteractions fast)
		{
			IFastMemberBuffer?[] buffers = fast.Buffers;
			if ((uint)memberId < (uint)buffers.Length &&
			    buffers[memberId] is FastEventBuffer buffer)
			{
				buffer.Append(name, target, method);
				return;
			}
		}

		if (isSubscribe)
		{
			Interactions.RegisterInteraction(new EventSubscription(name, target, method));
		}
		else
		{
			Interactions.RegisterInteraction(new EventUnsubscription(name, target, method));
		}
	}
}
