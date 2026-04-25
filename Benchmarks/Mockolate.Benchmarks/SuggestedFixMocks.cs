// Hand-written mocks that demonstrate the suggested fixes from the d-refactor performance
// regression analysis. Side-by-side with the generator-emitted "_Mockolate" benchmarks in
// OptimizedMockComparisonBenchmarks so the same lifecycle (setup → N calls → verify) can be
// measured against both.
//
// Each mock applies the same set of fixes:
//   1. Verify walks the typed Record[] directly (no IFastMemberBuffer.AppendBoxed call,
//      no MethodInvocation<T...> allocation per recorded call).
//   2. Append reserves slots via Interlocked.Increment (no per-buffer Lock).
//   3. Record struct holds only what the verify-time predicate needs — no Seq, no Name,
//      no Boxed reference.
//   4. No InteractionAdded event firing per call (the benchmark workload doesn't subscribe;
//      the runtime fires it unconditionally).

using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Mockolate.Parameters;

namespace Mockolate.Benchmarks.Suggested;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
///     Lock-free typed append-only buffer. Single-writer is fully lock-free; multi-writer
///     correctness is preserved by Interlocked-reserved slot indices and a brief grow lock.
///     Verify walks the published prefix directly via <see cref="Snapshot" />.
/// </summary>
internal sealed class TypedBuffer<TRecord> where TRecord : struct
{
	private TRecord[] _records;
	private int _reserved;
	private int _published;
#if NET10_0_OR_GREATER
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif

	public TypedBuffer(int initialCapacity = 8)
	{
		_records = new TRecord[initialCapacity];
	}

	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Reserves a slot, writes the record, publishes the count. No per-call Lock acquisition
	///     when the backing array doesn't need to grow.
	/// </summary>
	public void Append(in TRecord record)
	{
		int i = Interlocked.Increment(ref _reserved) - 1;
		TRecord[] records = _records;
		if (i >= records.Length)
		{
			lock (_growLock)
			{
				while (i >= _records.Length)
				{
					Array.Resize(ref _records, _records.Length * 2);
				}

				records = _records;
			}
		}

		records[i] = record;
		Interlocked.Increment(ref _published);
	}

	public ReadOnlySpan<TRecord> Snapshot()
	{
		int n = Volatile.Read(ref _published);
		return new ReadOnlySpan<TRecord>(_records, 0, n);
	}
}

internal struct MethodCallRecord<T1, T2>
{
	public T1 P1;
	public T2 P2;
}

internal struct IndexerGetRecord<TKey>
{
	public TKey Key;
}

internal struct EventSubRecord
{
	public Delegate Handler;
}

/// <summary>
///     Hand-written method mock — same shape as the generator-emitted IMockolateMy2ParamInterface mock,
///     with the suggested verify-path fixes applied.
/// </summary>
public sealed class SuggestedMy2ParamMock : OptimizedMockComparisonBenchmarks.IMockolateMy2ParamInterface
{
	internal readonly TypedBuffer<MethodCallRecord<int, string>> _calls = new();
	internal bool _returnValue;

	public SuggestedMy2ParamMock()
	{
		Mock = new Facade(this);
	}

	public Facade Mock { get; }

	public bool MyFunc(int value, string name)
	{
		_calls.Append(new MethodCallRecord<int, string> { P1 = value, P2 = name, });
		return _returnValue;
	}

	public sealed class Facade
	{
		private readonly SuggestedMy2ParamMock _owner;

		internal Facade(SuggestedMy2ParamMock owner)
		{
			_owner = owner;
			Setup = new SetupFacade(owner);
			Verify = new VerifyFacade(owner);
		}

		public SetupFacade Setup { get; }
		public VerifyFacade Verify { get; }
	}

	public sealed class SetupFacade
	{
		private readonly SuggestedMy2ParamMock _owner;

		internal SetupFacade(SuggestedMy2ParamMock owner) => _owner = owner;

		public ReturnsBuilder MyFunc(IParameter<int> value, IParameter<string> name)
			=> new(_owner);

		public sealed class ReturnsBuilder
		{
			private readonly SuggestedMy2ParamMock _owner;
			internal ReturnsBuilder(SuggestedMy2ParamMock owner) => _owner = owner;
			public void Returns(bool value) => _owner._returnValue = value;
		}
	}

	public sealed class VerifyFacade
	{
		private readonly SuggestedMy2ParamMock _owner;

