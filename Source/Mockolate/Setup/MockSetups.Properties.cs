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
	internal PropertySetups Properties { get; } = new();

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
	internal sealed class PropertySetups
	{
		private int _count;
		private List<PropertySetup>? _storage;

		// ReSharper disable once InconsistentlySynchronizedField
		public int Count => Volatile.Read(ref _count);

		private List<PropertySetup> GetOrCreateStorage()
		{
			if (_storage is null)
			{
				Interlocked.CompareExchange(ref _storage, [], null);
			}

			return _storage!;
		}

		public void Add(PropertySetup setup)
		{
			List<PropertySetup> storage = GetOrCreateStorage();
			lock (storage)
			{
				for (int i = 0; i < storage.Count; i++)
				{
					if (string.Equals(storage[i].Name, setup.Name, StringComparison.Ordinal))
					{
						bool wasDefault = storage[i] is PropertySetup.Default;
						bool isDefault = setup is PropertySetup.Default;
						storage[i] = setup;
						if (wasDefault && !isDefault)
						{
							Volatile.Write(ref _count, _count + 1);
						}
						else if (!wasDefault && isDefault)
						{
							Volatile.Write(ref _count, _count - 1);
						}

						return;
					}
				}

				storage.Add(setup);
				if (setup is not PropertySetup.Default)
				{
					Volatile.Write(ref _count, _count + 1);
				}
			}
		}

		public bool TryGetValue(string propertyName, [NotNullWhen(true)] out PropertySetup? setup)
		{
			List<PropertySetup>? storage = _storage;
			if (storage is not null)
			{
				lock (storage)
				{
					for (int i = 0; i < storage.Count; i++)
					{
						if (string.Equals(storage[i].Name, propertyName, StringComparison.Ordinal))
						{
							setup = storage[i];
							return true;
						}
					}
				}
			}

			setup = null;
			return false;
		}

		internal IEnumerable<PropertySetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			List<PropertySetup>? storage = _storage;
			if (storage is null)
			{
				return [];
			}

			lock (storage)
			{
				return storage.Where(propertySetup => interactions.OfType<PropertyAccess>()
						.All(propertyAccess => !((IInteractivePropertySetup)propertySetup).Matches(propertyAccess)))
					.ToList();
			}
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			List<PropertySetup>? storage = _storage;
			if (storage is null || storage.Count == 0)
			{
				return "0 properties";
			}

			lock (storage)
			{
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
}
