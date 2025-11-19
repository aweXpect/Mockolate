using System.Collections.Generic;

namespace Mockolate;

/// <summary>
///     Extension methods for <see cref="Match" />.
/// </summary>
public static class MatchExtensions
{
	/// <summary>
	///     Create a <paramref name="monitor" /> to collect the matched values of the <paramref name="parameter" />.
	/// </summary>
	public static Match.IParameter<T> Monitor<T>(this Match.IParameter<T> parameter,
		out Match.IParameterMonitor<T> monitor)
	{
		ParameterMonitor<T> parameterMonitor = new();
		parameter.Do(v => parameterMonitor.AddValue(v));
		monitor = parameterMonitor;
		return parameter;
	}

	/// <summary>
	///     Create a <paramref name="monitor" /> to collect the matched values of the <paramref name="parameter" />.
	/// </summary>
	public static Match.IRefParameter<T> Monitor<T>(this Match.IRefParameter<T> parameter,
		out Match.IParameterMonitor<T> monitor)
	{
		ParameterMonitor<T> parameterMonitor = new();
		parameter.Do(v => parameterMonitor.AddValue(v));
		monitor = parameterMonitor;
		return parameter;
	}

	/// <summary>
	///     Create a <paramref name="monitor" /> to collect the matched values of the <paramref name="parameter" />.
	/// </summary>
	public static Match.IOutParameter<T> Monitor<T>(this Match.IOutParameter<T> parameter,
		out Match.IParameterMonitor<T> monitor)
	{
		ParameterMonitor<T> parameterMonitor = new();
		parameter.Do(v => parameterMonitor.AddValue(v));
		monitor = parameterMonitor;
		return parameter;
	}

	private sealed class ParameterMonitor<T> : Match.IParameterMonitor<T>
	{
		private readonly List<T> _values = new();

		/// <inheritdoc cref="ParameterMonitor{T}" />
		public ParameterMonitor()
		{
			Values = _values.AsReadOnly();
		}

		/// <inheritdoc cref="Match.IParameterMonitor{T}.Values" />
		public IReadOnlyList<T> Values { get; }

		public void AddValue(T value)
			=> _values.Add(value);
	}
}
