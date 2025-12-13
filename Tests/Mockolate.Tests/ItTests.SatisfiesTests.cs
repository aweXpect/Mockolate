using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class SatisfiesTests
	{
		[Fact]
		public async Task ToString_WithPredicate_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Satisfies<string>(x => x.Length == 3);
			string expectedValue = "It.Satisfies<string>(x => x.Length == 3)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData(1, false)]
		public async Task WithMatching_CheckForNull_ShouldMatchForExpectedResult(int? value, bool expectedResult)
		{
			IParameter<int?> sut = It.Satisfies<int?>(v => v is null);

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectedResult);
		}

		[Theory]
		[InlineData(42L)]
		[InlineData("foo")]
		public async Task WithMatching_DifferentType_ShouldNotMatch(object? value)
		{
			IParameter<int?> sut = It.Satisfies<int?>(_ => true);

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsFalse();
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task WithMatching_ShouldMatchForExpectedResult(bool predicateValue)
		{
			IParameter<string> sut = It.Satisfies<string>(_ => predicateValue);

			bool result = ((IParameter)sut).Matches("foo");

			await That(result).IsEqualTo(predicateValue);
		}

		[Fact]
		public async Task WithPredicate_ShouldSupportCovarianceInSetup()
		{
			IMyService mock = Mock.Create<IMyService>();
			mock.SetupMock.Method.DoSomething(It.Satisfies<MyImplementation>(_ => true))
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
			mock.SetupMock.Method.DoSomething(It.Satisfies<MyImplementation>(_ => true))
				.Do(d => d.DoWork())
				.Returns(3);
			MyImplementation value1 = new();

			int result1 = mock.DoSomething(value1);

			await That(mock.VerifyMock.Invoked.DoSomething(It.Satisfies<MyImplementation>(p => p.Progress > 0))).Once();
			await That(mock.VerifyMock.Invoked.DoSomething(It.Satisfies<MyImplementation>(p => p.Progress > 1))).Never();
			await That(value1.Progress).IsEqualTo(1);
			await That(result1).IsEqualTo(3);
			await That(mock.VerifyMock.Invoked.DoSomething(It.Satisfies<MyOtherImplementation>(_ => true))).Never();
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
