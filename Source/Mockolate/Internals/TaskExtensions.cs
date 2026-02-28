using System.Threading;
using System.Threading.Tasks;

namespace Mockolate.Internals;

internal static class TaskExtensions
{
	public static Task<T> WaitAsync<T>(this TaskCompletionSource<T> tcs, CancellationToken cancellationToken = default)
	{
#if NET8_0_OR_GREATER
		return tcs.Task.WaitAsync(cancellationToken);
#else
		if (!cancellationToken.CanBeCanceled)
		{
			return tcs.Task;
		}

		CancellationTokenRegistration registration = cancellationToken.Register(() =>
		{
			tcs.TrySetCanceled(cancellationToken);
		});
		return tcs.Task.ContinueWith(
			task =>
			{
				registration.Dispose();
				return task.GetAwaiter().GetResult();
			},
			CancellationToken.None);
#endif
	}
}
