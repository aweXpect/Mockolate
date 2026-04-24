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
			}
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

		internal IEnumerable<MethodSetup> EnumerateUnusedSetupsBy(IMockInteractions interactions)
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
