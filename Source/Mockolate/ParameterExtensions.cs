using System.Collections.Generic;
using Mockolate.Parameters;

namespace Mockolate;

/// <summary>
///     Extension methods for <see cref="IParameter" />s.
/// </summary>
public static class ParameterExtensions
{
	/// <summary>
	///     Create a <paramref name="monitor" /> to collect the matched values of the <paramref name="parameter" />.
	/// </summary>
	public static IParameter<T> Monitor<T>(this IParameter<T> parameter,
		out IParameterMonitor<T> monitor)
	{
		ParameterMonitor<T> parameterMonitor = new();
		parameter.Do(v => parameterMonitor.AddValue(v));
		monitor = parameterMonitor;
		return parameter;
	}

	/// <summary>
	///     Create a <paramref name="monitor" /> to collect the matched values of the <paramref name="parameter" />.
	/// </summary>
	public static IRefParameter<T> Monitor<T>(this IRefParameter<T> parameter,
		out IParameterMonitor<T> monitor)
	{
		ParameterMonitor<T> parameterMonitor = new();
		parameter.Do(v => parameterMonitor.AddValue(v));
		monitor = parameterMonitor;
		return parameter;
	}

	/// <summary>
	///     Create a <paramref name="monitor" /> to collect the matched values of the <paramref name="parameter" />.
	/// </summary>
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
