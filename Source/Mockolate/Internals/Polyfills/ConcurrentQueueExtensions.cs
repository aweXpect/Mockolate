#if NETSTANDARD2_0

// ReSharper disable once CheckNamespace
namespace System.Collections.Concurrent;

/// <summary>
///     Provides polyfill extension methods on <see cref="ConcurrentQueue{T}" />
/// </summary>
public static class ConcurrentQueueExtensions
{
	/// <inheritdoc cref="ConcurrentQueueExtensions" />
	extension<T>(ConcurrentQueue<T> @this)
	{
		/// <summary>
		///     Removes all objects from the <see cref="ConcurrentQueue{T}" />.
		/// </summary>
		public void Clear()
		{
			while (@this.TryDequeue(out _))
			{
				// Dequeue all items to clear the queue.
			}
		}
	}
}

#endif
