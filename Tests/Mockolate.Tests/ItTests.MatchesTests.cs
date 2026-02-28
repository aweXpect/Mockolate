using System.Text.RegularExpressions;
using Mockolate.Parameters;
using Mockolate.Verify;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class MatchesTests
	{
		[Theory]
		[InlineData("foo", "F[aeiou]+o", 0)]
		[InlineData("foo", "f[aeiou]+o", 1)]
		[InlineData("foobar", "f[aeiou]*BAR", 0)]
		[InlineData("foobar", "f[aeiou]*bar", 1)]
		public async Task AsRegex_CaseSensitive_ShouldMatchRegexCaseSensitive(
			string value, string regex, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString(value);

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(It.Matches(regex).AsRegex().CaseSensitive()))
				.Exactly(expectedCount);
		}

		[Theory]
		[InlineData("foo", "g[aeiou]+o", 0)]
		[InlineData("foo", "F[aeiou]+o", 1)]
		[InlineData("foobar", "f[aeiou]*baz", 0)]
		[InlineData("foobar", "f[aeiou]*BAR", 1)]
		public async Task AsRegex_ShouldMatchRegexCaseInsensitive(string value, string regex, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString(value);

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(It.Matches(regex).AsRegex()))
				.Exactly(expectedCount);
		}

		[Fact]
		public async Task AsRegex_WithRegexOptions_ShouldUseRegexOption()
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString("foo");

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(
					It.Matches("F[aeiou]+o").AsRegex(RegexOptions.IgnoreCase)))
				.Exactly(1);
		}

		[Fact]
		public async Task AsRegex_WithTimeout_ShouldApplyTimeoutToRegex()
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();
			mock.DoSomethingWithString("foo");

			void Act()
			{
				mock.VerifyMock.Invoked.DoSomethingWithString(
					It.Matches("F[aeiou]+o").AsRegex(timeout: TimeSpan.FromSeconds(0))).AtLeastOnce();
			}

			await That(Act)
				.Throws<ArgumentOutOfRangeException>()
				.WithParamName("matchTimeout");
		}

		[Theory]
		[InlineData("foo", "F?o", 0)]
		[InlineData("foo", "f?o", 1)]
		[InlineData("foobar", "f*BAR", 0)]
		[InlineData("foobar", "f*bar", 1)]
		[InlineData("foobar", "f?bar", 0)]
		[InlineData("foobar", "f??bar", 1)]
		public async Task CaseSensitive_ShouldMatchWildcardCaseSensitive(
			string value, string wildcard, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString(value);

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(It.Matches(wildcard).CaseSensitive()))
				.Exactly(expectedCount);
		}

		[Fact]
		public async Task ShouldFreezeValuesOnFirstMatch()
		{
			It.IParameterMatches match = It.Matches("F*o");
			match.CaseSensitive();
			IParameter parameter = (IParameter)match;

			bool result1 = parameter.Matches("foo");

			match.CaseSensitive(false);

			bool result2 = parameter.Matches("foo");

			await That(result1).IsFalse();
			await That(result2).IsFalse();
		}

		[Theory]
		[InlineData("foo", "g?o", 0)]
		[InlineData("foo", "F?o", 1)]
		[InlineData("foobar", "f*baz", 0)]
		[InlineData("foobar", "f*BAR", 1)]
		[InlineData("foobar", "f??baz", 0)]
		[InlineData("foobar", "f??bar", 1)]
		public async Task ShouldMatchWildcardCaseInsensitive(string value, string wildcard, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString(value);

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(It.Matches(wildcard))).Exactly(expectedCount);
		}

		[Fact]
		public async Task ToString_AsRegex_CaseSensitive_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Matches("F\"[aeiou]+o").AsRegex().CaseSensitive();
			string expectedValue = "It.Matches(\"F\\\"[aeiou]+o\").AsRegex().CaseSensitive()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_AsRegex_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Matches("F\"[aeiou]+o").AsRegex();
			string expectedValue = "It.Matches(\"F\\\"[aeiou]+o\").AsRegex()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_AsRegex_WithInfiniteMatchTimeout_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Matches("F[aeiou]+o").AsRegex(timeout: Regex.InfiniteMatchTimeout);
			string expectedValue = "It.Matches(\"F[aeiou]+o\").AsRegex()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_AsRegex_WithOptions_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Matches("F[aeiou]+o").AsRegex(RegexOptions.Compiled | RegexOptions.ECMAScript);
			string expectedValue =
				"It.Matches(\"F[aeiou]+o\").AsRegex(RegexOptions.Compiled | RegexOptions.ECMAScript)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_AsRegex_WithOptionsAndTimeout_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Matches("F[aeiou]+o")
				.AsRegex(RegexOptions.Compiled, TimeSpan.FromMilliseconds(400));
			string expectedValue =
				"It.Matches(\"F[aeiou]+o\").AsRegex(RegexOptions.Compiled, TimeSpan.FromMilliseconds(400))";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_AsRegex_WithTimeout_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Matches("F[aeiou]+o").AsRegex(timeout: TimeSpan.FromSeconds(2));
			string expectedValue = "It.Matches(\"F[aeiou]+o\").AsRegex(timeout: TimeSpan.FromSeconds(2))";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_CaseSensitive_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Matches("f*\"oo").CaseSensitive();
			string expectedValue = "It.Matches(\"f*\\\"oo\").CaseSensitive()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Matches("f*\"oo");
			string expectedValue = "It.Matches(\"f*\\\"oo\")";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
