using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsTests
	{
		[Fact]
		public async Task ToString_WithValue_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Is("foo");
			string expectedValue = "\"foo\"";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithValueWithComparer_ShouldReturnExpectedValue()
		{
			IParameter<int> sut = It.Is(4).Using(new AllEqualComparer());
			string expectedValue = "It.Is(4).Using(new AllEqualComparer())";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(1, false)]
		[InlineData(5, true)]
		[InlineData(-5, false)]
		[InlineData(42, false)]
		public async Task WithValue_ShouldMatchWhenEqual(int value, bool expectMatch)
		{
			IParameter<int> sut = It.Is(5);

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Fact]
		public async Task WithValue_ShouldSupportCovarianceInSetup()
		{
			IMyService mock = Mock.Create<IMyService>();
			MyImplementation value1 = new();
			MyOtherImplementation value2 = new();
			mock.SetupMock.Method.DoSomething(It.Is(value1))
				.Returns(3);

			int result1 = mock.DoSomething(value1);
			int result2 = mock.DoSomething(value2);

			await That(result1).IsEqualTo(3);
			await That(result2).IsEqualTo(0);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(-42)]
		public async Task WithValue_WithComparer_ShouldUseComparer(int value)
		{
			IParameter<int> sut = It.Is(5).Using(new AllEqualComparer());

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
