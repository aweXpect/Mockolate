using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockolate.Verify;

namespace Mockolate.Tests;

public sealed class WithTests
{
#if NET8_0_OR_GREATER
	[Theory]
	[InlineData(41, 43, true)]
	[InlineData(42, 42, false)]
	[InlineData(42, 44, false)]
	[InlineData(40, 42, false)]
	public async Task Between_Exclusive_ShouldMatchExclusive(int minimum, int maximum, bool expectMatch)
	{
		var sut = With.ValueBetween(minimum).And(maximum).Exclusive();

		bool result = sut.Matches(42);

		await That(result).IsEqualTo(expectMatch);
	}
#endif

#if NET8_0_OR_GREATER
	[Fact]
	public async Task Between_MaximumLessThanMinimum_ShouldThrowArgumentOutOfRangeException()
	{
		void Act() => 
			_ = With.ValueBetween(42.0).And(41.0).Exclusive();

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithMessage("The maximum must be greater than or equal to the minimum.").AsPrefix().And
			.WithParamName("maximum");
	}
#endif

#if NET8_0_OR_GREATER
	[Theory]
	[InlineData(41, 43, true)]
	[InlineData(42, 42, true)]
	[InlineData(43, 44, false)]
	[InlineData(40, 41, false)]
	public async Task Between_ShouldMatchInclusive(int minimum, int maximum, bool expectMatch)
	{
		var sut = With.ValueBetween(minimum).And(maximum);

		bool result = sut.Matches(42);

		await That(result).IsEqualTo(expectMatch);
	}
#endif

	[Theory]
	[InlineData(null, false)]
	[InlineData("", false)]
	[InlineData("foo", true)]
	[InlineData("fo", false)]
	public async Task ImplicitConversion_ShouldCheckForEquality(string? value, bool expectMatch)
	{
		With.Parameter<string> sut = "foo";

		bool result = sut.Matches(value);

		await That(result).IsEqualTo(expectMatch);
	}

	[Theory]
	[InlineData(null, true)]
	[InlineData("", false)]
	[InlineData("foo", false)]
	public async Task ImplicitConversionFromNull_ShouldCheckForEquality(string? value, bool expectMatch)
	{
		With.Parameter<string?> sut = (string?)null;

		bool result = sut.Matches(value);

		await That(result).IsEqualTo(expectMatch);
	}

	[Fact]
	public async Task ShouldOnlyHaveOneParameterlessPrivateConstructor()
	{
		ConstructorInfo[] constructors = typeof(With)
			.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

		await That(constructors).HasSingle().Which
			.For(c => c.GetParameters(), p => p.HasCount(0));

		_ = constructors.Single().Invoke([]);
	}

