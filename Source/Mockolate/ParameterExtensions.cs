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
	/// <remarks>
	///     The bound is inclusive, but because binary floating point cannot represent every decimal fraction exactly,
	///     values that lie exactly on the bound may or may not match.
	///     <para />
	///     The returned matcher does not expose <see cref="It.IIsParameter{T}.Using" />, because a custom comparer
	///     would replace the tolerance again.
	/// </remarks>
	/// <param name="parameter">The equality matcher created by <c>It.Is(value)</c>.</param>
	/// <param name="tolerance">The maximum allowed absolute difference (inclusive).</param>
	/// <param name="doNotPopulateThisValue">Do not populate - captured automatically by the compiler.</param>
	/// <returns>The same <paramref name="parameter" />, allowing callbacks to be attached.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	///     <paramref name="tolerance" /> is negative or <see cref="double.NaN" />.
	/// </exception>
	public static IParameterWithCallback<double> Within(this It.IIsParameter<double> parameter, double tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<double>)parameter).Within(
			(x, y) => IsWithinTolerance(x, y, tolerance), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<double?> Within(this It.IIsParameter<double?> parameter, double tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<double?>)parameter).Within(
			ForNullable<double>((x, y) => IsWithinTolerance(x, y, tolerance)), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<float> Within(this It.IIsParameter<float> parameter, float tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<float>)parameter).Within(
			(x, y) => IsWithinTolerance(x, y, tolerance), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<float?> Within(this It.IIsParameter<float?> parameter, float tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<float?>)parameter).Within(
			ForNullable<float>((x, y) => IsWithinTolerance(x, y, tolerance)), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<decimal> Within(this It.IIsParameter<decimal> parameter, decimal tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<decimal>)parameter).Within(
			(x, y) => IsWithinTolerance(x, y, tolerance), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<decimal?> Within(this It.IIsParameter<decimal?> parameter, decimal tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<decimal?>)parameter).Within(
			ForNullable<decimal>((x, y) => IsWithinTolerance(x, y, tolerance)), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<DateTime> Within(this It.IIsParameter<DateTime> parameter, TimeSpan tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<DateTime>)parameter).Within(
			(x, y) => IsWithinTolerance(x, y, tolerance), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<DateTime?> Within(this It.IIsParameter<DateTime?> parameter,
		TimeSpan tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<DateTime?>)parameter).Within(
			ForNullable<DateTime>((x, y) => IsWithinTolerance(x, y, tolerance)), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<DateTimeOffset> Within(this It.IIsParameter<DateTimeOffset> parameter,
		TimeSpan tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<DateTimeOffset>)parameter).Within(
			(x, y) => IsWithinTolerance(x, y, tolerance), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<DateTimeOffset?> Within(this It.IIsParameter<DateTimeOffset?> parameter,
		TimeSpan tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<DateTimeOffset?>)parameter).Within(
			ForNullable<DateTimeOffset>((x, y) => IsWithinTolerance(x, y, tolerance)), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<TimeSpan> Within(this It.IIsParameter<TimeSpan> parameter, TimeSpan tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<TimeSpan>)parameter).Within(
			(x, y) => IsWithinTolerance(x, y, tolerance), doNotPopulateThisValue);
	}

	/// <inheritdoc cref="Within(It.IIsParameter{double}, double, string)" />
	public static IParameterWithCallback<TimeSpan?> Within(this It.IIsParameter<TimeSpan?> parameter,
		TimeSpan tolerance,
		[CallerArgumentExpression(nameof(tolerance))]
		string doNotPopulateThisValue = "")
	{
		ThrowIfInvalidTolerance(tolerance);
		return ((It.IToleranceParameter<TimeSpan?>)parameter).Within(
			ForNullable<TimeSpan>((x, y) => IsWithinTolerance(x, y, tolerance)), doNotPopulateThisValue);
	}

	/// <summary>
	///     Applies <paramref name="isWithinTolerance" /> when both values are present; otherwise <see langword="null" />
	///     only matches <see langword="null" />, as <see cref="EqualityComparer{T}.Default" /> does.
	/// </summary>
	private static Func<T?, T?, bool> ForNullable<T>(Func<T, T, bool> isWithinTolerance)
		where T : struct
		=> (x, y) => x.HasValue && y.HasValue
			? isWithinTolerance(x.Value, y.Value)
			: x.HasValue == y.HasValue;

	// Equals also accepts NaN and the infinities, for which the difference is NaN and never within the tolerance.
	private static bool IsWithinTolerance(double x, double y, double tolerance)
		=> x.Equals(y) || Math.Abs(x - y) <= tolerance;

	// Equals also accepts NaN and the infinities, for which the difference is NaN and never within the tolerance.
	private static bool IsWithinTolerance(float x, float y, float tolerance)
		=> x.Equals(y) || Math.Abs(x - y) <= tolerance;

	/// <summary>
	///     Whether <paramref name="x" /> and <paramref name="y" /> differ by at most <paramref name="tolerance" />.
	/// </summary>
	/// <remarks>
	///     Subtracting two values of opposite sign can overflow, so for that case the tolerance is subtracted from the
	///     larger value instead; as the tolerance is never negative, that direction always stays in range.
	/// </remarks>
	private static bool IsWithinTolerance(decimal x, decimal y, decimal tolerance)
	{
		decimal larger = x > y ? x : y;
		decimal smaller = x > y ? y : x;
		return larger >= 0m && smaller <= 0m
			? larger - tolerance <= smaller
			: larger - smaller <= tolerance;
	}

	/// <inheritdoc cref="IsWithinTolerance(decimal, decimal, decimal)" />
	private static bool IsWithinTolerance(long x, long y, long tolerance)
	{
		long larger = x > y ? x : y;
		long smaller = x > y ? y : x;
		return larger >= 0 && smaller <= 0
			? larger - tolerance <= smaller
			: larger - smaller <= tolerance;
	}

	// The difference between two DateTime values always fits into a TimeSpan, so it cannot overflow.
	private static bool IsWithinTolerance(DateTime x, DateTime y, TimeSpan tolerance)
		=> (x - y).Duration() <= tolerance;

	// The difference between two DateTimeOffset values always fits into a TimeSpan, so it cannot overflow.
	private static bool IsWithinTolerance(DateTimeOffset x, DateTimeOffset y, TimeSpan tolerance)
		=> (x - y).Duration() <= tolerance;

	private static bool IsWithinTolerance(TimeSpan x, TimeSpan y, TimeSpan tolerance)
		=> IsWithinTolerance(x.Ticks, y.Ticks, tolerance.Ticks);

	private static void ThrowIfInvalidTolerance(double tolerance)
	{
		if (double.IsNaN(tolerance) || tolerance < 0)
		{
			// ReSharper disable once LocalizableElement
			throw new ArgumentOutOfRangeException(nameof(tolerance), "The tolerance must be a non-negative number.");
		}
	}

	private static void ThrowIfInvalidTolerance(decimal tolerance)
	{
		if (tolerance < 0m)
		{
			// ReSharper disable once LocalizableElement
			throw new ArgumentOutOfRangeException(nameof(tolerance), "The tolerance must not be negative.");
		}
	}

	private static void ThrowIfInvalidTolerance(TimeSpan tolerance)
	{
		if (tolerance < TimeSpan.Zero)
		{
			// ReSharper disable once LocalizableElement
			throw new ArgumentOutOfRangeException(nameof(tolerance), "The tolerance must not be negative.");
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
