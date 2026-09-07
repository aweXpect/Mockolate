using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

	/// <summary>
	///     Relaxes the equality check to accept any value whose distance to the expected value is at most
	///     <paramref name="tolerance" />.
	/// </summary>
	/// <param name="parameter">The equality matcher created by <c>It.Is(value)</c>.</param>
	/// <param name="tolerance">The maximum allowed absolute difference (inclusive).</param>
	/// <param name="doNotPopulateThisValue">Do not populate - captured automatically by the compiler.</param>
	/// <returns>The same <paramref name="parameter" />, allowing further fluent calls.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance" /> is negative.</exception>
	public static It.IIsParameter<double> Within(this It.IIsParameter<double> parameter, double tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfNegative(tolerance < 0, nameof(tolerance));
		return parameter.Using(new It.ToleranceComparer<double>((x, y) => Math.Abs(x - y) <= tolerance),
			doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static It.IIsParameter<float> Within(this It.IIsParameter<float> parameter, float tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfNegative(tolerance < 0, nameof(tolerance));
		return parameter.Using(new It.ToleranceComparer<float>((x, y) => Math.Abs(x - y) <= tolerance),
			doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static It.IIsParameter<decimal> Within(this It.IIsParameter<decimal> parameter, decimal tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfNegative(tolerance < 0, nameof(tolerance));
		return parameter.Using(new It.ToleranceComparer<decimal>((x, y) => Math.Abs(x - y) <= tolerance),
			doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static It.IIsParameter<DateTime> Within(this It.IIsParameter<DateTime> parameter, TimeSpan tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfNegative(tolerance < TimeSpan.Zero, nameof(tolerance));
		return parameter.Using(new It.ToleranceComparer<DateTime>((x, y) => (x - y).Duration() <= tolerance),
			doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static It.IIsParameter<TimeSpan> Within(this It.IIsParameter<TimeSpan> parameter, TimeSpan tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfNegative(tolerance < TimeSpan.Zero, nameof(tolerance));
		return parameter.Using(new It.ToleranceComparer<TimeSpan>((x, y) => (x - y).Duration() <= tolerance),
			doNotPopulateThisValue);
	}

	private static void ThrowIfNegative(bool isNegative, string paramName)
	{
		if (isNegative)
		{
			// ReSharper disable once LocalizableElement
			throw new ArgumentOutOfRangeException(paramName, "The tolerance must not be negative.");
		}
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