	[Fact]
	public async Task ToString_ImplicitFromNull_ShouldBeNull()
	{
		int? value = null;
		With.Parameter<int?> sut = value;
		string expectedValue = "null";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_NamedParameter_ShouldReturnExpectedValue()
	{
		With.NamedParameter sut = new("foo", With.Out<int>());
		string expectedValue = "With.Out<int>() foo";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithAny_ShouldReturnExpectedValue()
	{
		With.Parameter<string> sut = With.Any<string>();
		string expectedValue = "With.Any<string>()";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithMatching_ShouldReturnExpectedValue()
	{
		With.Parameter<string> sut = With.Matching<string>(x => x.Length == 3);
		string expectedValue = "With.Matching<string>(x => x.Length == 3)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithNull_ShouldReturnExpectedValue()
	{
		With.Parameter<string> sut = With.Null<string>();
		string expectedValue = "With.Null<string>()";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithOut_Invoked_ShouldReturnExpectedValue()
	{
		With.InvokedOutParameter<int> sut = With.Out<int>();
		string expectedValue = "With.Out<int>()";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithOut_ShouldReturnExpectedValue()
	{
		With.OutParameter<int> sut = With.Out(() => 3);
		string expectedValue = "With.Out<int>(() => 3)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

#if NET8_0_OR_GREATER
	[Fact]
	public async Task ToString_Between_ShouldReturnExpectedValue()
	{
		var sut = With.ValueBetween(4).And(2 * 3);
		string expectedValue = "With.ValueBetween<int>(4).And(2 * 3)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}
#endif

#if NET8_0_OR_GREATER
	[Fact]
	public async Task ToString_Between_Exclusive_ShouldMatchExpectedValue()
	{
		var sut = With.ValueBetween(4).And(2 * 3).Exclusive();
		string expectedValue = "With.ValueBetween<int>(4).And(2 * 3).Exclusive()";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}
#endif

	[Fact]
	public async Task ToString_WithRef_Invoked_ShouldReturnExpectedValue()
	{
		With.InvokedRefParameter<int> sut = With.Ref<int>();
		string expectedValue = "With.Ref<int>()";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithRef_ShouldReturnExpectedValue()
	{
		With.RefParameter<int?> sut = With.Ref<int?>(v => v * 3);
		string expectedValue = "With.Ref<int?>(v => v * 3)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithRef_WithPredicate_ShouldReturnExpectedValue()
	{
		With.RefParameter<int?> sut = With.Ref<int?>(v => v > 3, v => v * 3);
		string expectedValue = "With.Ref<int?>(v => v > 3, v => v * 3)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithValue_ShouldReturnExpectedValue()
	{
		With.Parameter<string> sut = With.Value("foo");
		string expectedValue = "\"foo\"";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithValueWithComparer_ShouldReturnExpectedValue()
	{
		With.Parameter<int> sut = With.Value(4, new AllEqualComparer());
		string expectedValue = "With.Value(4, new AllEqualComparer())";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("foo")]
	public async Task WithAny_ShouldAlwaysMatch(string? value)
	{
		With.Parameter<string> sut = With.Any<string>();

		bool result = sut.Matches(value);

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData(null, true)]
	[InlineData(1, false)]
	public async Task WithMatching_CheckForNull_ShouldMatchForExpectedResult(int? value, bool expectedResult)
	{
		With.Parameter<int?> sut = With.Matching<int?>(v => v is null);

		bool result = sut.Matches(value);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithMatching_DifferentType_ShouldNotMatch(object? value)
	{
		With.Parameter<int?> sut = With.Matching<int?>(_ => true);

		bool result = sut.Matches(value);

		await That(result).IsFalse();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task WithMatching_ShouldMatchForExpectedResult(bool predicateValue)
	{
		With.Parameter<string> sut = With.Matching<string>(_ => predicateValue);

		bool result = sut.Matches("foo");

		await That(result).IsEqualTo(predicateValue);
	}

	[Theory]
	[InlineData(null, 1)]
	[InlineData(1, 0)]
	public async Task WithNull_ShouldMatchWhenNull(int? value, int expectedCount)
	{
		Mock<IMyServiceWithNullable> mock = Mock.Create<IMyServiceWithNullable>();
		mock.Setup.Method.DoSomething(null, true);

		mock.Subject.DoSomething(value, true);

		await That(mock.Verify.Invoked.DoSomething(null, true)).Exactly(expectedCount);
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithOut_Invoked_ShouldAlwaysMatch(object? value)
	{
		With.InvokedOutParameter<int?> sut = With.Out<int?>();

		bool result = sut.Matches(value);

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithOut_ShouldAlwaysMatch(object? value)
	{
		With.OutParameter<int?> sut = With.Out<int?>(() => null);

		bool result = sut.Matches(value);

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData(42)]
	[InlineData(-2)]
	public async Task WithOut_ShouldReturnValue(int? value)
	{
		With.OutParameter<int?> sut = With.Out(() => value);

		int? result = sut.GetValue();

		await That(result).IsEqualTo(value);
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithRef_DifferentType_ShouldNotMatch(object? value)
	{
		With.RefParameter<int?> sut = With.Ref<int?>(_ => true, _ => null);

		bool result = sut.Matches(value);

		await That(result).IsFalse();
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithRef_Invoked_ShouldAlwaysMatch(object? value)
	{
		With.InvokedRefParameter<int?> sut = With.Ref<int?>();

		bool result = sut.Matches(value);

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task WithRef_ShouldMatchForExpectedResult(bool predicateValue)
	{
		With.RefParameter<string> sut = With.Ref<string>(_ => predicateValue, _ => "");

		bool result = sut.Matches("foo");

		await That(result).IsEqualTo(predicateValue);
	}

	[Theory]
	[InlineData(42)]
	[InlineData(-2)]
	public async Task WithRef_ShouldReturnValue(int? value)
	{
		With.RefParameter<int?> sut = With.Ref<int?>(v => v * 2);

		int? result = sut.GetValue(value);

		await That(result).IsEqualTo(2 * value);
	}

	[Theory]
	[InlineData(null, true)]
	[InlineData("", false)]
	[InlineData("foo", false)]
	public async Task WithValue_Nullable_ShouldMatchWhenEqual(string? value, bool expectMatch)
	{
		With.Parameter<string?> sut = With.Value<string?>(null);

		bool result = sut.Matches(value);

		await That(result).IsEqualTo(expectMatch);
	}

	[Theory]
	[InlineData(1, false)]
	[InlineData(5, true)]
	[InlineData(-5, false)]
	[InlineData(42, false)]
	public async Task WithValue_ShouldMatchWhenEqual(int value, bool expectMatch)
	{
		With.Parameter<int> sut = With.Value(5);

		bool result = sut.Matches(value);

		await That(result).IsEqualTo(expectMatch);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(5)]
	[InlineData(-42)]
	public async Task WithValue_WithComparer_ShouldUseComparer(int value)
	{
		With.Parameter<int> sut = With.Value(5, new AllEqualComparer());

		bool result = sut.Matches(value);

		await That(result).IsTrue();
	}

	internal interface IMyServiceWithNullable
	{
		void DoSomething(int? value, bool flag);
	}

	internal class AllEqualComparer : IEqualityComparer<int>
	{
		bool IEqualityComparer<int>.Equals(int x, int y) => true;

		int IEqualityComparer<int>.GetHashCode(int obj) => obj.GetHashCode();
	}
}
