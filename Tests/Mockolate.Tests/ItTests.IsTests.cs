using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsTests
	{
		[Theory]
		[InlineData(1, false)]
		[InlineData(5, true)]
		[InlineData(-5, false)]
		[InlineData(42, false)]
		public async Task ShouldMatchWhenEqual(int value, bool expectMatch)
		{
			IParameter<int> sut = It.Is(5);

			bool result = ((IParameterMatch<int>)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Fact]
		public async Task ShouldSupportCovarianceInSetup()
		{
			IMyService sut = IMyService.CreateMock();
			MyImplementation value1 = new();
			MyOtherImplementation value2 = new();
			sut.Mock.Setup.DoSomething(It.Is(value1))
				.Returns(3);

			int result1 = sut.DoSomething(value1);
			int result2 = sut.DoSomething(value2);

			await That(result1).IsEqualTo(3);
			await That(result2).IsEqualTo(0);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Is("foo");
			string expectedValue = "\"foo\"";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithComparer_ShouldReturnExpectedValue()
		{
			IParameter<int> sut = It.Is(4).Using(new AllEqualComparer());
			string expectedValue = "It.Is(4).Using(new AllEqualComparer())";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(-42)]
		public async Task WithComparer_ShouldUseComparer(int value)
		{
			IParameter<int> sut = It.Is(5).Using(new AllEqualComparer());

			bool result = ((IParameterMatch<int>)sut).Matches(value);

			await That(result).IsTrue();
		}

		[Theory]
		[InlineData(-11, false)]
		[InlineData(-10, true)]
		[InlineData(0, true)]
		[InlineData(10, true)]
		[InlineData(11, false)]
		public async Task Within_DateTime_ShouldMatchWhenWithinTolerance(int offsetInSeconds, bool expectMatch)
		{
			DateTime expected = new(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc);
			IParameter<DateTime> sut = It.Is(expected).Within(TimeSpan.FromSeconds(10));

			bool result = ((IParameterMatch<DateTime>)sut).Matches(expected.AddSeconds(offsetInSeconds));

			await That(result).IsEqualTo(expectMatch);
		}

		[Theory]
		[InlineData(4.85, false)]
		[InlineData(4.9, true)]
		[InlineData(5.0, true)]
		[InlineData(5.1, true)]
		[InlineData(5.15, false)]
		public async Task Within_Decimal_ShouldMatchWhenWithinTolerance(double value, bool expectMatch)
		{
			IParameter<decimal> sut = It.Is(5.0m).Within(0.1m);

			bool result = ((IParameterMatch<decimal>)sut).Matches((decimal)value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Theory]
		[InlineData(4.85, false)]
		[InlineData(4.9, true)]
		[InlineData(5.0, true)]
		[InlineData(5.1, true)]
		[InlineData(5.15, false)]
		public async Task Within_Double_ShouldMatchWhenWithinTolerance(double value, bool expectMatch)
		{
			IParameter<double> sut = It.Is(5.0).Within(0.1);

			bool result = ((IParameterMatch<double>)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Theory]
		[InlineData(4.85f, false)]
		[InlineData(4.9f, true)]
		[InlineData(5.0f, true)]
		[InlineData(5.1f, true)]
		[InlineData(5.15f, false)]
		public async Task Within_Float_ShouldMatchWhenWithinTolerance(float value, bool expectMatch)
		{
			IParameter<float> sut = It.Is(5.0f).Within(0.1f);

			bool result = ((IParameterMatch<float>)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Fact]
		public async Task Within_NegativeTolerance_ShouldThrowArgumentOutOfRangeException()
		{
			await That(() => It.Is(5.0).Within(-0.1)).Throws<ArgumentOutOfRangeException>()
				.WithParamName("tolerance");
			await That(() => It.Is(5.0f).Within(-0.1f)).Throws<ArgumentOutOfRangeException>()
				.WithParamName("tolerance");
			await That(() => It.Is(5.0m).Within(-0.1m)).Throws<ArgumentOutOfRangeException>()
				.WithParamName("tolerance");
			await That(() => It.Is(DateTime.UtcNow).Within(TimeSpan.FromSeconds(-1)))
				.Throws<ArgumentOutOfRangeException>().WithParamName("tolerance");
			await That(() => It.Is(TimeSpan.Zero).Within(TimeSpan.FromSeconds(-1)))
				.Throws<ArgumentOutOfRangeException>().WithParamName("tolerance");
		}

		[Fact]
		public async Task Within_ShouldVerifyMockInvocation()
		{
			IMyServiceWithNullable sut = IMyServiceWithNullable.CreateMock();

			sut.DoSomethingWithDouble(4.95);

			await That(sut.Mock.Verify.DoSomethingWithDouble(It.Is(5.0).Within(0.1))).Once();
			await That(sut.Mock.Verify.DoSomethingWithDouble(It.Is(5.0).Within(0.01))).Never();
		}

		[Theory]
		[InlineData(-11, false)]
		[InlineData(-10, true)]
		[InlineData(0, true)]
		[InlineData(10, true)]
		[InlineData(11, false)]
		public async Task Within_TimeSpan_ShouldMatchWhenWithinTolerance(int offsetInSeconds, bool expectMatch)
		{
			TimeSpan expected = TimeSpan.FromMinutes(3);
			IParameter<TimeSpan> sut = It.Is(expected).Within(TimeSpan.FromSeconds(10));

			bool result = ((IParameterMatch<TimeSpan>)sut).Matches(expected + TimeSpan.FromSeconds(offsetInSeconds));

			await That(result).IsEqualTo(expectMatch);
		}

		[Fact]
		public async Task Within_ToString_ShouldReturnExpectedValue()
		{
			double tolerance = 0.1;
			IParameter<double> sut = It.Is(5.0).Within(tolerance);
			string expectedValue = "It.Is(5.0).Within(tolerance)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
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
