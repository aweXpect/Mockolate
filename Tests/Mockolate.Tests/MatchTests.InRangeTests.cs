namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class InRangeTests
	{
		[Theory]
		[InlineData(3, 5, false)]
		[InlineData(3, 6, true)]
		[InlineData(2, 4, false)]
		[InlineData(5, 5, false)]
		[InlineData(5, 6, false)]
		[InlineData(4, 6, true)]
		[InlineData(6, 7, false)]
		public async Task ExecuteWith5_Exclusive_ShouldMatchWhenRangeContains5(int minimum, int maximum,
			bool expectFound)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithInt(5);

			await That(mock.VerifyMock.Invoked.DoSomethingWithInt(InRange(minimum, maximum).Exclusive()))
				.Exactly(expectFound ? 1 : 0);
		}

		[Theory]
		[InlineData(3, 5, true)]
		[InlineData(3, 6, true)]
		[InlineData(2, 4, false)]
		[InlineData(5, 5, true)]
		[InlineData(5, 6, true)]
		[InlineData(4, 6, true)]
		[InlineData(6, 7, false)]
		public async Task ExecuteWith5_ShouldMatchWhenRangeContains5(int minimum, int maximum, bool expectFound)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithInt(5);

			await That(mock.VerifyMock.Invoked.DoSomethingWithInt(InRange(minimum, maximum)))
				.Exactly(expectFound ? 1 : 0);
		}

		[Fact]
		public async Task ToString_Exclusive_ShouldReturnExpectedValue()
		{
			IParameter<double> sut = InRange(1.1, 4.1).Exclusive();
			string expectedValue = "InRange(1.1, 4.1).Exclusive()";

			string result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_Inclusive_ShouldReturnExpectedValue()
		{
			IParameter<int> sut = InRange(1, 4);
			string expectedValue = "InRange(1, 4)";

			string result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task WithMinimumGreaterThanMaximum_ShouldThrowArgumentOutOfRangeException()
		{
			void Act()
			{
				_ = InRange(4, 3);
			}

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithParamName("maximum").And
				.WithMessage("The maximum value must be greater than or equal to the minimum value.").AsPrefix();
		}
	}
}
