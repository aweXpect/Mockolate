using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Mockolate.Interactions;

namespace Mockolate.Setup;

internal partial class MockSetups
{
	[DebuggerDisplay("{ToString()}")]
#if !DEBUG
	[DebuggerNonUserCode]
#endif
	internal sealed class MethodSetups
	{
		private List<MethodSetup>? _storage;

		// Member-id-indexed setup store. Populated alongside `_storage` under the same lock.
		// Mutable lists are kept for setup writes; `_snapshotByMemberId` is an immutable-per-generation
		// snapshot that the invocation hot path reads without acquiring the lock.
		// `_hasStaleSnapshot` is set on every add; the next reader rebuilds the snapshot under the
		// lock. Same pattern as TUnit's `_setupsByMemberId` / `_hasStaleSetups`.
		private List<MethodSetup>?[]? _byMemberId;
		private volatile MethodSetup[]?[]? _snapshotByMemberId;
		private volatile bool _hasStaleSnapshot;

		public int Count
		{
			get
			{
				List<MethodSetup>? storage = _storage;
				if (storage is null)
				{
					return 0;
				}

				lock (storage)
				{
					return storage.Count;
				}
			}
		}

		private List<MethodSetup> GetOrCreateStorage()
		{
			if (_storage is null)
			{
				Interlocked.CompareExchange(ref _storage, [], null);
			}

			return _storage!;
		}

		public void Add(MethodSetup setup)
		{
			List<MethodSetup> storage = GetOrCreateStorage();
			lock (storage)
			{
				storage.Add(setup);
				int memberId = setup.MemberId;
				if (memberId >= 0)
				{
					EnsureByMemberIdCapacity(memberId);
					List<MethodSetup>? list = _byMemberId![memberId];
					if (list is null)
					{
						list = new List<MethodSetup>();
						_byMemberId[memberId] = list;
					}

					list.Add(setup);
					_hasStaleSnapshot = true;
				}
			}
		}

		private void EnsureByMemberIdCapacity(int memberId)
		{
			int required = memberId + 1;
			if (_byMemberId is null)
			{
				int initial = Math.Max(required, 8);
				_byMemberId = new List<MethodSetup>?[initial];
				return;
			}

			if (_byMemberId.Length >= required)
			{
				return;
			}

			int newSize = Math.Max(required, _byMemberId.Length * 2);
			List<MethodSetup>?[] grown = new List<MethodSetup>?[newSize];
			Array.Copy(_byMemberId, grown, _byMemberId.Length);
			_byMemberId = grown;
		}

		private MethodSetup[]?[]? GetSnapshot()
		{
			MethodSetup[]?[]? current = _snapshotByMemberId;
			if (!_hasStaleSnapshot && current is not null)
			{
				return current;
			}

			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				if (!_hasStaleSnapshot)
				{
					return _snapshotByMemberId;
				}

				List<MethodSetup>?[]? byMemberId = _byMemberId;
				if (byMemberId is null)
				{
					_snapshotByMemberId = null;
					_hasStaleSnapshot = false;
					return null;
				}

				MethodSetup[]?[] rebuilt = new MethodSetup[]?[byMemberId.Length];
				for (int i = 0; i < byMemberId.Length; i++)
				{
					List<MethodSetup>? list = byMemberId[i];
					if (list is { Count: > 0, })
					{
						rebuilt[i] = list.ToArray();
					}
				}

				_snapshotByMemberId = rebuilt;
				_hasStaleSnapshot = false;
				return rebuilt;
			}
		}

