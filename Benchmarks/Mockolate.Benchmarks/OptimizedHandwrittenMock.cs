// PREVIEW — D-refactor reference harness (Phase 0). Hand-written model of optimizations A+B+C+E+F
// (no per-member buffer; that's the D file). Demonstrates the "feature-safe" speedup achievable
// without touching interaction storage — the target state for Phase 4. Will be deleted in
// Step 7.3 once the generator emits the real thing. Do not depend on any type defined here
// from outside the benchmark project.

using System;
using System.Threading;
using Mockolate;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate.Benchmarks.Optimized;

/// <summary>
///     Hand-written mock of <see cref="IMy2ParamInterface" /> that sketches what the Mockolate source
///     generator could emit if it adopted the six proposed hot-path optimizations. Everything the
///     library already does well — matchers (<c>It.*</c>), the <see cref="Callbacks{T}" /> pipeline,
///     <see cref="MockRegistry" />, <see cref="MockInteractions" />, the full <see cref="VerificationResult{TVerify}" />
///     surface (<c>Exactly</c>, <c>Once</c>, <c>Within</c>, <c>Then</c>, ...) — is reused as-is, so the
///     feature set is identical to a generated mock.
///     <para />
///     What changes on the hot path:
///     <list type="bullet">
///       <item><description>(A) integer member-id dispatch — the member id is a compile-time constant, no string name compare.</description></item>
///       <item><description>(B) no predicate closure — the typed <see cref="FastReturnMethodSetup{TReturn,T1,T2}.MatchesFast" /> runs directly against the matchers stored on the setup.</description></item>
///       <item><description>(C) lock-free setup reads — setups live in a volatile per-member snapshot array; writes take a rare lock, reads are a <see cref="Volatile.Read{T}" />.</description></item>
///       <item><description>(E revised) slim interaction record — <see cref="SlimMethodInvocation{T1,T2}" /> drops the two parameter-name strings kept by <see cref="MethodInvocation{T1,T2}" />. The setup recognizes the slim shape in <see cref="FastReturnMethodSetup{TReturn,T1,T2}.MatchesInteraction" /> so the existing <see cref="VerificationResult{TVerify}" /> pipeline still matches.</description></item>
///       <item><description>(F) direct typed callbacks — setup/verification both use the state-passing <see cref="Callback{TDelegate}.Invoke" /> overload that is already on <see cref="ReturnMethodSetup{TReturn,T1,T2}" />; no per-call closure allocation remains.</description></item>
///     </list>
///     The recording still goes through <see cref="MockRegistry.RegisterInteraction" /> (= the shared
///     <see cref="MockInteractions" /> list) so existing <c>Verify</c>, <c>VerifyThatAllInteractionsAreVerified</c>,
///     unused-setup detection, monitoring, and scenarios all keep working. Proposal (D) per-member
///     buffers would require a Verify rewrite and is deliberately out of scope here — the point of
///     this file is what's achievable without sacrificing features.
/// </summary>
public interface IMy2ParamInterface
{
	bool MyFunc(int value, string name);
}

/// <summary>
///     Mock of <see cref="IMy2ParamInterface" /> that applies optimizations (A), (B), (C), (E revised),
///     (F) while keeping the full feature set of a generator-produced mock.
/// </summary>
public sealed class OptimizedMy2ParamMock : IMy2ParamInterface, IOptimizedMy2ParamMockFor
{
	// Member-id table: the source generator would emit these as const ints. For a single-method
	// interface there's only one slot.
	internal const int MemberId_MyFunc = 0;
	internal const int MemberCount = 1;

	private const string MethodName_MyFunc =
		"Mockolate.Benchmarks.Optimized.IMy2ParamInterface.MyFunc";

	// (A)+(C) Volatile snapshot array indexed by member id. Each slot holds a tiny array of the
	// setups registered for that member, latest-first. Writes copy-on-write under _setupLock;
	// reads are a single Volatile.Read.
	private readonly FastReturnMethodSetup<bool, int, string>[]?[] _setupsByMemberId =
		new FastReturnMethodSetup<bool, int, string>[MemberCount][];
#if NET10_0_OR_GREATER
	private readonly Lock _setupLock = new();
#else
	private readonly object _setupLock = new();
#endif

	public OptimizedMy2ParamMock() : this(MockBehavior.Default)
	{
	}

	public OptimizedMy2ParamMock(MockBehavior behavior)
	{
		Mock = new OptimizedMockFacade(this, behavior);
	}

	public OptimizedMockFacade Mock { get; }

	MockRegistry IOptimizedMy2ParamMockFor.Registry => Mock.Registry;

