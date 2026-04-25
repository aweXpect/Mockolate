// Hand-written mock for IMockolateMy2ParamInterface that mirrors the structure of the
// source-generator output (see
// generated/Mockolate.SourceGenerators/.../Mock.OptimizedMockComparisonBenchmarks_IMockolateMy2ParamInterface.g.cs)
// while applying the suggested D-refactor fixes.
//
// What's the same:
//   - Construction creates a real MockRegistry + FastMockInteractions sized to MemberCount.
//   - Setup chain builds a real ReturnMethodSetup<bool, int, string>.WithParameterCollection
//     and registers it via MockRegistry.SetupMethod (member-id keyed, Phase 4 fast path).
//   - MyFunc body does the same setup-snapshot scan, MockBehavior check, default-value
//     generation, and TriggerCallbacks the generator emits.
//   - Throws MockNotSetupException when behavior demands.
//
// What changes:
//   - The buffer installed at MemberId_MyFunc is FastMethod2BufferOptimized<int, string>
//     instead of FastMethod2Buffer<int, string>.
//   - The MyFunc body's buffer cast targets the optimized type — one token diff vs the
//     generator output.
//   - Verify.MyFunc(...) returns a CountAssert<bool, int, string> that walks the typed
//     buffer via CountMatching for count terminators, instead of going through
//     MockRegistry.VerifyMethod → VerificationResult<T>.CollectMatching →
//     IFastMemberBuffer.AppendBoxed (which boxes every record).
//
// This is the smallest set of changes that recovers the predicted speedup while keeping
// every other piece of Mockolate runtime infrastructure intact.

using System;
using BenchmarkDotNet.Attributes;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Benchmarks.RealisticSuggestedFix;

#pragma warning disable CA1822

public sealed class RealisticSuggestedMy2ParamMock : OptimizedMockComparisonBenchmarks.IMockolateMy2ParamInterface
{
	internal const int MemberId_MyFunc = 0;
	internal const int MemberCount = 1;
	private const string MethodName = "Mockolate.Benchmarks.OptimizedMockComparisonBenchmarks.IMockolateMy2ParamInterface.MyFunc";

	private readonly MockRegistry _mockRegistry;
	private readonly FastMethod2BufferOptimized<int, string> _myFuncBuffer;

	public RealisticSuggestedMy2ParamMock() : this(MockBehavior.Default)
	{
	}

	public RealisticSuggestedMy2ParamMock(MockBehavior behavior)
	{
		FastMockInteractions interactions = new(MemberCount, behavior.SkipInteractionRecording);
		_myFuncBuffer = interactions.InstallMethodOptimized<int, string>(MemberId_MyFunc, MethodName);
		_mockRegistry = new MockRegistry(behavior, interactions);
		Mock = new Facade(this);
	}

	public Facade Mock { get; }

	public bool MyFunc(int value, string name)
	{
		ReturnMethodSetup<bool, int, string>? methodSetup = null;
		if (string.IsNullOrEmpty(_mockRegistry.Scenario))
		{
			MethodSetup[]? snapshot = _mockRegistry.GetMethodSetupSnapshot(MemberId_MyFunc);
			if (snapshot is not null)
			{
				for (int i = snapshot.Length - 1; i >= 0; i--)
				{
					if (snapshot[i] is ReturnMethodSetup<bool, int, string> typed && typed.Matches(value, name))
					{
						methodSetup = typed;
						break;
					}
				}
			}
		}

		if (methodSetup is null)
		{
			foreach (ReturnMethodSetup<bool, int, string> s in _mockRegistry.GetMethodSetups<ReturnMethodSetup<bool, int, string>>(MethodName))
			{
				if (s.Matches(value, name))
				{
					methodSetup = s;
					break;
				}
			}
		}

		if (!_mockRegistry.Behavior.SkipInteractionRecording)
		{
			_myFuncBuffer.Append(MethodName, value, name);
		}

		try
		{
			// (no Wraps support in the benchmark)
		}
		finally
		{
			methodSetup?.TriggerCallbacks(value, name);
		}

		if (methodSetup is null && _mockRegistry.Behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException($"The method '{MethodName}(int, string)' was invoked without prior setup.");
		}

		return methodSetup?.TryGetReturnValue(value, name, out bool returnValue) == true
			? returnValue
			: _mockRegistry.Behavior.DefaultValue.Generate(default(bool)!, value, name);
	}

	public sealed class Facade
	{
		internal Facade(RealisticSuggestedMy2ParamMock owner)
		{
			Setup = new SetupFacade(owner);
			Verify = new VerifyFacade(owner);
		}

		public SetupFacade Setup { get; }
		public VerifyFacade Verify { get; }
	}

	public sealed class SetupFacade
	{
		private readonly RealisticSuggestedMy2ParamMock _owner;
		internal SetupFacade(RealisticSuggestedMy2ParamMock owner) => _owner = owner;

		public IReturnMethodSetupWithCallback<bool, int, string> MyFunc(IParameter<int>? value, IParameter<string>? name)
		{
			ReturnMethodSetup<bool, int, string>.WithParameterCollection setup = new(
				_owner._mockRegistry,
				MethodName,
				CovariantParameterAdapter<int>.Wrap(value ?? It.IsNull<int>("null")),
				CovariantParameterAdapter<string>.Wrap(name ?? It.IsNull<string>("null")));
			_owner._mockRegistry.SetupMethod(MemberId_MyFunc, setup);
			return setup;
		}
	}

	public sealed class VerifyFacade
	{
		private readonly RealisticSuggestedMy2ParamMock _owner;
		internal VerifyFacade(RealisticSuggestedMy2ParamMock owner) => _owner = owner;

		/// <summary>
		///     Returns a <see cref="CountAssert" /> bound to the typed buffer's CountMatching path —
		///     bypasses MockRegistry.VerifyMethod / VerificationResult.CollectMatching / AppendBoxed.
		/// </summary>
		public CountAssert MyFunc(IParameter<int>? value, IParameter<string>? name)
			=> new(
				_owner._myFuncBuffer,
				CovariantParameterAdapter<int>.Wrap(value ?? It.IsNull<int>("null")),
				CovariantParameterAdapter<string>.Wrap(name ?? It.IsNull<string>("null")));

		public readonly struct CountAssert
		{
			private readonly FastMethod2BufferOptimized<int, string> _buffer;
			private readonly IParameterMatch<int> _match1;
			private readonly IParameterMatch<string> _match2;

			internal CountAssert(FastMethod2BufferOptimized<int, string> buffer,
				IParameterMatch<int> match1, IParameterMatch<string> match2)
			{
				_buffer = buffer;
				_match1 = match1;
				_match2 = match2;
			}

			public void Exactly(int times)
			{
				int hit = _buffer.CountMatching(_match1, _match2);
				if (hit != times)
				{
					throw new MockVerificationException(
						$"Expected {MethodName} to be called exactly {times} time(s) but was called {hit} time(s).");
				}
			}
		}
	}

	private sealed class CovariantParameterAdapter<T>(IParameter inner) : IParameterMatch<T>
	{
		public bool Matches(T value) => inner.Matches(value);
		public void InvokeCallbacks(T value) => inner.InvokeCallbacks(value);
		public override string? ToString() => inner.ToString();

		public static IParameterMatch<T> Wrap(IParameter<T> parameter)
			=> parameter as IParameterMatch<T> ?? new CovariantParameterAdapter<T>(parameter);
	}
}

#pragma warning restore CA1822