		/// <summary>
		///     O(1) setup lookup by generator-assigned member id. Returns the latest-registered setup
		///     of type <typeparamref name="T" /> whose <see cref="IMethodMatchByValue" /> implementation
		///     accepts the zero-argument call, or <see langword="null" /> when none matches. Invocation
		///     hot path — reads the volatile snapshot without a lock.
		/// </summary>
		/// <remarks>
		///     Falls back to scanning legacy setups (those registered through the pre-memberId ctor
		///     with <see cref="MethodSetup.MemberId" /> &lt; 0) when the indexed bucket has no match.
		///     Required for third-party extensions that still use the legacy ctor (e.g. Mockolate.Web).
		/// </remarks>
		public T? GetByMemberId<T>(int memberId, string methodName) where T : MethodSetup
		{
			MethodSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				MethodSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						MethodSetup setup = setups[i];
						if (setup is T typed && setup is IMethodMatchByValue m && m.Matches())
						{
							return typed;
						}
					}
				}
			}

			return GetLegacyByValue<T>(methodName);
		}

		/// <summary>
		///     O(1) setup lookup by member id for single-argument methods. Closure-free, lock-free read
		///     on the indexed path; falls back to a name+value scan of legacy setups.
		/// </summary>
		public T? GetByMemberId<T, T1>(int memberId, string methodName, T1 arg1) where T : MethodSetup
		{
			MethodSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				MethodSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						MethodSetup setup = setups[i];
						if (setup is T typed && setup is IMethodMatchByValue<T1> m && m.Matches(arg1))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacyByValue<T, T1>(methodName, arg1);
		}

		/// <summary>
		///     O(1) setup lookup by member id for two-argument methods.
		/// </summary>
		public T? GetByMemberId<T, T1, T2>(int memberId, string methodName, T1 arg1, T2 arg2) where T : MethodSetup
		{
			MethodSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				MethodSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						MethodSetup setup = setups[i];
						if (setup is T typed && setup is IMethodMatchByValue<T1, T2> m && m.Matches(arg1, arg2))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacyByValue<T, T1, T2>(methodName, arg1, arg2);
		}

		/// <summary>
		///     O(1) setup lookup by member id for three-argument methods.
		/// </summary>
		public T? GetByMemberId<T, T1, T2, T3>(int memberId, string methodName, T1 arg1, T2 arg2, T3 arg3)
			where T : MethodSetup
		{
			MethodSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				MethodSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						MethodSetup setup = setups[i];
						if (setup is T typed && setup is IMethodMatchByValue<T1, T2, T3> m
						    && m.Matches(arg1, arg2, arg3))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacyByValue<T, T1, T2, T3>(methodName, arg1, arg2, arg3);
		}

		/// <summary>
		///     O(1) setup lookup by member id for four-argument methods.
		/// </summary>
		public T? GetByMemberId<T, T1, T2, T3, T4>(int memberId, string methodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
			where T : MethodSetup
		{
			MethodSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				MethodSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						MethodSetup setup = setups[i];
						if (setup is T typed && setup is IMethodMatchByValue<T1, T2, T3, T4> m
						    && m.Matches(arg1, arg2, arg3, arg4))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacyByValue<T, T1, T2, T3, T4>(methodName, arg1, arg2, arg3, arg4);
		}

		// Legacy fallback: setups registered with the pre-memberId ctor (MemberId < 0) live only in
		// `_storage`. Scan them under the list lock, newest first, filtering by method name to avoid
		// cross-method matches when several generated proxies share a single MockRegistry instance
		// (e.g. HttpClient + HttpMessageHandler). The check is O(N) over legacy setups only —
		// typically zero on the hot path.
		private T? GetLegacyByValue<T>(string methodName) where T : MethodSetup
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					MethodSetup setup = storage[i];
					if (setup.MemberId >= 0 || !setup.Name.Equals(methodName))
					{
						continue;
					}

					if (setup is T typed && setup is IMethodMatchByValue m && m.Matches())
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacyByValue<T, T1>(string methodName, T1 arg1) where T : MethodSetup
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					MethodSetup setup = storage[i];
					if (setup.MemberId >= 0 || !setup.Name.Equals(methodName))
					{
						continue;
					}

					if (setup is T typed && setup is IMethodMatchByValue<T1> m && m.Matches(arg1))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacyByValue<T, T1, T2>(string methodName, T1 arg1, T2 arg2) where T : MethodSetup
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					MethodSetup setup = storage[i];
					if (setup.MemberId >= 0 || !setup.Name.Equals(methodName))
					{
						continue;
					}

					if (setup is T typed && setup is IMethodMatchByValue<T1, T2> m && m.Matches(arg1, arg2))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacyByValue<T, T1, T2, T3>(string methodName, T1 arg1, T2 arg2, T3 arg3)
			where T : MethodSetup
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					MethodSetup setup = storage[i];
					if (setup.MemberId >= 0 || !setup.Name.Equals(methodName))
					{
						continue;
					}

					if (setup is T typed && setup is IMethodMatchByValue<T1, T2, T3> m
					    && m.Matches(arg1, arg2, arg3))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacyByValue<T, T1, T2, T3, T4>(string methodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
			where T : MethodSetup
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					MethodSetup setup = storage[i];
					if (setup.MemberId >= 0 || !setup.Name.Equals(methodName))
					{
						continue;
					}

					if (setup is T typed && setup is IMethodMatchByValue<T1, T2, T3, T4> m
					    && m.Matches(arg1, arg2, arg3, arg4))
					{
						return typed;
					}
				}
			}

			return null;
		}

		public MethodSetup? GetLatestOrDefault(Func<MethodSetup, bool> predicate)
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					if (predicate(storage[i]))
					{
						return storage[i];
					}
				}
			}

			return null;
		}

		public T? GetMatching<T>(string methodName, Func<T, bool> predicate) where T : MethodSetup
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					if (storage[i].Name.Equals(methodName) &&
					    storage[i] is T methodSetup &&
					    predicate(methodSetup))
					{
						return methodSetup;
					}
				}
			}

			return null;
		}

		/// <summary>
		///     Enumerates all method setups of type <typeparamref name="T" /> matching <paramref name="methodName" />
		///     in latest-registered-first order.
		/// </summary>
		/// <remarks>
		///     Used by the generated code for methods with ref-struct parameters, where the usual
		///     <c>GetMatching</c> predicate cannot capture the ref-struct value. Callers iterate this
		///     sequence and evaluate <c>Matches</c> synchronously on the stack, then invoke the first
		///     matching setup.
		/// </remarks>
		public IEnumerable<T> EnumerateByName<T>(string methodName) where T : MethodSetup
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				yield break;
			}

			// Snapshot the matching entries under lock; yield them without holding the lock so the
			// caller's Matches/Invoke can run user code (including throws) without risk of re-entering
			// the storage lock on the same thread.
			List<T> snapshot;
			lock (storage)
			{
				snapshot = new List<T>(storage.Count);
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					if (storage[i].Name.Equals(methodName) && storage[i] is T typed)
					{
						snapshot.Add(typed);
					}
				}
			}

			foreach (T item in snapshot)
			{
				yield return item;
			}
		}

		internal IEnumerable<MethodSetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return [];
			}

			lock (storage)
			{
				return storage.Where(methodSetup => !interactions.OfType<IMethodInteraction>()
						.Any(methodInvocation => ((IVerifiableMethodSetup)methodSetup).Matches(methodInvocation)))
					.ToList();
			}
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return "0 methods";
			}

			lock (storage)
			{
				int count = storage.Count;
				if (count == 0)
				{
					return "0 methods";
				}

				StringBuilder sb = new();
				sb.Append(count).Append(count == 1 ? " method:" : " methods:").AppendLine();
				foreach (MethodSetup methodSetup in storage)
				{
					sb.Append(methodSetup).AppendLine();
				}

				sb.Length -= Environment.NewLine.Length;
				return sb.ToString();
			}
		}
	}
}
