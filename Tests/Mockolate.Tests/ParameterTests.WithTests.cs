using System.ComponentModel;
using Mockolate.Match;

namespace Mockolate.Tests;

public sealed partial class ParameterTests
{
	public sealed class WithTests
	{
		[Fact]
		public async Task ToString_WithPredicate_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = With<string>(x => x.Length == 3);
			string expectedValue = "With<string>(x => x.Length == 3)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithValue_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = With("foo");
			string expectedValue = "\"foo\"";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithValueWithComparer_ShouldReturnExpectedValue()
		{
			IParameter<int> sut = With(4, new AllEqualComparer());
			string expectedValue = "With(4, new AllEqualComparer())";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData(1, false)]
		public async Task WithMatching_CheckForNull_ShouldMatchForExpectedResult(int? value, bool expectedResult)
		{
			IParameter<int?> sut = With<int?>(v => v is null);

			bool result = sut.Matches(value);

			await That(result).IsEqualTo(expectedResult);
		}

		[Theory]
		[InlineData(42L)]
		[InlineData("foo")]
		public async Task WithMatching_DifferentType_ShouldNotMatch(object? value)
		{
			IParameter<int?> sut = With<int?>(_ => true);

			bool result = sut.Matches(value);

			await That(result).IsFalse();
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task WithMatching_ShouldMatchForExpectedResult(bool predicateValue)
		{
			IParameter<string> sut = With<string>(_ => predicateValue);

			bool result = sut.Matches("foo");

			await That(result).IsEqualTo(predicateValue);
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData("", false)]
		[InlineData("foo", false)]
		public async Task WithValue_Nullable_ShouldMatchWhenEqual(string? value, bool expectMatch)
		{
			IParameter<string?> sut = Null<string?>();

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
			IParameter<int> sut = With(5);

			bool result = sut.Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(-42)]
		public async Task WithValue_WithComparer_ShouldUseComparer(int value)
		{
			IParameter<int> sut = With(5, new AllEqualComparer());

			bool result = sut.Matches(value);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task WithPredicate_ShouldSupportCovarianceInSetup()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			mock.Setup.Method.DoSomething(With<MyImplementation>(_ => true))
				.Returns(3);
			var value1 = new MyImplementation();
			var value2 = new MyOtherImplementation();

			var result1 = mock.Subject.DoSomething(value1);
			var result2 = mock.Subject.DoSomething(value2);

			await That(result1).IsEqualTo(3);
			await That(result2).IsEqualTo(0);
		}

		[Fact]
		public async Task WithPredicate_ShouldSupportCovarianceInVerify()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			mock.Setup.Method.DoSomething(With<MyImplementation>(_ => true))
				.Callback(d => d.DoWork())
				.Returns(3);
			var value1 = new MyImplementation();

			var result1 = mock.Subject.DoSomething(value1);

			await That(mock.Verify.Invoked.DoSomething(With<MyImplementation>(p => p.Progress > 0))).Once();
			await That(mock.Verify.Invoked.DoSomething(With<MyImplementation>(p => p.Progress > 1))).Never();
			await That(value1.Progress).IsEqualTo(1);
			await That(mock.Verify.Invoked.DoSomething(With<MyOtherImplementation>(_ => true))).Never();
		}

		[Fact]
		public async Task WithValue_ShouldSupportCovarianceInVerify()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			mock.Setup.Method.DoSomething(With<MyImplementation>(_ => true))
				.Callback(d => d.DoWork())
				.Returns(3);
			var value1 = new MyImplementation();
			var value2 = new MyOtherImplementation();

			var result1 = mock.Subject.DoSomething(value1);

			await That(mock.Verify.Invoked.DoSomething(With(value1))).Once();
			await That(value1.Progress).IsEqualTo(1);
			await That(mock.Verify.Invoked.DoSomething(With(value2))).Never();
		}

		[Fact]
		public async Task WithValue_ShouldSupportCovarianceInSetup()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			var value1 = new MyImplementation();
			var value2 = new MyOtherImplementation();
			mock.Setup.Method.DoSomething(With(value1))
				.Returns(3);

			var result1 = mock.Subject.DoSomething(value1);
			var result2 = mock.Subject.DoSomething(value2);

			await That(result1).IsEqualTo(3);
			await That(result2).IsEqualTo(0);
		}

		public interface IMyBase
		{
			int DoWork();
		}

		public class MyImplementation : IMyBase
		{
			public int Progress { get; private set; }
			public int DoWork()
			{
				Progress++;
				return Progress;
			}
		}

		public class MyOtherImplementation : IMyBase
		{
			public string Output { get; private set; } = "";
			public int DoWork()
			{
				Output += "did something\n";
				return 1;
			}
		}

		public interface IMyService
		{
			int DoSomething(IMyBase value);
		}
	}
}
