using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Mockerade.Checks;
using Mockerade.Exceptions;

namespace Mockerade.Setup;

/// <summary>
///     Base class for method setups.
/// </summary>
public abstract class MethodSetup : IMethodSetup
{
	private int _invocationCount;

	/// <inheritdoc cref="IMethodSetup.InvocationCount" />
	int IMethodSetup.InvocationCount => _invocationCount;

	internal TResult Invoke<TResult>(Invocation invocation, MockBehavior behavior)
	{
		Interlocked.Increment(ref _invocationCount);
		if (invocation is MethodInvocation methodInvocation)
		{
			ExecuteCallback(methodInvocation, behavior);
			return GetReturnValue<TResult>(methodInvocation, behavior);
		}

		throw new MockException("Invalid registered invocation for a method.");
	}

	internal void Invoke(Invocation invocation, MockBehavior behavior)
	{
		Interlocked.Increment(ref _invocationCount);
		if (invocation is MethodInvocation methodInvocation)
		{
			ExecuteCallback(methodInvocation, behavior);
		}
	}

	/// <summary>
	/// Sets an <see langword="out" /> parameter with the specified name and returns its generated value of type <typeparamref name="T"/>.
	/// </summary>
	/// <remarks>
	/// If a setup is configured, the value is generated according to the setup; otherwise, a default value
	/// is generated using the current <paramref name="behavior"/>.
	/// </remarks>
	protected internal abstract T SetOutParameter<T>(string parameterName, MockBehavior behavior);

	/// <summary>
	/// Sets an <see langword="ref" /> parameter with the specified name and the initial <paramref name="value"/> and returns its generated value of type <typeparamref name="T"/>.
	/// </summary>
	/// <remarks>
	/// If a setup is configured, the value is generated according to the setup; otherwise, a default value
	/// is generated using the current <paramref name="behavior"/>.
	/// </remarks>
	protected internal abstract T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior);

	/// <summary>
	///     Execute a potentially registered callback.
	/// </summary>
	protected abstract void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior);

	/// <summary>
	///     Gets the registered return value.
	/// </summary>
	protected abstract TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior);

	/// <inheritdoc cref="IMethodSetup.Matches(Invocation)" />
	bool IMethodSetup.Matches(Invocation invocation)
		=> IsMatch(invocation);

	/// <summary>
	///     Checks if the <paramref name="invocation" /> matches the setup.
	/// </summary>
	protected abstract bool IsMatch(Invocation invocation);

	internal static bool HasRefParameter<T>(With.NamedParameter[] namedParameters, string parameterName, [NotNullWhen(true)] out With.RefParameter<T>? parameter)
	{
		foreach (var namedParameter in namedParameters)
		{
			if (namedParameter.Name.Equals(parameterName, StringComparison.Ordinal) &&
				namedParameter.Parameter is With.RefParameter<T> refParameter)
			{
				parameter = refParameter;
				return true;
			}
		}
		parameter = null;
		return false;
	}

	internal static bool HasOutParameter<T>(With.NamedParameter[] namedParameters, string parameterName, [NotNullWhen(true)] out With.OutParameter<T>? parameter)
	{
		foreach (var namedParameter in namedParameters)
		{
			if (namedParameter.Name.Equals(parameterName, StringComparison.Ordinal) &&
				namedParameter.Parameter is With.OutParameter<T> outParameter)
			{
				parameter = outParameter;
				return true;
			}
		}
		parameter = null;
		return false;
	}

	internal static bool Matches(With.NamedParameter[] namedParameters, object?[] values)
	{
		if (namedParameters.Length != values.Length)
		{
			return false;
		}
		for (int i = 0; i < namedParameters.Length; i++)
		{
			if (!namedParameters[i].Parameter.Matches(values[i]))
			{
				return false;
			}
		}
		return true;
	}
	internal static bool TryCast<T>(object? value, out T result, MockBehavior behavior)
	{
		if (value is T typedValue)
		{
			result = typedValue;
			return true;
		}

		result = behavior.DefaultValueGenerator.Generate<T>();
		return value is null;
	}
}
