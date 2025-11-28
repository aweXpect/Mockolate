using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mockolate.DefaultValues;

public static class DefaultValueGeneratorExtensions
{
	/// <summary>
	///     Generates a <see cref="Task" /> of <typeparamref name="T" />, with
	///     the <paramref name="parameters" /> for context.
	/// </summary>
	public static Task<T> Generate<T>(this IDefaultValueGenerator generator, Task<T> nullValue,
		params object?[] parameters)
	{
		CancellationToken cancellationToken = parameters.OfType<CancellationToken>().FirstOrDefault();
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled<T>(cancellationToken);
		}

		return Task.FromResult(generator.Generate(default(T)!, parameters));
	}
#if NET8_0_OR_GREATER
	/// <summary>
	///     Generates a <see cref="ValueTask" /> of <typeparamref name="T" />, with
	///     the <paramref name="parameters" /> for context.
	/// </summary>
	public static ValueTask<T> Generate<T>(this IDefaultValueGenerator generator, ValueTask<T> nullValue,
		params object?[] parameters)
	{
		CancellationToken cancellationToken = parameters.OfType<CancellationToken>().FirstOrDefault();
		if (cancellationToken.IsCancellationRequested)
		{
			return ValueTask.FromCanceled<T>(cancellationToken);
		}

		return ValueTask.FromResult(generator.Generate(default(T)!, parameters));
	}
#endif

	/// <summary>
	///     Generates a default value of type <typeparamref name="T" />, with
	///     the <paramref name="parameters" /> for context.
	/// </summary>
	public static T Generate<T>(this IDefaultValueGenerator generator, T nullValue, params object?[] parameters)
	{
		if (generator.Generate(typeof(T), parameters) is T value)
		{
			return value;
		}

		return nullValue;
	}
}