	// ---- HOT PATH ----
	public bool MyFunc(int value, string name)
	{
		// (A)+(B)+(C) lock-free, typed setup lookup.
		FastReturnMethodSetup<bool, int, string>? matched = null;
		FastReturnMethodSetup<bool, int, string>[]? snapshot =
			Volatile.Read(ref _setupsByMemberId[MemberId_MyFunc]);
		if (snapshot is not null)
		{
			for (int i = snapshot.Length - 1; i >= 0; i--)
			{
				FastReturnMethodSetup<bool, int, string> s = snapshot[i];
				if (s.MatchesFast(value, name))
				{
					matched = s;
					break;
				}
			}
		}

		MockRegistry registry = Mock.Registry;

		// (E revised) slim interaction — no parameter-name strings. Still recorded through the
		// existing MockInteractions so every existing Verify/Monitor/unused-setup tool sees it.
		if (!registry.Behavior.SkipInteractionRecording)
		{
			registry.RegisterInteraction(new SlimMethodInvocation<int, string>(
				MethodName_MyFunc, value, name));
		}

		bool returnValue = default;
		bool hasReturnValue = false;
		try
		{
			if (matched is not null && matched.TryGetReturnValue(value, name, out bool gotValue))
			{
				returnValue = gotValue;
				hasReturnValue = true;
			}
		}
		finally
		{
			// (F) existing TriggerCallbacks uses state-passing Callback.Invoke — no closure alloc.
			matched?.TriggerCallbacks(value, name);
		}

		if (matched is null && !hasReturnValue && registry.Behavior.ThrowWhenNotSetup)
		{
			throw new Exceptions.MockNotSetupException(
				$"The method '{MethodName_MyFunc}(int, string)' was invoked without prior setup.");
		}

		if (hasReturnValue)
		{
			return returnValue;
		}

		return registry.Behavior.DefaultValue.GenerateValue(typeof(bool)) is bool v ? v : default;
	}

	internal void AddMethodSetup(int memberId, FastReturnMethodSetup<bool, int, string> setup)
	{
		lock (_setupLock)
		{
			FastReturnMethodSetup<bool, int, string>[]? existing = _setupsByMemberId[memberId];
			FastReturnMethodSetup<bool, int, string>[] next;
			if (existing is null)
			{
				next = new[] { setup, };
			}
			else
			{
				next = new FastReturnMethodSetup<bool, int, string>[existing.Length + 1];
				Array.Copy(existing, next, existing.Length);
				next[existing.Length] = setup;
			}

			Volatile.Write(ref _setupsByMemberId[memberId], next);
		}

		// Also register in the shared MockSetups so unused-setup detection and other
		// registry-based tooling keep working. This is strictly a setup-time cost.
		Mock.Registry.SetupMethod(setup);
	}
}

internal interface IOptimizedMy2ParamMockFor
{
	MockRegistry Registry { get; }
}

public sealed class OptimizedMockFacade
{
	internal OptimizedMockFacade(OptimizedMy2ParamMock owner, MockBehavior behavior)
	{
		Owner = owner;
		Registry = new MockRegistry(behavior);
		Setup = new OptimizedSetupFacade(owner);
		Verify = new OptimizedVerifyFacade(this);
	}

	internal OptimizedMy2ParamMock Owner { get; }
	public MockRegistry Registry { get; }
	public OptimizedSetupFacade Setup { get; }
	public OptimizedVerifyFacade Verify { get; }
}

public sealed class OptimizedSetupFacade
{
	private readonly OptimizedMy2ParamMock _owner;

	internal OptimizedSetupFacade(OptimizedMy2ParamMock owner)
	{
		_owner = owner;
	}

	public FastReturnMethodSetup<bool, int, string> MyFunc(
		IParameter<int> value, IParameter<string> name)
	{
		// Covariance-safe cast, mirroring what the Mockolate generator already does — every
		// in-the-box matcher (TypedMatch<T>, RefParameterMatch<T>, etc.) implements both.
		FastReturnMethodSetup<bool, int, string> setup =
			new(((IOptimizedMy2ParamMockFor)_owner).Registry,
				"Mockolate.Benchmarks.Optimized.IMy2ParamInterface.MyFunc",
				(IParameterMatch<int>)value, (IParameterMatch<string>)name);
		_owner.AddMethodSetup(OptimizedMy2ParamMock.MemberId_MyFunc, setup);
		return setup;
	}
}

public sealed class OptimizedVerifyFacade
{
	private readonly OptimizedMockFacade _mock;

	internal OptimizedVerifyFacade(OptimizedMockFacade mock)
	{
		_mock = mock;
	}

