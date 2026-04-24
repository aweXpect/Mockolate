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
	internal sealed class PropertySetups
	{
		private int _count;
		private PropertySetup[]? _storage;

		// Member-id-indexed slot table. Each property has a single setup per id (dedup by name is
		// native to Add), so a flat PropertySetup?[] is enough — no per-slot list. Updated
		// copy-on-write under `_writeLock`; the invocation hot path reads via Volatile.Read
		// without a lock. Mirrors the `_byMemberId` pattern used by methods and indexers, but
		// uses a simpler single-slot shape because Add already dedups by name.
		private PropertySetup?[]? _byMemberId;
#if NET10_0_OR_GREATER
		private readonly Lock _writeLock = new();
#else
		private readonly object _writeLock = new();
#endif

		public int Count => Volatile.Read(ref _count);

		public void Add(PropertySetup setup)
		{
			lock (_writeLock)
			{
				PropertySetup[] old = _storage ?? [];
				int existingIndex = -1;
				string setupName = setup.Name;
				for (int i = 0; i < old.Length; i++)
				{
					if (string.Equals(old[i].Name, setupName, StringComparison.Ordinal))
					{
						existingIndex = i;
						break;
					}
				}

				bool isDefault = setup is PropertySetup.Default;
				if (existingIndex >= 0)
				{
					bool wasDefault = old[existingIndex] is PropertySetup.Default;
					// Never overwrite a user-configured setup with a default placeholder:
					// a first property access can race with Setup...InitializeWith(...) and
					// otherwise lose the user's setup.
					if (isDefault && !wasDefault)
					{
						return;
					}

					PropertySetup[] next = new PropertySetup[old.Length];
					Array.Copy(old, next, old.Length);
					next[existingIndex] = setup;
					Volatile.Write(ref _storage, next);
					if (wasDefault && !isDefault)
					{
						Volatile.Write(ref _count, _count + 1);
					}
				}
				else
				{
					PropertySetup[] next = new PropertySetup[old.Length + 1];
					Array.Copy(old, next, old.Length);
					next[old.Length] = setup;
					Volatile.Write(ref _storage, next);
					if (!isDefault)
					{
						Volatile.Write(ref _count, _count + 1);
					}
				}

				UpdateByMemberId(setup, isDefault);
			}
		}

		// Copy-on-write mirror of the name-based store, keyed by dense member id. Same
		// "don't overwrite user setup with default" rule as the name-based path.
		private void UpdateByMemberId(PropertySetup setup, bool isDefault)
		{
			int memberId = setup.MemberId;
			if (memberId < 0)
			{
				return;
			}

			PropertySetup?[] old = _byMemberId ?? [];
			int required = memberId + 1;
			PropertySetup?[] next;
			if (old.Length >= required)
			{
				PropertySetup? existing = old[memberId];
				if (existing is not null && isDefault && existing is not PropertySetup.Default)
				{
					return;
				}

				next = new PropertySetup?[old.Length];
				Array.Copy(old, next, old.Length);
			}
			else
			{
				int newLength = Math.Max(required, Math.Max(old.Length * 2, 4));
				next = new PropertySetup?[newLength];
				Array.Copy(old, next, old.Length);
			}

			next[memberId] = setup;
			Volatile.Write(ref _byMemberId, next);
		}

		public bool TryGetValue(string propertyName, [NotNullWhen(true)] out PropertySetup? setup)
		{
			PropertySetup[]? storage = Volatile.Read(ref _storage);
			if (storage is not null)
			{
				for (int i = 0; i < storage.Length; i++)
				{
					if (string.Equals(storage[i].Name, propertyName, StringComparison.Ordinal))
					{
						setup = storage[i];
						return true;
					}
				}
			}

			setup = null;
			return false;
		}

		/// <summary>
		///     O(1) setup lookup by generator-assigned member id. Invocation hot path — a single
		///     volatile array read then a bounds-checked slot fetch, no locks, no string compare.
		/// </summary>
		public bool TryGetByMemberId(int memberId, [NotNullWhen(true)] out PropertySetup? setup)
		{
			PropertySetup?[]? byMemberId = Volatile.Read(ref _byMemberId);
			if (byMemberId is not null && (uint)memberId < (uint)byMemberId.Length)
			{
				PropertySetup? slot = byMemberId[memberId];
				if (slot is not null)
				{
					setup = slot;
					return true;
				}
			}

			setup = null;
			return false;
		}

		internal IEnumerable<PropertySetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			PropertySetup[]? storage = Volatile.Read(ref _storage);
			if (storage is null || storage.Length == 0)
			{
				return [];
			}

			return storage.Where(propertySetup => interactions.OfType<PropertyAccess>()
					.All(propertyAccess => !((IInteractivePropertySetup)propertySetup).Matches(propertyAccess)))
				.ToList();
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			PropertySetup[]? storage = Volatile.Read(ref _storage);
			if (storage is null || storage.Length == 0)
			{
				return "0 properties";
			}

			List<PropertySetup>? setups = null;
			foreach (PropertySetup setup in storage)
			{
				if (setup is not PropertySetup.Default)
				{
					setups ??= [];
					setups.Add(setup);
				}
			}

			if (setups is null || setups.Count == 0)
			{
				return "0 properties";
			}

			StringBuilder sb = new();
			sb.Append(setups.Count).Append(setups.Count == 1 ? " property:" : " properties:").AppendLine();
			foreach (PropertySetup item in setups)
			{
				sb.Append(item).Append(' ').Append(item.Name).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
	}
}
