using System.Text.RegularExpressions;
using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class MatchesTests
	{
		[Theory]
		[InlineData("foo", "g[aeiou]+o", 0)]
		[InlineData("foo", "F[aeiou]+o", 1)]
		[InlineData("foobar", "f[aeiou]*baz", 0)]
		[InlineData("foobar", "f[aeiou]*BAR", 1)]
		public async Task Matches_AsRegex_IgnoringCase_ShouldMatchRegexCaseInsensitive(
			string value, string regex, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString(value);

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(It.Matches(regex).AsRegex().IgnoringCase()))
				.Exactly(expectedCount);
		}

		[Theory]
		[InlineData("foo", "F[aeiou]+o", 0)]
		[InlineData("foo", "f[aeiou]+o", 1)]
		[InlineData("foobar", "f[aeiou]*BAR", 0)]
		[InlineData("foobar", "f[aeiou]*bar", 1)]
		public async Task Matches_AsRegex_ShouldMatchRegexCaseSensitive(string value, string regex, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString(value);

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(It.Matches(regex).AsRegex()))
				.Exactly(expectedCount);
		}

		[Fact]
		public async Task Matches_AsRegex_WithRegexOptions_ShouldUseRegexOption()
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString("foo");

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(
					It.Matches("F[aeiou]+o").AsRegex(RegexOptions.IgnoreCase)))
				.Exactly(1);
		}

		[Theory]
		[InlineData("foo", "g?o", 0)]
		[InlineData("foo", "F?o", 1)]
		[InlineData("foobar", "f*baz", 0)]
		[InlineData("foobar", "f*BAR", 1)]
		[InlineData("foobar", "f??baz", 0)]
		[InlineData("foobar", "f??bar", 1)]
		public async Task Matches_IgnoringCase_ShouldMatchWildcardCaseInsensitive(
			string value, string wildcard, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString(value);

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(It.Matches(wildcard).IgnoringCase()))
				.Exactly(expectedCount);
		}

		[Theory]
		[InlineData("foo", "F?o", 0)]
		[InlineData("foo", "f?o", 1)]
		[InlineData("foobar", "f*BAR", 0)]
		[InlineData("foobar", "f*bar", 1)]
		[InlineData("foobar", "f?bar", 0)]
		[InlineData("foobar", "f??bar", 1)]
		public async Task Matches_ShouldMatchWildcardCaseSensitive(string value, string wildcard, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithString(value);

			await That(mock.VerifyMock.Invoked.DoSomethingWithString(It.Matches(wildcard))).Exactly(expectedCount);
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