	public VerificationResult<OptimizedVerifyFacade>.IgnoreParameters MyFunc(
		IParameter<int> value, IParameter<string> name)
	{
		const string methodName = "Mockolate.Benchmarks.Optimized.IMy2ParamInterface.MyFunc";
		IParameterMatch<int> m1 = (IParameterMatch<int>)value;
		IParameterMatch<string> m2 = (IParameterMatch<string>)name;

		// Uses the existing VerifyMethod pipeline — so Exactly/Once/Within/Then/... all work
		// unchanged. The predicate is matched against SlimMethodInvocation to cover the slim
		// records emitted from the hot path.
		return _mock.Registry.VerifyMethod<OptimizedVerifyFacade, SlimMethodInvocation<int, string>>(
			this, methodName,
			m => m1.Matches(m.Parameter1) && m2.Matches(m.Parameter2),
			() => $"{methodName}({value}, {name})");
	}
}

/// <summary>
///     Fast <see cref="ReturnMethodSetup{TReturn, T1, T2}" /> — inherits the base class so every
///     existing fluent API (<c>Returns</c>, <c>Throws</c>, <c>Do</c>, <c>For</c>, <c>Only</c>,
///     <c>When</c>, <c>TransitionTo</c>, <c>SkippingBaseClass</c>, ...) and the state-passing
///     <c>Callback&lt;T&gt;.Invoke</c> overloads light up for free. Only the matching surface is
///     replaced: <see cref="MatchesFast(T1, T2)" /> bypasses the parameter-name routing, and the
///     overridden <c>MatchesInteraction</c> recognizes <see cref="SlimMethodInvocation{T1,T2}" />.
/// </summary>
public sealed class FastReturnMethodSetup<TReturn, T1, T2>
	: ReturnMethodSetup<TReturn, T1, T2>
{
	public FastReturnMethodSetup(MockRegistry mockRegistry, string name,
		IParameterMatch<T1> parameter1, IParameterMatch<T2> parameter2)
		: base(mockRegistry, name)
	{
		Parameter1 = parameter1;
		Parameter2 = parameter2;
	}

	public IParameterMatch<T1> Parameter1 { get; }
	public IParameterMatch<T2> Parameter2 { get; }

	/// <summary>
	///     Hot-path match — called directly from the mock proxy with strongly-typed arguments.
	///     No parameter-name strings, no predicate closure.
	/// </summary>
	public bool MatchesFast(T1 p1, T2 p2)
		=> Parameter1.Matches(p1) && Parameter2.Matches(p2);

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}.Matches(T1, T2)" />
	public override bool Matches(T1 p1Value, T2 p2Value)
		=> Parameter1.Matches(p1Value) && Parameter2.Matches(p2Value);

	/// <inheritdoc />
	protected override bool MatchesInteraction(IMethodInteraction interaction)
	{
		if (interaction is SlimMethodInvocation<T1, T2> slim)
		{
			return Parameter1.Matches(slim.Parameter1) && Parameter2.Matches(slim.Parameter2);
		}

		return base.MatchesInteraction(interaction);
	}

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}.TriggerCallbacks(T1, T2)" />
	public override void TriggerCallbacks(T1 parameter1, T2 parameter2)
	{
		Parameter1.InvokeCallbacks(parameter1);
		Parameter2.InvokeCallbacks(parameter2);
		base.TriggerCallbacks(parameter1, parameter2);
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{FormatType(typeof(TReturn))} {ShortName(Name)}({Parameter1}, {Parameter2})";

	private static string ShortName(string name)
	{
		int i = name.LastIndexOf('.');
		return i < 0 ? name : name.Substring(i + 1);
	}
}

/// <summary>
///     (E revised) A slimmer alternative to <see cref="MethodInvocation{T1,T2}" /> that drops the
///     two parameter-name strings. The generator already has the names as compile-time constants,
///     so they're not needed per-invocation; diagnostic messages fall back to the setup's
///     description. Saves 2 string fields (and their writes) per recorded call.
/// </summary>
public sealed class SlimMethodInvocation<T1, T2> : IMethodInteraction
{
	public SlimMethodInvocation(string name, T1 parameter1, T2 parameter2)
	{
		Name = name;
		Parameter1 = parameter1;
		Parameter2 = parameter2;
	}

	public string Name { get; }
	public T1 Parameter1 { get; }
	public T2 Parameter2 { get; }

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		int i = Name.LastIndexOf('.');
		string shortName = i < 0 ? Name : Name.Substring(i + 1);
		return $"invoke method {shortName}({Parameter1?.ToString() ?? "null"}, {Parameter2?.ToString() ?? "null"})";
	}
}
