using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsTests
	{
		[Test]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.Is("foo");
			string expectedValue = "\"foo\"";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Test]
		public async Task ToString_WithComparer_ShouldReturnExpectedValue()
		{
			IParameter<int> sut = It.Is(4).Using(new AllEqualComparer());
			string expectedValue = "It.Is(4).Using(new AllEqualComparer())";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Test]
		[Arguments(1, false)]
		[Arguments(5, true)]
		[Arguments(-5, false)]
		[Arguments(42, false)]
		public async Task ShouldMatchWhenEqual(int value, bool expectMatch)
		{
			IParameter<int> sut = It.Is(5);

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Test]
		public async Task ShouldSupportCovarianceInSetup()
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

		[Test]
		[Arguments(1)]
		[Arguments(5)]
		[Arguments(-42)]
		public async Task WithComparer_ShouldUseComparer(int value)
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
