using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches a <see langword="string" /> parameter against the given wildcard <paramref name="pattern" />.
	/// </summary>
	/// <remarks>
	///     Default comparison is case-insensitive.
	///     Use <see cref="IParameterMatches.CaseSensitive(bool)" /> to change this behavior.
	/// </remarks>
	public static IParameterMatches Matches(string pattern)
		=> new MatchesAsWildcardMatch(pattern);

	/// <summary>
	///     A string parameter that matches against a pattern.
	/// </summary>
	public interface IParameterMatches : IParameter<string>
	{
		/// <summary>
		///     Enables case-sensitive matching of the pattern when specified.
		/// </summary>
		IParameterMatches CaseSensitive(bool caseSensitive = true);

		/// <summary>
		///     Matches the pattern directly as a regular expression.
		/// </summary>
		IParameterMatches AsRegex(
			RegexOptions options = RegexOptions.None,
			TimeSpan? timeout = null,
			[CallerArgumentExpression("options")] string doNotPopulateThisValue1 = "",
			[CallerArgumentExpression("timeout")] string doNotPopulateThisValue2 = "");
	}

	private sealed class MatchesAsWildcardMatch(string pattern) : TypedMatch<string>, IParameterMatches
	{
		private bool _caseSensitive;
		private Regex? _regex;
		private RegexOptions _regexOptions = RegexOptions.None;
		private string? _regexOptionsExpression;
		private TimeSpan _timeout = Regex.InfiniteMatchTimeout;
		private string? _timeoutExpression;

		/// <inheritdoc cref="IParameterMatches.CaseSensitive" />
		public IParameterMatches CaseSensitive(bool caseSensitive = true)
		{
			_caseSensitive = caseSensitive;
			return this;
		}

		/// <inheritdoc cref="IParameterMatches.AsRegex(RegexOptions, TimeSpan?, string, string)" />
		public IParameterMatches AsRegex(RegexOptions options = RegexOptions.None,
			TimeSpan? timeout = null,
			[CallerArgumentExpression("options")] string doNotPopulateThisValue1 = "",
			[CallerArgumentExpression("timeout")] string doNotPopulateThisValue2 = "")
		{
			_regexOptions = options;
			_regexOptionsExpression = doNotPopulateThisValue1;
			if (timeout is not null)
			{
				_timeout = timeout.Value;
				_timeoutExpression = doNotPopulateThisValue2;
			}

			return this;
		}

#pragma warning disable S3218
		/// <inheritdoc cref="TypedMatch{T}.Matches(T)" />
		protected override bool Matches(string value)
		{
			_regex ??= (_regexOptionsExpression is not null, _caseSensitive) switch
			{
				(false, false) => new Regex(WildcardToRegularExpression(pattern),
					RegexOptions.Multiline | RegexOptions.IgnoreCase, _timeout),
				(false, true) => new Regex(WildcardToRegularExpression(pattern),
					RegexOptions.Multiline, _timeout),
				(true, false) => new Regex(pattern,
					_regexOptions | RegexOptions.IgnoreCase, _timeout),
				(true, true) => new Regex(pattern,
					_regexOptions, _timeout),
			};
			return _regex.IsMatch(value);
		}
#pragma warning restore S3218

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> (_regexOptionsExpression is not null, _caseSensitive) switch
			{
				(true, false) =>
					$"It.Matches(\"{pattern.Replace("\"", "\\\"")}\").AsRegex({RegexParameterToString(_timeout, _timeoutExpression, _regexOptionsExpression!)})",
				(true, true) =>
					$"It.Matches(\"{pattern.Replace("\"", "\\\"")}\").AsRegex({RegexParameterToString(_timeout, _timeoutExpression, _regexOptionsExpression!)}).CaseSensitive()",
				(false, false) => $"It.Matches(\"{pattern.Replace("\"", "\\\"")}\")",
				(false, true) => $"It.Matches(\"{pattern.Replace("\"", "\\\"")}\").CaseSensitive()",
			};

		private static string RegexParameterToString(TimeSpan timeout, string? timeoutExpression,
			string regexOptionsExpression)
		{
			if (timeout == Regex.InfiniteMatchTimeout)
			{
				return regexOptionsExpression;
			}

			if (string.IsNullOrEmpty(regexOptionsExpression))
			{
				return $"timeout: {timeoutExpression}";
			}

			return $"{regexOptionsExpression}, {timeoutExpression}";
		}

		private static string WildcardToRegularExpression(string value)
		{
			string regex = Regex.Escape(value)
				.Replace("\\?", ".")
				.Replace("\\*", "(?:.|\\n)*");
			return $"^{regex}$";
		}
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
