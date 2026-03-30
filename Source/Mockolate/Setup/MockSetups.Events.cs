using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Mockolate.Setup;

internal partial class MockSetups
{
	internal EventSetups Events { get; } = new();

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
	internal sealed class EventSetups
	{
		private Storage? _storage;

		public int Count
		{
			get
			{
				Storage? storage = _storage;
				if (storage is null)
				{
					return 0;
				}

				lock (storage)
				{
					return storage.List.Count;
				}
			}
		}

		public IEnumerable<(object? Target, MethodInfo Method)> Enumerate(string eventName)
		{
			Storage? storage = _storage;
			if (storage is null)
			{
				return [];
			}

			lock (storage)
			{
				List<(object? Target, MethodInfo Method, string Name)> list = storage.List;
				if (list.Count == 0)
				{
					return [];
				}

				List<(object?, MethodInfo)>? result = null;
				foreach ((object? target, MethodInfo method, string name) in list)
				{
					if (name == eventName)
					{
						result ??= [];
						result.Add((target, method));
					}
				}

				return result ?? [];
			}
		}

		public void Add(object? target, MethodInfo method, string eventName)
		{
			Storage storage = GetOrCreateStorage();
			lock (storage)
			{
				foreach ((object? Target, MethodInfo Method, string Name) entry in storage.List)
				{
					if (Matches(entry, target, method, eventName))
					{
						return;
					}
				}

				storage.List.Add((target, method, eventName));
			}
		}

		public void Remove(object? target, MethodInfo method, string eventName)
		{
			Storage? storage = _storage;
			if (storage is null)
			{
				return;
			}

			lock (storage)
			{
				List<(object? Target, MethodInfo Method, string Name)> list = storage.List;
				for (int i = 0; i < list.Count; i++)
				{
					if (Matches(list[i], target, method, eventName))
					{
						list.RemoveAt(i);
						return;
					}
				}
			}
		}

		private Storage GetOrCreateStorage()
		{
			if (_storage is null)
			{
				Interlocked.CompareExchange(ref _storage, new Storage(), null);
			}

			return _storage!;
		}

		private static bool Matches(
			(object? Target, MethodInfo Method, string Name) entry,
			object? target,
			MethodInfo method,
			string eventName)
			=> EqualityComparer<object?>.Default.Equals(entry.Target, target)
			   && entry.Method.Equals(method)
			   && entry.Name == eventName;

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			Storage? storage = _storage;
			if (storage is null)
			{
				return "0 events";
			}

			(object? Target, MethodInfo Method, string Name)[] snapshot;
			lock (storage)
			{
				List<(object? Target, MethodInfo Method, string Name)> list = storage.List;
				if (list.Count == 0)
				{
					return "0 events";
				}

				snapshot = list.ToArray();
			}

			StringBuilder sb = new();
			sb.Append(snapshot.Length).Append(snapshot.Length == 1 ? " event:" : " events:").AppendLine();
			foreach ((object? _, MethodInfo _, string name) in snapshot)
			{
				sb.Append(name).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}

		private sealed class Storage
		{
			internal readonly List<(object? Target, MethodInfo Method, string Name)> List = [];
		}
	}
}
