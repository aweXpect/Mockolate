namespace Mockolate.Parameters;

/// <summary>
///     Bridges a <see cref="IParameter{T}" /> to <see cref="IParameterMatch{T}" /> for matchers that do not
///     natively implement the strongly-typed match interface.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
internal static class ParameterMatchAdapter
{
	/// <summary>
	///     Returns <paramref name="parameter" /> as an <see cref="IParameterMatch{T}" />. If it already implements the
	///     interface the cast is free; otherwise a small adapter is allocated that delegates to the non-generic
	///     <see cref="IParameter" /> members.
	/// </summary>
	internal static IParameterMatch<T> AsParameterMatch<T>(this IParameter<T> parameter)
		=> parameter as IParameterMatch<T> ?? new Adapter<T>(parameter);

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class Adapter<T>(IParameter<T> parameter) : IParameterMatch<T>
	{
		public bool Matches(T value)
			=> parameter.Matches(value);

		public void InvokeCallbacks(T value)
			=> parameter.InvokeCallbacks(value);

		public override string? ToString()
			=> parameter.ToString();
	}
}