		internal VerifyFacade(SuggestedMy2ParamMock owner) => _owner = owner;

		public MethodVerify MyFunc(IParameter<int> value, IParameter<string> name)
			=> new(_owner._calls, (IParameterMatch<int>)value, (IParameterMatch<string>)name);

		public readonly struct MethodVerify
		{
			private readonly TypedBuffer<MethodCallRecord<int, string>> _buffer;
			private readonly IParameterMatch<int> _m1;
			private readonly IParameterMatch<string> _m2;

			internal MethodVerify(TypedBuffer<MethodCallRecord<int, string>> buffer,
				IParameterMatch<int> m1, IParameterMatch<string> m2)
			{
				_buffer = buffer;
				_m1 = m1;
				_m2 = m2;
			}

			public void Exactly(int times)
			{
				ReadOnlySpan<MethodCallRecord<int, string>> span = _buffer.Snapshot();
				int hit = 0;
				for (int i = 0; i < span.Length; i++)
				{
					ref readonly MethodCallRecord<int, string> r = ref span[i];
					if (_m1.Matches(r.P1) && _m2.Matches(r.P2))
					{
						hit++;
					}
				}

				if (hit != times)
				{
					throw new InvalidOperationException(
						$"Expected MyFunc to be called exactly {times} times but was called {hit} times.");
				}
			}
		}
	}
}

/// <summary>
///     Hand-written property mock for IMockolateCounterInterface. The Counter property has
///     getter+setter; the benchmark only exercises the getter.
/// </summary>
public sealed class SuggestedCounterMock : OptimizedMockComparisonBenchmarks.IMockolateCounterInterface
{
	private readonly TypedBuffer<int> _gets = new();
	private readonly TypedBuffer<int> _sets = new();
	private int _value;

	public SuggestedCounterMock()
	{
		Mock = new Facade(this);
	}

	public Facade Mock { get; }

	public int Counter
	{
		get
		{
			_gets.Append(0);
			return _value;
		}
		set
		{
			_sets.Append(value);
			_value = value;
		}
	}

	public sealed class Facade
	{
		private readonly SuggestedCounterMock _owner;

		internal Facade(SuggestedCounterMock owner)
		{
			_owner = owner;
			Setup = new SetupFacade(owner);
			Verify = new VerifyFacade(owner);
		}

		public SetupFacade Setup { get; }
		public VerifyFacade Verify { get; }
	}

	public sealed class SetupFacade
	{
		private readonly SuggestedCounterMock _owner;
		internal SetupFacade(SuggestedCounterMock owner) => _owner = owner;
		public CounterSetup Counter => new(_owner);

		public readonly struct CounterSetup
		{
			private readonly SuggestedCounterMock _owner;
			internal CounterSetup(SuggestedCounterMock owner) => _owner = owner;
			public void InitializeWith(int value) => _owner._value = value;
		}
	}

	public sealed class VerifyFacade
	{
		private readonly SuggestedCounterMock _owner;
		internal VerifyFacade(SuggestedCounterMock owner) => _owner = owner;
		public CounterVerify Counter => new(_owner);

		public readonly struct CounterVerify
		{
			private readonly SuggestedCounterMock _owner;
			internal CounterVerify(SuggestedCounterMock owner) => _owner = owner;

			public GotChain Got() => new(_owner._gets);

			public readonly struct GotChain
			{
				private readonly TypedBuffer<int> _buffer;
				internal GotChain(TypedBuffer<int> buffer) => _buffer = buffer;

				public void Exactly(int times)
				{
					int hit = _buffer.Count;
					if (hit != times)
					{
						throw new InvalidOperationException(
							$"Expected Counter.Got() to match exactly {times} times but found {hit}.");
					}
				}
			}
		}
	}
}

/// <summary>
///     Hand-written indexer mock for IMockolateKeyIndexerInterface.
/// </summary>
public sealed class SuggestedKeyIndexerMock : OptimizedMockComparisonBenchmarks.IMockolateKeyIndexerInterface
{
	private readonly TypedBuffer<IndexerGetRecord<string>> _gets = new();
	private bool _returnValue;

	public SuggestedKeyIndexerMock()
	{
		Mock = new Facade(this);
	}

	public Facade Mock { get; }

	public bool this[string key]
	{
		get
		{
			_gets.Append(new IndexerGetRecord<string> { Key = key, });
			return _returnValue;
		}
		set => throw new NotSupportedException();
	}

	public sealed class Facade
	{
		private readonly SuggestedKeyIndexerMock _owner;

