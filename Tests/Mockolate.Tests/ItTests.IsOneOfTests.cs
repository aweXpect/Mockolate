using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsOneOfTests
	{
		[Fact]
		public async Task ToString_WithNullableIntValues_ShouldReturnExpectedValue()
		{
			IParameter<int?> sut = It.IsOneOf<int?>(3, null, 5);
			string expectedValue = "It.IsOneOf(3, null, 5)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithStringValues_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.IsOneOf("foo", "bar");
			string expectedValue = "It.IsOneOf(\"foo\", \"bar\")";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(1, false)]
		[InlineData(4, false)]
		[InlineData(5, true)]
		[InlineData(6, true)]
		[InlineData(7, true)]
		[InlineData(8, false)]
		[InlineData(-5, false)]
		[InlineData(42, false)]
		public async Task WithValue_ShouldMatchWhenEqualToAny(int value, bool expectMatch)
		{
			IParameter<int> sut = It.IsOneOf(5, 6, 7);

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Fact]
		public async Task WithValue_ShouldSupportCovarianceInSetup()
		{
			IMyService mock = Mock.Create<IMyService>();
			MyImplementation value1 = new();
			MyImplementation value2 = new();
			MyOtherImplementation other1 = new();
			mock.SetupMock.Method.DoSomething(It.IsOneOf(value1, value2))
				.Returns(3);

			int result1 = mock.DoSomething(value1);
			int result2 = mock.DoSomething(other1);

			await That(result1).IsEqualTo(3);
			await That(result2).IsEqualTo(0);
		}

		[Fact]
		public async Task WithValue_ShouldSupportCovarianceInVerify()
		{
			IMyService mock = Mock.Create<IMyService>();
			mock.SetupMock.Method.DoSomething(It.Satisfies<MyImplementation>(_ => true))
				.Do(d => d.DoWork())
				.Returns(3);
			MyImplementation value1 = new();
			MyImplementation value2 = new();
			MyOtherImplementation other1 = new();
			MyOtherImplementation other2 = new();

			int result1 = mock.DoSomething(value1);

			await That(mock.VerifyMock.Invoked.DoSomething(It.IsOneOf(value1, value2))).Once();
			await That(value1.Progress).IsEqualTo(1);
			await That(result1).IsEqualTo(3);
			await That(mock.VerifyMock.Invoked.DoSomething(It.IsOneOf(other1, other2))).Never();
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
