using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mockolate.DefaultValues;

/// <summary>
///     A <see cref="IDefaultValueFactory" /> that creates default values for Task types.
///     If a cancelled <see cref="CancellationToken" /> is present in the parameters, returns a cancelled task.
/// </summary>
internal class CancellableTaskFactory : IDefaultValueFactory
{
	private readonly Type _targetType;
	private readonly Func<object?> _defaultFactory;
	private readonly Func<object> _cancelledFactory;

	private CancellableTaskFactory(Type targetType, Func<object?> defaultFactory, Func<object> cancelledFactory)
	{
		_targetType = targetType;
		_defaultFactory = defaultFactory;
		_cancelledFactory = cancelledFactory;
	}

	/// <summary>
	///     Creates a factory for <see cref="Task" />.
	/// </summary>
	public static CancellableTaskFactory ForTask()
		=> new(typeof(Task),
			() => Task.CompletedTask,
			() => Task.FromCanceled(new CancellationToken(true)));

#if !NETSTANDARD2_0
	/// <summary>
	///     Creates a factory for <see cref="ValueTask" /> (not available on netstandard2.0).
	/// </summary>
	public static CancellableTaskFactory ForValueTask()
		=> new(typeof(ValueTask),
			() => default(ValueTask),
			() => ValueTask.FromCanceled(new CancellationToken(true)));
#endif

	/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
	public bool IsMatch(Type type)
		=> type == _targetType;

	/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object[])" />
	public object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[]? parameters)
	{
		// Check if any parameter is a cancelled CancellationToken
		if (HasCancelledToken(parameters))
		{
			return _cancelledFactory();
		}

		return _defaultFactory();
	}

	private static bool HasCancelledToken(object?[]? parameters)
	{
		if (parameters is null)
		{
			return false;
		}

		return parameters.Any(p => p is CancellationToken token && token.IsCancellationRequested);
	}
}
