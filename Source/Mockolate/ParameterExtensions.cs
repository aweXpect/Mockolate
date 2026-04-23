using System.Collections.Generic;
using Mockolate.Parameters;

namespace Mockolate;

/// <summary>
///     Extension methods for <see cref="IParameter" />s.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public static class ParameterExtensions
{
	/// <summary>
	///     Attaches a <paramref name="monitor" /> that records every argument value matched by this parameter.
	/// </summary>
	/// <typeparam name="T">The parameter's value type.</typeparam>
	/// <param name="parameter">The parameter matcher to observe.</param>
	/// <param name="monitor">An out-parameter receiving the monitor; its <see cref="IParameterMonitor{T}.Values" /> list grows as the mock is invoked.</param>
	/// <returns>The same <paramref name="parameter" />, allowing further fluent calls.</returns>
	public static IParameterWithCallback<T> Monitor<T>(this IParameterWithCallback<T> parameter,
		out IParameterMonitor<T> monitor)
	{
		ParameterMonitor<T> parameterMonitor = new();
		monitor = parameterMonitor;
		return parameter.Do(v => parameterMonitor.AddValue(v));
	}

	/// <summary>
	///     Attaches a <paramref name="monitor" /> that records every argument value matched by this ref-parameter.
	/// </summary>
	/// <typeparam name="T">The ref-parameter's value type.</typeparam>
	/// <param name="parameter">The ref-parameter matcher to observe.</param>
	/// <param name="monitor">An out-parameter receiving the monitor; its <see cref="IParameterMonitor{T}.Values" /> list grows as the mock is invoked.</param>
	/// <returns>The same <paramref name="parameter" />, allowing further fluent calls.</returns>
	public static IRefParameter<T> Monitor<T>(this IRefParameter<T> parameter,
		out IParameterMonitor<T> monitor)
	{
		ParameterMonitor<T> parameterMonitor = new();
		parameter.Do(v => parameterMonitor.AddValue(v));
		monitor = parameterMonitor;
		return parameter;
	}

	/// <summary>
	///     Attaches a <paramref name="monitor" /> that records every argument value matched by this out-parameter.
	/// </summary>
	/// <typeparam name="T">The out-parameter's value type.</typeparam>
	/// <param name="parameter">The out-parameter matcher to observe.</param>
	/// <param name="monitor">An out-parameter receiving the monitor; its <see cref="IParameterMonitor{T}.Values" /> list grows as the mock is invoked.</param>
	/// <returns>The same <paramref name="parameter" />, allowing further fluent calls.</returns>
	public static IOutParameter<T> Monitor<T>(this IOutParameter<T> parameter,
		out IParameterMonitor<T> monitor)
	{
		ParameterMonitor<T> parameterMonitor = new();
		parameter.Do(v => parameterMonitor.AddValue(v));
		monitor = parameterMonitor;
		return parameter;
	}

	private sealed class ParameterMonitor<T> : IParameterMonitor<T>
	{
		private readonly List<T> _values = new();

		/// <inheritdoc cref="ParameterMonitor{T}" />
		public ParameterMonitor()
		{
			Values = _values.AsReadOnly();
		}

		/// <inheritdoc cref="IParameterMonitor{T}.Values" />
		public IReadOnlyList<T> Values { get; }

		public void AddValue(T value)
			=> _values.Add(value);
	}
}
