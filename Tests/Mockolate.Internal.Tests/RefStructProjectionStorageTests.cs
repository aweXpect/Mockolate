#if NET9_0_OR_GREATER
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

/// <summary>
///     White-box tests for the projection-based storage on
///     <see cref="RefStructIndexerGetterSetup{TValue, T}" /> and the
///     <see cref="RefStructIndexerSetterSetup{TValue, T}" /> <c>BoundGetter</c> slot. These
///     exercise the internal <c>StoreValue</c> / <c>TryGetStoredValue</c> surface that the
///     end-to-end tests in <c>Mockolate.Tests</c> cannot reach.
/// </summary>
public sealed class RefStructProjectionStorageTests
{
	/// <summary>
	///     Local ref struct used instead of the test-project <c>Packet</c> (which lives in
	///     <c>Mockolate.Tests</c> — not referenced from this project).
	/// </summary>
	private readonly ref struct Key(int id)
	{
		public int Id { get; } = id;
	}

	[Fact]
	public async Task GetterSetup_WithProjection_StoreAndTryGet_RoundTrip()
	{
		IParameter<Key> matcher = It.IsRefStructBy<Key, int>(k => k.Id);
		RefStructIndexerGetterSetup<string, Key> getter = new(
			"get_Item", (IParameterMatch<Key>)matcher);

		getter.StoreValue(new Key(42), "forty-two");

		bool found = getter.TryGetStoredValue(new Key(42), out string? value);

		await That(found).IsTrue();
		await That(value).IsEqualTo("forty-two");
	}

	[Fact]
	public async Task GetterSetup_WithoutProjection_StoreIsNoOp_TryGetReturnsFalse()
	{
		IParameter<Key> matcher = It.IsAnyRefStruct<Key>();
		RefStructIndexerGetterSetup<string, Key> getter = new(
			"get_Item", (IParameterMatch<Key>)matcher);

		getter.StoreValue(new Key(1), "ignored");

		bool found = getter.TryGetStoredValue(new Key(1), out string? _);

		await That(found).IsFalse();
	}

	[Fact]
	public async Task GetterSetup_NullMatcher_HasNoStorage()
	{
		RefStructIndexerGetterSetup<string, Key> getter = new("get_Item", null);

		getter.StoreValue(new Key(1), "ignored");

		bool found = getter.TryGetStoredValue(new Key(1), out _);

		await That(found).IsFalse();
	}

	[Fact]
	public async Task GetterSetup_WithProjectionAndPredicate_StoreSkipsNonMatchingKey()
	{
		IParameter<Key> matcher = It.IsRefStructBy<Key, int>(k => k.Id, id => id > 10);
		RefStructIndexerGetterSetup<string, Key> getter = new(
			"get_Item", (IParameterMatch<Key>)matcher);

		getter.StoreValue(new Key(5), "small"); // rejected by predicate
		getter.StoreValue(new Key(42), "big");  // accepted

		bool smallFound = getter.TryGetStoredValue(new Key(5), out _);
		bool bigFound = getter.TryGetStoredValue(new Key(42), out string? bigValue);

		await That(smallFound).IsFalse();
		await That(bigFound).IsTrue();
		await That(bigValue).IsEqualTo("big");
	}

	[Fact]
	public async Task GetterSetup_WithProjection_Invoke_ReturnsStoredValue_BeforeReturnFactory()
	{
		IRefStructIndexerGetterSetup<string, Key> setup;
		RefStructIndexerGetterSetup<string, Key> getter = new(
			"get_Item",
			(IParameterMatch<Key>)It.IsRefStructBy<Key, int>(k => k.Id));
		setup = getter;
		setup.Returns("fallback");

		getter.StoreValue(new Key(1), "stored");

		string read = getter.Invoke(new Key(1));

		await That(read).IsEqualTo("stored");
	}

	[Fact]
	public async Task GetterSetup_WithProjection_EmptyBucket_Invoke_FallsBackToReturns()
	{
		IRefStructIndexerGetterSetup<string, Key> setup;
		RefStructIndexerGetterSetup<string, Key> getter = new(
			"get_Item",
			(IParameterMatch<Key>)It.IsRefStructBy<Key, int>(k => k.Id));
		setup = getter;
		setup.Returns("fallback");

		string read = getter.Invoke(new Key(99));

		await That(read).IsEqualTo("fallback");
	}

	[Fact]
	public async Task GetterSetup_WithProjection_HasReturnValue_IsTrueEvenWithoutReturnFactory()
	{
		// Activation of the storage dictionary flips HasReturnValue to true so that the
		// generator's emitted getter body takes the return path and serves stored values.
		RefStructIndexerGetterSetup<string, Key> getter = new(
			"get_Item",
			(IParameterMatch<Key>)It.IsRefStructBy<Key, int>(k => k.Id));

		await That(getter.HasReturnValue).IsTrue();
	}

	[Fact]
	public async Task GetterSetup_WithoutProjection_HasReturnValue_IsFalse_UntilReturnsConfigured()
	{
		RefStructIndexerGetterSetup<string, Key> getter = new(
			"get_Item", (IParameterMatch<Key>)It.IsAnyRefStruct<Key>());

		await That(getter.HasReturnValue).IsFalse();

		((IRefStructIndexerGetterSetup<string, Key>)getter).Returns("x");

		await That(getter.HasReturnValue).IsTrue();
	}

	[Fact]
	public async Task CombinedSetup_Constructor_WiresSetterBoundGetter()
	{
		IParameter<Key> matcher = It.IsRefStructBy<Key, int>(k => k.Id);
		RefStructIndexerSetup<string, Key> setup = new(
			"get_Item", "set_Item", (IParameterMatch<Key>)matcher);

		await That(setup.Setter.BoundGetter).IsSameAs(setup.Getter);
	}

	[Fact]
	public async Task CombinedSetup_WithProjection_SetterInvoke_StoresIntoGetter()
	{
		IParameter<Key> matcher = It.IsRefStructBy<Key, int>(k => k.Id);
		RefStructIndexerSetup<string, Key> setup = new(
			"get_Item", "set_Item", (IParameterMatch<Key>)matcher);

		setup.Setter.Invoke(new Key(7), "seven");

		bool found = setup.Getter.TryGetStoredValue(new Key(7), out string? value);

		await That(found).IsTrue();
		await That(value).IsEqualTo("seven");
	}

	[Fact]
	public async Task SetterOnlySetup_BoundGetter_DefaultsToNull_InvokeIsStorageNoOp()
	{
		IParameter<Key> matcher = It.IsRefStructBy<Key, int>(k => k.Id);
		RefStructIndexerSetterSetup<string, Key> setter = new(
			"set_Item", (IParameterMatch<Key>)matcher);

		await That(setter.BoundGetter).IsNull();

		// Should be a no-op for storage but still execute any OnSet/Throws slots (neither set
		// here, so this must simply not throw).
		setter.Invoke(new Key(7), "seven");
	}

	[Fact]
	public async Task CombinedSetup_WithoutProjection_NoStorageActivated()
	{
		IParameter<Key> matcher = It.IsAnyRefStruct<Key>();
		RefStructIndexerSetup<string, Key> setup = new(
			"get_Item", "set_Item", (IParameterMatch<Key>)matcher);

		// BoundGetter is still wired (the combined setup always wires it), but the getter's
		// storage is not active — writes cannot be stored.
		await That(setup.Setter.BoundGetter).IsSameAs(setup.Getter);

		setup.Setter.Invoke(new Key(7), "seven");
		bool found = setup.Getter.TryGetStoredValue(new Key(7), out _);

		await That(found).IsFalse();
	}
}
#endif
