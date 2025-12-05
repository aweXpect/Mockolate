using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class MatchTests
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

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectedResult);
		}

		[Theory]
		[InlineData(42L)]
		[InlineData("foo")]
		public async Task WithMatching_DifferentType_ShouldNotMatch(object? value)
		{
			IParameter<int?> sut = With<int?>(_ => true);

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsFalse();
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task WithMatching_ShouldMatchForExpectedResult(bool predicateValue)
		{
			IParameter<string> sut = With<string>(_ => predicateValue);

			bool result = ((IParameter)sut).Matches("foo");

			await That(result).IsEqualTo(predicateValue);
		}

		[Fact]
		public async Task WithPredicate_ShouldSupportCovarianceInSetup()
		{
			IMyService mock = Mock.Create<IMyService>();
			mock.SetupMock.Method.DoSomething(With<MyImplementation>(_ => true))
				.Returns(3);
			MyImplementation value1 = new();
			MyOtherImplementation value2 = new();

			int result1 = mock.DoSomething(value1);
			int result2 = mock.DoSomething(value2);

			await That(result1).IsEqualTo(3);
			await That(result2).IsEqualTo(0);
		}

		[Fact]
		public async Task WithPredicate_ShouldSupportCovarianceInVerify()
		{
			IMyService mock = Mock.Create<IMyService>();
			mock.SetupMock.Method.DoSomething(With<MyImplementation>(_ => true))
				.Do(d => d.DoWork())
				.Returns(3);
			MyImplementation value1 = new();

			int result1 = mock.DoSomething(value1);

			await That(mock.VerifyMock.Invoked.DoSomething(With<MyImplementation>(p => p.Progress > 0))).Once();
			await That(mock.VerifyMock.Invoked.DoSomething(With<MyImplementation>(p => p.Progress > 1))).Never();
			await That(value1.Progress).IsEqualTo(1);
			await That(result1).IsEqualTo(3);
			await That(mock.VerifyMock.Invoked.DoSomething(With<MyOtherImplementation>(_ => true))).Never();
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData("", false)]
		[InlineData("foo", false)]
		public async Task WithValue_Nullable_ShouldMatchWhenEqual(string? value, bool expectMatch)
		{
			IParameter<string?> sut = Null<string?>();

			bool result = ((IParameter)sut).Matches(value);

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

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Fact]
		public async Task WithValue_ShouldSupportCovarianceInSetup()
		{
			IMyService mock = Mock.Create<IMyService>();
			MyImplementation value1 = new();
			MyOtherImplementation value2 = new();
			mock.SetupMock.Method.DoSomething(With(value1))
				.Returns(3);

			int result1 = mock.DoSomething(value1);
			int result2 = mock.DoSomething(value2);

			await That(result1).IsEqualTo(3);
			await That(result2).IsEqualTo(0);
		}

		[Fact]
		public async Task WithValue_ShouldSupportCovarianceInVerify()
		{
			IMyService mock = Mock.Create<IMyService>();
			mock.SetupMock.Method.DoSomething(With<MyImplementation>(_ => true))
				.Do(d => d.DoWork())
				.Returns(3);
			MyImplementation value1 = new();
			MyOtherImplementation value2 = new();

			int result1 = mock.DoSomething(value1);

			await That(mock.VerifyMock.Invoked.DoSomething(With(value1))).Once();
			await That(value1.Progress).IsEqualTo(1);
			await That(result1).IsEqualTo(3);
			await That(mock.VerifyMock.Invoked.DoSomething(With(value2))).Never();
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(-42)]
		public async Task WithValue_WithComparer_ShouldUseComparer(int value)
		{
			IParameter<int> sut = With(5, new AllEqualComparer());

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsTrue();
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