		internal Facade(SuggestedKeyIndexerMock owner)
		{
			_owner = owner;
			Setup = new SetupFacade(owner);
			Verify = new VerifyFacade(owner);
		}

		public SetupFacade Setup { get; }
		public VerifyFacade Verify { get; }
	}

	public sealed class SetupFacade
	{
		private readonly SuggestedKeyIndexerMock _owner;
		internal SetupFacade(SuggestedKeyIndexerMock owner) => _owner = owner;

		public ReturnsBuilder this[IParameter<string> key] => new(_owner);

		public readonly struct ReturnsBuilder
		{
			private readonly SuggestedKeyIndexerMock _owner;
			internal ReturnsBuilder(SuggestedKeyIndexerMock owner) => _owner = owner;
			public void Returns(bool value) => _owner._returnValue = value;
		}
	}

	public sealed class VerifyFacade
	{
		private readonly SuggestedKeyIndexerMock _owner;
		internal VerifyFacade(SuggestedKeyIndexerMock owner) => _owner = owner;

		public IndexerVerify this[IParameter<string> key] => new(_owner._gets, (IParameterMatch<string>)key);

		public readonly struct IndexerVerify
		{
			private readonly TypedBuffer<IndexerGetRecord<string>> _buffer;
			private readonly IParameterMatch<string> _key;

			internal IndexerVerify(TypedBuffer<IndexerGetRecord<string>> buffer, IParameterMatch<string> key)
			{
				_buffer = buffer;
				_key = key;
			}

			public GotChain Got() => new(_buffer, _key);

			public readonly struct GotChain
			{
				private readonly TypedBuffer<IndexerGetRecord<string>> _buffer;
				private readonly IParameterMatch<string> _key;

				internal GotChain(TypedBuffer<IndexerGetRecord<string>> buffer, IParameterMatch<string> key)
				{
					_buffer = buffer;
					_key = key;
				}

				public void Exactly(int times)
				{
					ReadOnlySpan<IndexerGetRecord<string>> span = _buffer.Snapshot();
					int hit = 0;
					for (int i = 0; i < span.Length; i++)
					{
						if (_key.Matches(span[i].Key))
						{
							hit++;
						}
					}

					if (hit != times)
					{
						throw new InvalidOperationException(
							$"Expected indexer.Got() to match exactly {times} times but was {hit}.");
					}
				}
			}
		}
	}
}

/// <summary>
///     Hand-written event mock for IMockolateSomeEventInterface.
/// </summary>
public sealed class SuggestedSomeEventMock : OptimizedMockComparisonBenchmarks.IMockolateSomeEventInterface
{
	private readonly TypedBuffer<EventSubRecord> _subs = new();
	private readonly TypedBuffer<EventSubRecord> _unsubs = new();

	public SuggestedSomeEventMock()
	{
		Mock = new Facade(this);
	}

	public Facade Mock { get; }

	public event EventHandler SomeEvent
	{
		add => _subs.Append(new EventSubRecord { Handler = value, });
		remove => _unsubs.Append(new EventSubRecord { Handler = value, });
	}

	public sealed class Facade
	{
		private readonly SuggestedSomeEventMock _owner;

		internal Facade(SuggestedSomeEventMock owner)
		{
			_owner = owner;
			Verify = new VerifyFacade(owner);
		}

		public VerifyFacade Verify { get; }
	}

	public sealed class VerifyFacade
	{
		private readonly SuggestedSomeEventMock _owner;
		internal VerifyFacade(SuggestedSomeEventMock owner) => _owner = owner;
		public EventVerify SomeEvent => new(_owner._subs);

		public readonly struct EventVerify
		{
			private readonly TypedBuffer<EventSubRecord> _buffer;
			internal EventVerify(TypedBuffer<EventSubRecord> buffer) => _buffer = buffer;

			public SubscribedChain Subscribed() => new(_buffer);

			public readonly struct SubscribedChain
			{
				private readonly TypedBuffer<EventSubRecord> _buffer;
				internal SubscribedChain(TypedBuffer<EventSubRecord> buffer) => _buffer = buffer;

				public void Exactly(int times)
				{
					int hit = _buffer.Count;
					if (hit != times)
					{
						throw new InvalidOperationException(
							$"Expected SomeEvent.Subscribed() to match exactly {times} times but was {hit}.");
					}
				}
			}
		}
	}
}

#pragma warning restore CA1822
