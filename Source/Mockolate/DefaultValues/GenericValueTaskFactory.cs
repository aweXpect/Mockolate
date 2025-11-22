#if !NETSTANDARD2_0
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Mockolate.DefaultValues;

/// <summary>
///     A <see cref="IDefaultValueFactory" /> that creates default values for <see cref="ValueTask{T}" /> types.
///     If a cancelled <see cref="CancellationToken" /> is present in the parameters, returns a cancelled task.
/// </summary>
internal class GenericValueTaskFactory : IDefaultValueFactory
{
	/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
	public bool IsMatch(Type type)
		=> type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);

	/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object[])" />
#if !NETSTANDARD2_0
	[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Reflection is required to create ValueTask<T> for arbitrary runtime types.")]
	[UnconditionalSuppressMessage("Trimming", "IL2060", Justification = "Reflection is required to create ValueTask<T> for arbitrary runtime types.")]
#endif
	public object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[]? parameters)
	{
		Type resultType = type.GetGenericArguments()[0];
		
		// Check if any parameter is a cancelled CancellationToken
		if (HasCancelledToken(parameters))
		{
			// ValueTask.FromCanceled<T>(CancellationToken)
			MethodInfo fromCanceledMethod = typeof(ValueTask)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.First(m => m.Name == nameof(ValueTask.FromCanceled) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
			MethodInfo genericMethod = fromCanceledMethod.MakeGenericMethod(resultType);
			return genericMethod.Invoke(null, [new CancellationToken(true)]);
		}

		// ValueTask.FromResult<T>(T)
		MethodInfo fromResultMethod = typeof(ValueTask).GetMethod(nameof(ValueTask.FromResult))!.MakeGenericMethod(resultType);
		object? defaultValue = GenerateForType(defaultValueGenerator, resultType);
		return fromResultMethod.Invoke(null, [defaultValue]);
	}
	
	/// <summary>
	///     Helper to call Generate with a runtime Type.
	/// </summary>
#if !NETSTANDARD2_0
	[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Reflection is required to create ValueTask<T> for arbitrary runtime types.")]
	[UnconditionalSuppressMessage("Trimming", "IL2060", Justification = "Reflection is required to create ValueTask<T> for arbitrary runtime types.")]
#endif
	private static object? GenerateForType(IDefaultValueGenerator generator, Type type)
	{
		MethodInfo method = typeof(IDefaultValueGenerator)
			.GetMethods()
			.First(m => m.Name == nameof(IDefaultValueGenerator.Generate) && m.GetParameters().Length == 0);
		return method.MakeGenericMethod(type).Invoke(generator, null);
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
#endif
